namespace PoEBuildGacha.Web.Gacha;

public enum SkillKind { Attack, Spell, Minion }

public sealed record Skill(string Name, string Korean, SkillKind Kind);

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
/// 일반 전직은 3.30 (Covens of Umbra) 기준, 프레시아 전직은 Legacy of Phrecia 이벤트의 19종입니다.
/// 한글 이름은 한국 클라이언트 표기(poedb.tw/kr)를 따릅니다.
/// 리그에서 더 이상 얻을 수 없는 젬은 넣지 않습니다 (스탠다드에만 남은 젬 포함).
/// 공식 패치 노트 3.8.0~3.30.0 기준으로 확인했고, 3.25.0에서 삭제된 선대의 대전사(Ancestral Warchief)를 뺐습니다.
/// https://www.pathofexile.com/forum/view-forum/patch-notes
/// </summary>
public static class PoeData
{
    static Skill A(string name, string korean) => new(name, korean, SkillKind.Attack);
    static Skill S(string name, string korean) => new(name, korean, SkillKind.Spell);
    static Skill M(string name, string korean) => new(name, korean, SkillKind.Minion);
    static Ascendancy Asc(string name, string korean) => new(name, korean);

    public static readonly PoeClass[] Classes =
    [
        new("Marauder", "머라우더", "#c0392b",
            [Asc("Juggernaut", "저거넛"), Asc("Berserker", "버서커"), Asc("Chieftain", "치프틴")],
            [Asc("Ancestral Commander", "앤세스트럴 커맨더"), Asc("Behemoth", "베히모스"), Asc("Antiquarian", "앤티쿼리언")],
            [A("Boneshatter", "뼈 박살"), A("Earthquake", "지진"), A("Sunder", "산산조각"),
             A("Ground Slam", "대지 강타"), A("Molten Strike", "용암 타격"), A("Earthshatter", "지면 분쇄"),
             A("Shield Crush", "방패 타쇄"), A("Infernal Blow", "지옥불 맹타"), A("Rage Vortex", "격노 소용돌이"),
             A("Volcanic Fissure", "화산 균열"), S("Holy Flame Totem", "신성한 화염 토템")]),

        new("Duelist", "듀얼리스트", "#d35400",
            [Asc("Slayer", "슬레이어"), Asc("Gladiator", "글래디에이터"), Asc("Champion", "챔피언")],
            [Asc("Gambler", "갬블러"), Asc("Paladin", "팔라딘"), Asc("Aristocrat", "아리스토크랫")],
            [A("Cyclone", "회오리바람"), A("Lacerate", "피부 찢기"), A("Splitting Steel", "강철 분할"),
             A("Blade Flurry", "질풍의 칼날"), A("Perforate", "천공"), A("Bladestorm", "칼날 폭풍"),
             A("Lightning Strike", "번개 타격"), A("Spectral Throw", "환영 무기 투척"),
             A("Double Strike", "이중 타격"), A("Smite", "징벌"), A("Reave", "약탈")]),

        new("Ranger", "레인저", "#27ae60",
            [Asc("Deadeye", "데드아이"), Asc("Warden", "워든"), Asc("Pathfinder", "패스파인더")],
            [Asc("Daughter of Oshabi", "도터 오브 오샤비"), Asc("Whisperer", "위스퍼러"), Asc("Wildspeaker", "와일드스피커")],
            [A("Lightning Arrow", "번개 화살"), A("Tornado Shot", "회오리 사격"), A("Ice Shot", "얼음 화살"),
             A("Toxic Rain", "맹독성 비"), A("Caustic Arrow", "부식성 화살"), A("Rain of Arrows", "화살비"),
             A("Elemental Hit", "원소의 일격"), A("Galvanic Arrow", "충격 화살"), A("Kinetic Blast", "역학 폭발"),
             A("Split Arrow", "분할 화살"), A("Explosive Arrow", "폭발 화살"),
             A("Poisonous Concoction", "독성 혼합물")]),

        new("Shadow", "쉐도우", "#2980b9",
            [Asc("Assassin", "어쌔신"), Asc("Saboteur", "사보추어"), Asc("Trickster", "트릭스터")],
            [Asc("Servant of Arakaali", "서번트 오브 아라칼리"), Asc("Blind Prophet", "블라인드 프로펫"), Asc("Surfcaster", "서프캐스터")],
            [S("Blade Vortex", "칼날 소용돌이"), S("Ethereal Knives", "천상의 단도"), A("Cobra Lash", "코브라 채찍"),
             A("Venom Gyre", "독액 선회"), S("Bladefall", "칼날비"), S("Explosive Trap", "폭발 덫"),
             S("Lightning Trap", "번개 덫"), S("Pyroclast Mine", "화산탄 지뢰"), S("Seismic Trap", "지진 덫"),
             S("Essence Drain", "정수 흡수"), A("Kinetic Bolt", "역학 투사체"), A("Viper Strike", "맹독 타격")]),

        new("Witch", "위치", "#8e44ad",
            [Asc("Necromancer", "네크로맨서"), Asc("Elementalist", "엘리멘탈리스트"), Asc("Occultist", "오컬티스트")],
            [Asc("Harbinger", "하빈저"), Asc("Herald", "헤럴드"), Asc("Bog Shaman", "보그 샤먼")],
            [S("Arc", "연쇄 번개"), S("Fireball", "화염구"), S("Spark", "전기불꽃"), S("Firestorm", "화염 폭풍"),
             S("Vortex", "소용돌이"), S("Ice Nova", "얼음 폭발"), M("Raise Zombie", "좀비 소환"),
             M("Summon Skeletons", "해골 소환"), M("Raise Spectre", "망령 소환"),
             M("Summon Raging Spirit", "격노의 유령 소환"), S("Bane", "파멸"), S("Detonate Dead", "시체 폭발")]),

        new("Templar", "템플러", "#c9a227",
            [Asc("Inquisitor", "인퀴지터"), Asc("Hierophant", "하이로펀트"), Asc("Guardian", "가디언")],
            [Asc("Architect of Chaos", "아키텍트 오브 카오스"), Asc("Polytheist", "폴리시이스트"), Asc("Puppeteer", "퍼페티어")],
            [S("Righteous Fire", "정의의 화염"), S("Storm Brand", "폭풍의 낙인"), S("Armageddon Brand", "종말의 낙인"),
             S("Penance Brand", "속죄의 낙인"), S("Divine Ire", "신성한 진노"), S("Shockwave Totem", "충격파 토템"),
             S("Ball Lightning", "구형 번개"), S("Purifying Flame", "정화의 불길"), M("Absolution", "면죄"),
             S("Holy Flame Totem", "신성한 화염 토템"), A("Smite", "징벌"), S("Arc", "연쇄 번개")]),

        new("Scion", "사이온", "#7f8c8d",
            [Asc("Ascendant", "어센던트"), Asc("Reliquarian", "렐리쿼리언"), Asc("Luminary", "루미너리")],
            [Asc("Scavenger", "스캐빈저")],
            [A("Cyclone", "회오리바람"), A("Lightning Arrow", "번개 화살"), S("Arc", "연쇄 번개"),
             S("Righteous Fire", "정의의 화염"), A("Boneshatter", "뼈 박살"),
             M("Summon Raging Spirit", "격노의 유령 소환"), A("Toxic Rain", "맹독성 비"), S("Ice Nova", "얼음 폭발"),
             A("Spectral Helix", "영체 나선"), A("Kinetic Blast", "역학 폭발"), A("Flicker Strike", "점멸 타격"),
             S("Blade Vortex", "칼날 소용돌이")]),
    ];

    // 이동기 (프레시아 리그에서만 추천). 직업 무기에 맞춰 고릅니다.
    static readonly Skill LeapSlam = A("Leap Slam", "도약 강타");
    static readonly Skill Dash = S("Dash", "질주");
    static readonly Skill FlameDash = S("Flame Dash", "화염 질주");
    static readonly Skill Frostblink = S("Frostblink", "서리점멸");
    static readonly Skill WhirlingBlades = A("Whirling Blades", "칼날 선회");
    static readonly Skill ShieldCharge = A("Shield Charge", "방패 돌진");
    static readonly Skill LightningWarp = S("Lightning Warp", "번개 차원 이동");
    static readonly Skill BlinkArrow = A("Blink Arrow", "점멸 화살");
    static readonly Skill SmokeMine = S("Smoke Mine", "연막 지뢰");
    static readonly Skill Bodyswap = S("Bodyswap", "신체 전환");
    static readonly Skill WitheringStep = S("Withering Step", "고사시키는 걸음");

    static readonly Dictionary<string, Skill[]> MovementByClass = new()
    {
        ["Marauder"] = [LeapSlam, ShieldCharge, Dash, FlameDash, Frostblink],
        ["Duelist"] = [WhirlingBlades, LeapSlam, ShieldCharge, Dash, Frostblink],
        ["Ranger"] = [BlinkArrow, WhirlingBlades, Dash, FlameDash, Frostblink],
        ["Shadow"] = [WhirlingBlades, SmokeMine, WitheringStep, Dash, FlameDash, Frostblink],
        ["Witch"] = [FlameDash, Frostblink, Bodyswap, LightningWarp, Dash],
        ["Templar"] = [FlameDash, Frostblink, LightningWarp, LeapSlam, Dash],
        ["Scion"] = [Dash, FlameDash, Frostblink, WhirlingBlades, LeapSlam, BlinkArrow],
    };

    public static Skill[] MovementSkills(PoeClass poeClass) => MovementByClass[poeClass.Name];

    // 영문 이름 → 한글 이름 (스타터 빌드 목록에서 씁니다). 모르는 이름은 그대로 둡니다.
    static readonly Dictionary<string, string> KoreanNames = BuildKoreanNames();

    static Dictionary<string, string> BuildKoreanNames()
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in Classes)
        {
            names[c.Name] = c.Korean;
            foreach (var a in c.Ascendancies.Concat(c.PhreciaAscendancies)) names[a.Name] = a.Korean;
            foreach (var s in c.Skills) names[s.Name] = s.Korean;
        }
        foreach (var s in MovementByClass.Values.SelectMany(x => x)) names[s.Name] = s.Korean;
        return names;
    }

    public static string? Korean(string? english) =>
        english is not null && KoreanNames.TryGetValue(english, out var korean) ? korean : null;

    /// <summary>전직 이름으로 직업을 찾습니다 (일반·프레시아 전직 모두).</summary>
    public static PoeClass? ClassOfAscendancy(string? ascendancy) =>
        Classes.FirstOrDefault(c => c.Ascendancies.Concat(c.PhreciaAscendancies)
            .Any(a => string.Equals(a.Name, ascendancy, StringComparison.OrdinalIgnoreCase)));

    public static string KindLabel(SkillKind kind) => kind switch
    {
        SkillKind.Attack => "공격",
        SkillKind.Spell => "주문",
        _ => "소환",
    };
}
