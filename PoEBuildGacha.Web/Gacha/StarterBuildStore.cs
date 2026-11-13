using System.Net.Http.Json;
using System.Text.Json;

namespace PoEBuildGacha.Web.Gacha;

/// <summary>
/// starter-builds.json 을 한 번만 받아서 여러 화면이 같이 씁니다.
/// </summary>
public sealed class StarterBuildStore(HttpClient http)
{
    static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    Task<StarterBuildData?>? loading;

    public Task<StarterBuildData?> GetAsync() => loading ??= LoadAsync();

    async Task<StarterBuildData?> LoadAsync()
    {
        try
        {
            // GitHub Pages 캐시 때문에 예전 목록이 보이지 않도록 매번 새로 받습니다.
            return await http.GetFromJsonAsync<StarterBuildData>(
                $"data/starter-builds.json?t={DateTime.UtcNow.Ticks}", Options);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>뽑은 결과와 맞는 추천 가이드: 같은 스킬을 쓰는 가이드 먼저, 그다음 같은 전직 가이드</summary>
    public static (MobalyticsBuild[] SameSkill, MobalyticsBuild[] SameAscendancy) Match(StarterBuildData? data, GachaResult result)
    {
        var builds = data?.Mobalytics?.Builds ?? [];
        var sameSkill = builds
            .Where(b => b.Name.Contains(result.Skill.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(b => string.Equals(b.Ascendancy, result.Ascendancy.Name, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(b => b.Favorites)
            .ToArray();
        var sameAscendancy = builds
            .Where(b => string.Equals(b.Ascendancy, result.Ascendancy.Name, StringComparison.OrdinalIgnoreCase))
            .Except(sameSkill)
            .OrderByDescending(b => b.Favorites)
            .ToArray();
        return (sameSkill, sameAscendancy);
    }
}
