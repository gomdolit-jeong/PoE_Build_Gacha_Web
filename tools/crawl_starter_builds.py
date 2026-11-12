"""이번 리그 추천 스타터 빌드를 모아 starter-builds.json으로 저장합니다.

출처 (둘 다 robots.txt에서 허용된 경로만 씁니다)
- Mobalytics: 검증된(Verified) 스타터 빌드 목록 (https://mobalytics.gg/poe/starter-builds)
- poe.ninja: 현재 리그에서 실제로 많이 하는 전직 + 스킬 순위

Maxroll은 robots.txt에서 자동 수집을 금지하므로 쓰지 않습니다.

한쪽 출처가 실패하면 기존 파일의 해당 부분을 그대로 둡니다.
사용법: python tools/crawl_starter_builds.py [출력 경로]
"""

from __future__ import annotations

import json
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime, timezone
from pathlib import Path

DEFAULT_OUT = Path(__file__).resolve().parent.parent / "PoEBuildGacha.Web" / "wwwroot" / "data" / "starter-builds.json"
USER_AGENT = "PoE-Build-Gacha fan site (https://github.com/gomdolit-jeong/PoE_Build_Gacha_Web)"

MOBALYTICS_API = "https://mobalytics.gg/api/poe/v1/graphql/query"
MOBALYTICS_PAGE = "https://mobalytics.gg/poe/starter-builds"
MOBALYTICS_QUERY = """query PoeStFilterDocumentContentQuery($input: PoeStructDocumentInputBySlug!) {
  game: poe { documents { structDocumentBySlug(input: $input) { error data { content: contentV2 {
    ... on NgfDocumentStWidgetDocumentsDiscoveryV1 { id data { discovery { ... on NgfUserGeneratedDocumentsDiscovery {
      items { error pageInfo { hasMoreItems cursor total } documents {
        slugifiedName updatedAt
        tags { data { groupSlug name } }
        data { name }
        author { name user { username } }
        favorites { favoritesCount }
      } }
    } } } }
  } } } } }
}"""

NINJA_INDEX = "https://poe.ninja/poe1/api/data/build-index-state"


def fetch_json(req: urllib.request.Request, tries: int = 3) -> dict:
    # 일시적인 네트워크 오류는 잠깐 쉬었다가 다시 시도합니다.
    for attempt in range(1, tries + 1):
        try:
            with urllib.request.urlopen(req, timeout=30) as res:
                return json.load(res)
        except urllib.error.URLError:
            if attempt == tries:
                raise
            time.sleep(3 * attempt)
    raise AssertionError("unreachable")


def post_json(url: str, body: dict) -> dict:
    return fetch_json(urllib.request.Request(
        url,
        data=json.dumps(body).encode("utf-8"),
        headers={"content-type": "application/json", "user-agent": USER_AGENT},
    ))


def get_json(url: str) -> dict:
    return fetch_json(urllib.request.Request(url, headers={"user-agent": USER_AGENT}))


def mobalytics_page(widget_id: str | None, cursor: str | None) -> list[dict]:
    variables: dict = {"input": {"slug": "starter-builds"}}
    if widget_id is not None:
        variables["input"]["widgetsOverride"] = [{
            "widgetID": widget_id,
            "NgfDocumentStWidgetDocumentsDiscoveryV1": {
                "name": None, "sortBy": None, "publishedTimeframe": None, "tags": None, "cursor": cursor,
            },
        }]
    result = post_json(MOBALYTICS_API, {"query": MOBALYTICS_QUERY, "variables": variables})
    if result.get("errors"):
        raise RuntimeError(result["errors"][0].get("message"))
    content = result["data"]["game"]["documents"]["structDocumentBySlug"]["data"]["content"]
    # 빌드 목록이 들어 있는 위젯만 남깁니다.
    return [c for c in content if c and (c.get("data") or {}).get("discovery", {}).get("items")]


def crawl_mobalytics(limit: int = 40) -> dict:
    widgets = mobalytics_page(None, None)
    if not widgets:
        raise RuntimeError("빌드 목록 위젯을 찾지 못했습니다")
    widget = widgets[0]
    widget_id = widget["id"]
    items = widget["data"]["discovery"]["items"]

    builds: list[dict] = []
    while True:
        for doc in items["documents"]:
            tags: dict[str, list[str]] = {}
            for tag in doc["tags"]["data"]:
                tags.setdefault(tag["groupSlug"], []).append(tag["name"])
            username = ((doc.get("author") or {}).get("user") or {}).get("username")
            builds.append({
                "name": doc["data"]["name"].strip(),
                "class": (tags.get("class") or [None])[0],
                "ascendancy": (tags.get("ascendancy") or [None])[0],
                "patch": (tags.get("patch") or [None])[0],
                "author": (doc.get("author") or {}).get("name"),
                "favorites": (doc.get("favorites") or {}).get("favoritesCount", 0),
                "updatedAt": doc.get("updatedAt"),
                "url": f"https://mobalytics.gg/poe/profile/{username}/builds/{doc['slugifiedName']}" if username else MOBALYTICS_PAGE,
            })
        page_info = items["pageInfo"]
        if not page_info["hasMoreItems"] or len(builds) >= limit:
            break
        widgets = [w for w in mobalytics_page(widget_id, page_info["cursor"]) if w["id"] == widget_id]
        if not widgets:
            break
        items = widgets[0]["data"]["discovery"]["items"]

    return {"source": MOBALYTICS_PAGE, "builds": builds[:limit]}


def crawl_ninja() -> dict:
    leagues = get_json(NINJA_INDEX)["leagueBuilds"]
    # 진행 중인 챌린지 리그 (소프트코어) 중 캐릭터가 가장 많은 리그 = 일반 트레이드 리그
    current = [l for l in leagues if l.get("category") == 0 and l.get("status") == 1 and not l.get("hardcore")]
    if not current:
        raise RuntimeError("진행 중인 리그를 찾지 못했습니다")
    league = max(current, key=lambda l: l.get("total", 0))
    return {
        "source": f"https://poe.ninja/poe1/builds/{league['leagueUrl']}",
        "league": league["leagueName"],
        "leagueUrl": league["leagueUrl"],
        "characters": league.get("total", 0),
        "top": [
            {
                "ascendancy": s["class"],
                "skill": s["skill"],
                "percentage": round(s["percentage"], 2),
                "trend": s.get("trend", 0),
            }
            for s in league.get("statistics", [])
        ],
    }


def main() -> int:
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_OUT
    previous = json.loads(out.read_text(encoding="utf-8")) if out.exists() else {}
    data = dict(previous)
    ok = 0

    for key, crawl in (("mobalytics", crawl_mobalytics), ("ninja", crawl_ninja)):
        try:
            data[key] = crawl()
            ok += 1
            print(f"{key}: OK")
        except Exception as e:  # 한쪽이 실패해도 다른 쪽은 갱신합니다.
            print(f"{key}: 실패 - {e} (기존 데이터 유지)")

    if ok == 0:
        print("모든 출처가 실패해서 파일을 바꾸지 않습니다.")
        return 1

    data["updatedAt"] = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"저장: {out}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
