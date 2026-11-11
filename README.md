# PoE Build Gacha

"운명 뽑기" 버튼을 누르면 주사위가 굴러가고, Path of Exile 1의 직업, 전직, 주요 스킬 젬을 무작위로 뽑아 주는 재미용 웹 앱입니다.

**👉 바로 가기: https://gomdolit-jeong.github.io/PoE_Build_Gacha_Web/**

- 누를 때마다 완전히 새로 뽑습니다.
- **프레시아 리그** 체크 시 Legacy of Phrecia 전직(19종)에서 뽑고, 이동기도 추천합니다.
- PoE 2는 개발 예정입니다.

## 실행

```bash
dotnet run --project PoEBuildGacha.Web --launch-profile http
```

`main`에 push하면 GitHub Actions가 GitHub Pages로 배포합니다 (Settings → Pages → Source: GitHub Actions).

## 구조

- `PoEBuildGacha.Web/Gacha/PoeData.cs` — 직업·전직·스킬·이동기 목록
- `PoEBuildGacha.Web/Gacha/BuildGacha.cs` — 무작위 뽑기
- `PoEBuildGacha.Web/Pages/Home.razor` — 화면과 주사위 효과

This is a fan project, not affiliated with Grinding Gear Games.
