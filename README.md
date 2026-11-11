# PoE Build Gacha

닉네임을 입력하면 Path of Exile 1의 직업, 전직, 주요 스킬 젬을 뽑아 주는 재미용 웹 앱입니다.

**👉 바로 가기: https://gomdolit-jeong.github.io/PoE_Build_Gacha_Web/**

- 같은 닉네임은 항상 같은 결과가 나옵니다 (FNV-1a 해시, 대소문자·앞뒤 공백 무시).
- **프레시아 리그** 체크 시 Legacy of Phrecia 전직(19종)에서 뽑고, 이동기도 추천합니다.
- PoE 2는 개발 예정입니다.

## 실행

```bash
dotnet run --project PoEBuildGacha.Web --launch-profile http
```

`main`에 push하면 GitHub Actions가 GitHub Pages로 배포합니다 (Settings → Pages → Source: GitHub Actions).

## 구조

- `PoEBuildGacha.Web/Gacha/PoeData.cs` — 직업·전직·스킬 목록
- `PoEBuildGacha.Web/Gacha/BuildGacha.cs` — 닉네임 해시 → 결과
- `PoEBuildGacha.Web/Pages/Home.razor` — 화면

This is a fan project, not affiliated with Grinding Gear Games.
