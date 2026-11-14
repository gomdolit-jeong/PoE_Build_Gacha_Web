"""이번 리그 추천 스타터 빌드를 모아 starter-builds.json으로 저장합니다.

출처 (둘 다 robots.txt에서 허용된 경로만 씁니다)
- Mobalytics: 검증된(Verified) 스타터 빌드 목록 (https://mobalytics.gg/poe/starter-builds)
- poe.ninja: 현재 리그에서 실제로 많이 하는 전직 + 스킬 순위
- poedb (https://poedb.tw/kr/): 순위에 나온 스킬·전직의 한국어 이름

공식 사이트의 /api/ 경로는 robots.txt에서 자동 수집을 금지하므로 쓰지 않습니다.

Maxroll은 robots.txt에서 자동 수집을 금지하므로 쓰지 않습니다.

한쪽 출처가 실패하면 기존 파일의 해당 부분을 그대로 둡니다.
사용법: python tools/crawl_starter_builds.py [출력 경로]
"""

from __future__ import annotations

import html
import json
import re
import sys
import time
import urllib.error
import urllib.parse
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

# 공식 패치 노트 게시판 (robots.txt에서 허용된 경로). 가장 최근 "Content Update 3.xx.0" 글 제목에서 리그 이름을 읽습니다.
PATCH_NOTES_FORUM = "https://www.pathofexile.com/forum/view-forum/patch-notes"
POEDB_KR = "https://poedb.tw/kr/"
OG_TITLE = re.compile(r'<meta[^>]+property="og:title"[^>]+content="([^"]+)"')

SEASON_TITLE = re.compile(r"Content Update (\d+\.\d+)\.0\s*(?:—|--|-)\s*Path of Exile:\s*([^<\n]+?)\s*<")


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


def get_text(url: str) -> str:
    req = urllib.request.Request(url, headers={"user-agent": USER_AGENT})
    with urllib.request.urlopen(req, timeout=30) as res:
        return res.read().decode("utf-8", errors="replace")


def crawl_season() -> dict:
    page = get_text(PATCH_NOTES_FORUM)
    m = SEASON_TITLE.search(page)
    if not m:
        raise RuntimeError("패치 노트 게시판에서 리그 이름을 찾지 못했습니다")
    return {"version": m.group(1), "name": html.unescape(m.group(2)).strip(), "source": PATCH_NOTES_FORUM}


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


def is_season_league(league_name: str, season_name: str | None) -> bool:
    """poe.ninja 리그 이름(예: Allflame)의 단어가 이번 리그 이름(예: Curse of the Allflame)에 들어 있는지"""
    if not season_name:
        return False
    words = [w for w in re.split(r"\W+", league_name) if len(w) > 2]
    return any(w.lower() in season_name.lower() for w in words)


def crawl_ninja(season_name: str | None = None) -> dict:
    leagues = get_json(NINJA_INDEX)["leagueBuilds"]
    # 챌린지 리그(소프트코어)만 봅니다. 같은 리그의 SSF·Ruthless보다 일반 트레이드 리그가 캐릭터가 가장 많습니다.
    challenge = [l for l in leagues if l.get("category") == 0 and not l.get("hardcore") and l.get("statistics")]

    # 1) 이번 리그 통계가 생겼으면 그것을 씁니다. 새 리그 초반에는 지난 리그가 아직 '진행 중'으로 남아 있고
    #    캐릭터 수도 더 많아서, 이름으로 먼저 찾아야 이번 리그로 넘어갑니다.
    # 2) 아직 없으면 진행 중인 리그 중 가장 큰 리그(보통 지난 리그)를 씁니다.
    season = [l for l in challenge if is_season_league(l["leagueName"], season_name)]
    current = [l for l in challenge if l.get("status") == 1]
    candidates = season or current
    if not candidates:
        raise RuntimeError("통계가 있는 리그를 찾지 못했습니다")
    league = max(candidates, key=lambda l: l.get("total", 0))
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


def crawl_korean_names(data: dict, known: dict) -> dict:
    """순위에 나온 스킬·전직의 한국어 이름을 poedb에서 찾습니다. 이미 아는 이름은 다시 받지 않습니다."""
    names = dict(known)
    wanted = set()
    for t in (data.get("ninja") or {}).get("top", []):
        wanted.update(n for n in (t.get("skill"), t.get("ascendancy")) if n)
    for b in (data.get("mobalytics") or {}).get("builds", []):
        if b.get("ascendancy"):
            wanted.add(b["ascendancy"])
    for name in sorted(wanted - names.keys()):
        try:
            page = get_text(POEDB_KR + urllib.parse.quote(name.replace(" ", "_")))
            m = OG_TITLE.search(page)
            korean = html.unescape(m.group(1)).strip() if m else ""
            # 한글이 없으면 (번역 없음·없는 페이지) 저장하지 않습니다.
            if re.search(r"[가-힣]", korean):
                names[name] = korean
            time.sleep(0.5)
        except Exception as e:
            print(f"  한국어 이름 못 찾음: {name} - {e}")
    return names


def main() -> int:
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_OUT
    previous = json.loads(out.read_text(encoding="utf-8")) if out.exists() else {}
    data = dict(previous)
    ok = 0

    crawlers = (
        ("season", crawl_season),
        ("mobalytics", crawl_mobalytics),
        ("ninja", lambda: crawl_ninja((data.get("season") or {}).get("name"))),
    )
    for key, crawl in crawlers:
        try:
            data[key] = crawl()
            ok += 1
            print(f"{key}: OK")
        except Exception as e:  # 한쪽이 실패해도 다른 쪽은 갱신합니다.
            print(f"{key}: 실패 - {e} (기존 데이터 유지)")

    try:
        data["koreanNames"] = crawl_korean_names(data, previous.get("koreanNames") or {})
        print(f"koreanNames: {len(data['koreanNames'])}개")
    except Exception as e:
        print(f"koreanNames: 실패 - {e}")

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
