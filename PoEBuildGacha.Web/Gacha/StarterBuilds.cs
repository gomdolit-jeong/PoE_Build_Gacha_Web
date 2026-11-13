namespace PoEBuildGacha.Web.Gacha;

// tools/crawl_starter_builds.py 가 만드는 wwwroot/data/starter-builds.json 의 모양입니다.

public sealed record StarterBuildData(
    string? UpdatedAt,
    Season? Season,
    MobalyticsList? Mobalytics,
    NinjaList? Ninja);

/// <summary>공식 패치 노트에서 읽은 이번 리그 (예: 3.30 Covens of Umbra)</summary>
public sealed record Season(string Version, string Name)
{
    public override string ToString() => $"{Version} {Name}";
}

public sealed record MobalyticsList(string Source, MobalyticsBuild[] Builds);

public sealed record MobalyticsBuild(
    string Name,
    string? Class,
    string? Ascendancy,
    string? Patch,
    string? Author,
    int Favorites,
    string? UpdatedAt,
    string Url);

public sealed record NinjaList(
    string Source,
    string League,
    string LeagueUrl,
    int Characters,
    NinjaEntry[] Top);

public sealed record NinjaEntry(string Ascendancy, string Skill, double Percentage, int Trend);
