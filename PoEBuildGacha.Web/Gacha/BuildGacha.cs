using System.Text;

namespace PoEBuildGacha.Web.Gacha;

public sealed record GachaResult(
    string Nickname,
    bool Phrecia,
    PoeClass Class,
    Ascendancy Ascendancy,
    Skill Skill);

/// <summary>
/// 닉네임을 해시해서 직업·전직·주요 스킬을 고릅니다.
/// 같은 닉네임은 언제, 어느 기기에서 뽑아도 같은 결과가 나옵니다.
/// (string.GetHashCode는 실행할 때마다 바뀌므로 쓰지 않고 FNV-1a를 직접 계산합니다.)
/// </summary>
public static class BuildGacha
{
    public static string Normalize(string nickname) => nickname.Trim().ToLowerInvariant();

    public static GachaResult Roll(string nickname, bool phrecia)
    {
        var key = Normalize(nickname);
        var league = phrecia ? "phrecia" : "standard";

        // 직업은 리그와 상관없이 닉네임만으로 정하고, 전직과 스킬은 리그에 따라 달라집니다.
        var poeClass = Pick(PoeData.Classes, Hash($"{key}|class"));
        var ascPool = phrecia ? poeClass.PhreciaAscendancies : poeClass.Ascendancies;
        var ascendancy = Pick(ascPool, Hash($"{key}|ascendancy|{league}"));
        var skill = Pick(poeClass.Skills, Hash($"{key}|skill|{league}"));

        return new GachaResult(nickname.Trim(), phrecia, poeClass, ascendancy, skill);
    }

    static T Pick<T>(T[] items, ulong hash) => items[(int)(hash % (ulong)items.Length)];

    /// <summary>FNV-1a 64비트 해시</summary>
    public static ulong Hash(string text)
    {
        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;

        var hash = offset;
        foreach (var b in Encoding.UTF8.GetBytes(text))
        {
            hash ^= b;
            hash *= prime;
        }

        // 비슷한 닉네임끼리 결과가 몰리지 않도록 비트를 한 번 더 섞습니다 (splitmix64 마무리 단계).
        hash ^= hash >> 30;
        hash *= 0xBF58476D1CE4E5B9UL;
        hash ^= hash >> 27;
        hash *= 0x94D049BB133111EBUL;
        hash ^= hash >> 31;
        return hash;
    }
}
