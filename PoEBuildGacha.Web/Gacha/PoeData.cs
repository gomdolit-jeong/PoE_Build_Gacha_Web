namespace PoEBuildGacha.Web.Gacha;

public enum SkillKind { Attack, Spell, Minion }

public sealed record Skill(string Name, SkillKind Kind);

public sealed record Ascendancy(string Name, string Korean);

public sealed record PoeClass(
    string Name,
    string Korean,
    string Color,
    Ascendancy[] Ascendancies,
    Ascendancy[] PhreciaAscendancies,
    Skill[] Skills);

/// <summary>
/// PoE1 직업·전직·주요 스킬 젬 목록.
/// 일반 전직은 3.28 기준, 프레시아 전직은 Legacy of Phrecia 이벤트의 19종입니다.
/// </summary>
public static class PoeData
{
    static Skill A(string name) => new(name, SkillKind.Attack);
    static Skill S(string name) => new(name, SkillKind.Spell);
    static Skill M(string name) => new(name, SkillKind.Minion);
    static Ascendancy Asc(string name, string korean) => new(name, korean);

    public static readonly PoeClass[] Classes =
    [
        new("Marauder", "머라우더", "#c0392b",
            [Asc("Juggernaut", "저거넛"), Asc("Berserker", "버서커"), Asc("Chieftain", "치프틴")],
            [Asc("Ancestral Commander", "선조 사령관"), Asc("Behemoth", "베헤모스"), Asc("Antiquarian", "앤티쿼리언")],
            [A("Boneshatter"), A("Earthquake"), A("Sunder"), A("Ground Slam"), A("Molten Strike"),
             A("Earthshatter"), A("Shield Crush"), A("Infernal Blow"), A("Rage Vortex"),
             A("Volcanic Fissure"), S("Holy Flame Totem"), A("Ancestral Warchief")]),

        new("Duelist", "듀얼리스트", "#d35400",
            [Asc("Slayer", "슬레이어"), Asc("Gladiator", "글래디에이터"), Asc("Champion", "챔피언")],
            [Asc("Gambler", "갬블러"), Asc("Paladin", "팔라딘"), Asc("Aristocrat", "아리스토크랫")],
            [A("Cyclone"), A("Lacerate"), A("Splitting Steel"), A("Blade Flurry"), A("Perforate"),
             A("Bladestorm"), A("Lightning Strike"), A("Spectral Throw"), A("Double Strike"),
             A("Smite"), A("Reave"), A("Shield Charge")]),

        new("Ranger", "레인저", "#27ae60",
            [Asc("Deadeye", "데드아이"), Asc("Warden", "워든"), Asc("Pathfinder", "패스파인더")],
            [Asc("Daughter of Oshabi", "오샤비의 딸"), Asc("Whisperer", "위스퍼러"), Asc("Wildspeaker", "와일드스피커")],
            [A("Lightning Arrow"), A("Tornado Shot"), A("Ice Shot"), A("Toxic Rain"), A("Caustic Arrow"),
             A("Rain of Arrows"), A("Elemental Hit"), A("Galvanic Arrow"), A("Kinetic Blast"),
             A("Split Arrow"), A("Explosive Arrow"), A("Poisonous Concoction")]),

        new("Shadow", "섀도우", "#2980b9",
            [Asc("Assassin", "어쌔신"), Asc("Saboteur", "사보추어"), Asc("Trickster", "트릭스터")],
            [Asc("Servant of Arakaali", "아라칼리의 종"), Asc("Blind Prophet", "눈먼 예언자"), Asc("Surfcaster", "서프캐스터")],
            [S("Blade Vortex"), S("Ethereal Knives"), A("Cobra Lash"), A("Venom Gyre"), S("Bladefall"),
             S("Explosive Trap"), S("Lightning Trap"), S("Pyroclast Mine"), S("Seismic Trap"),
             S("Essence Drain"), S("Kinetic Bolt"), A("Viper Strike")]),

        new("Witch", "위치", "#8e44ad",
            [Asc("Necromancer", "네크로맨서"), Asc("Elementalist", "엘리멘탈리스트"), Asc("Occultist", "오컬티스트")],
            [Asc("Harbinger", "하빈저"), Asc("Herald", "헤럴드"), Asc("Bog Shaman", "늪 주술사")],
            [S("Arc"), S("Fireball"), S("Spark"), S("Firestorm"), S("Vortex"), S("Ice Nova"),
             M("Raise Zombie"), M("Summon Skeletons"), M("Raise Spectre"), M("Summon Raging Spirit"),
             S("Bane"), S("Detonate Dead")]),

        new("Templar", "템플러", "#c9a227",
            [Asc("Inquisitor", "인퀴지터"), Asc("Hierophant", "하이로펀트"), Asc("Guardian", "가디언")],
            [Asc("Architect of Chaos", "혼돈의 설계자"), Asc("Polytheist", "다신교도"), Asc("Puppeteer", "퍼펫티어")],
            [S("Righteous Fire"), S("Storm Brand"), S("Armageddon Brand"), S("Penance Brand"),
             S("Divine Ire"), S("Shockwave Totem"), S("Ball Lightning"), S("Purifying Flame"),
             M("Absolution"), S("Holy Flame Totem"), A("Smite"), S("Arc")]),

        new("Scion", "사이온", "#7f8c8d",
            [Asc("Ascendant", "어센던트"), Asc("Reliquarian", "렐리쿼리언")],
            [Asc("Scavenger", "스캐빈저")],
            [A("Cyclone"), A("Lightning Arrow"), S("Arc"), S("Righteous Fire"), A("Boneshatter"),
             M("Summon Raging Spirit"), A("Toxic Rain"), S("Ice Nova"), A("Spectral Helix"),
             A("Kinetic Blast"), A("Flicker Strike"), S("Blade Vortex")]),
    ];

    public static string KindLabel(SkillKind kind) => kind switch
    {
        SkillKind.Attack => "공격",
        SkillKind.Spell => "주문",
        _ => "소환",
    };
}
