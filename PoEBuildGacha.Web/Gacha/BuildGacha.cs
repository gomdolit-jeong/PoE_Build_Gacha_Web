namespace PoEBuildGacha.Web.Gacha;

public sealed record GachaResult(
    bool Phrecia,
    PoeClass Class,
    Ascendancy Ascendancy,
    Skill Skill,
    Skill? Movement);

/// <summary>
/// 직업·전직·주요 스킬(프레시아면 이동기까지)을 완전히 무작위로 뽑습니다.
/// </summary>
public static class BuildGacha
{
    public static GachaResult Roll(bool phrecia, Random rng)
    {
        var poeClass = Pick(PoeData.Classes, rng);
        var ascendancy = Pick(phrecia ? poeClass.PhreciaAscendancies : poeClass.Ascendancies, rng);
        var skill = Pick(poeClass.Skills, rng);

        // 이동기는 프레시아 리그에서만 추천합니다.
        var movement = phrecia ? Pick(PoeData.MovementSkills(poeClass), rng) : null;

        return new GachaResult(phrecia, poeClass, ascendancy, skill, movement);
    }

    static T Pick<T>(T[] items, Random rng) => items[rng.Next(items.Length)];
}
