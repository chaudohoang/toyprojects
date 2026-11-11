using System.Collections.Generic;
using System.Linq;

namespace ArpgNameRandomizer
{
    /// <summary>A flavour pool: syllables for made-up names, word halves for compound names.</summary>
    class Theme
    {
        public string Name;
        public string[] Starts;   // name openers, e.g. "Mor", "Kael"
        public string[] Mids;     // optional middle syllable
        public string[] EndsM;    // masculine-sounding endings
        public string[] EndsF;    // feminine-sounding endings
        public string[] WordA;    // compound first half, e.g. "Bone"
        public string[] WordB;    // compound second half, e.g. "caller"
    }

    class GameClass
    {
        public string Name;
        public Theme[] Themes;
        public override string ToString() => Name;
    }

    static class NameData
    {
        static readonly Theme Brutal = new Theme
        {
            Name = "Brutal",
            Starts = S("Gor Kra Thro Bru Grom Dur Rag Hro Ulf Bor Vor Kar Thu Grak Mor Hal Skar Tor Bra Rog Haz Brak"),
            Mids = S("ga ru ka do ma tha gri"),
            EndsM = S("ak gar rok nar dur mund grim thar ok ulf rik gorn an ath"),
            EndsF = S("a ga hild ra run dis ka rid wyn ja"),
            WordA = S("Skull Blood Iron War Axe Bone Rage Storm Stone Grim Red Wolf Bear Doom Ash Gore Thunder Scar Hell Rust Flesh Bull Boar Ruin Black Steel Gut Brute"),
            WordB = S("splitter breaker crusher hide fist maw cleaver hammer born howl tusk jaw brand rend fang guard mane ripper smasher bane horn slayer skull wall brow roar gash back"),
        };

        static readonly Theme Blade = new Theme
        {
            Name = "Blade",
            Starts = S("Ves Cal Dar Ren Lio Mar Sev Ard Cass Teo Val Ezr Lu Ros Fen Sil Kas Cor Dom Rafe"),
            Mids = S("ri lo ve ta ni"),
            EndsM = S("ian ric ius an en ar ello ov as ard io ain"),
            EndsF = S("a ia ine ette ra elle issa ana ira"),
            WordA = S("Steel Silver Crimson Quick Duel Fell Keen Gilt Scar Bright Edge Sharp Iron Swift Fair Gold Rose True Last High Red Bold Lion"),
            WordB = S("blade edge lash parry strike brand mark wind step thrust spur cut guard dancer point song heart wing flash hilt foil sworn sabre"),
        };

        static readonly Theme Wild = new Theme
        {
            Name = "Wild",
            Starts = S("Ael Syl Thal Lyr Ari Fen Ny El Cael Rhi Tam Isil Ro Wren Bri Ash Eir"),
            Mids = S("la wy ri tha ve"),
            EndsM = S("an wyn dor ric en as ion orn ith eth"),
            EndsF = S("a wyn ra ia elle iss ara enne lia"),
            WordA = S("Thorn Oak Moss Hawk Wolf Fern Wild Briar Raven Storm Elk Bramble Hollow Sky Swift Fox Pine Elm Stag Owl Dew Sun Ivy Willow Lynx Falcon Cedar"),
            WordB = S("arrow bough song shot fletch glade wood wind runner stalker leaf horn feather tracker root bow strider whisper mane hunter claw path bloom dancer eye quill"),
        };

        static readonly Theme Shadow = new Theme
        {
            Name = "Shadow",
            Starts = S("Vex Nyx Sha Kae Zel Ves Ix Mor Sly Rav Cy Zor Ky Vy Nez Ish"),
            Mids = S("ra li sa ne"),
            EndsM = S("is ex ar ius an el ith oth ryn ix"),
            EndsF = S("a ys ira ess ae yne ix ara eth"),
            WordA = S("Night Shade Shadow Silent Black Dusk Ghost Veil Smoke Hollow Grey Mist Murk Venom Raven Ink Dark Hush Soot Crow Moon Blind Quiet Nether"),
            WordB = S("blade step walker cloak fang knife whisper stalker veil sting cut mark eye thorn dagger shade slip mask hand hood lurker needle wisp coil"),
        };

        static readonly Theme Dark = new Theme
        {
            Name = "Dark",
            Starts = S("Mor Vel Nec Xal Azh Lil Zar Bel Mal Ker Ves Dra Thra Ul Sar Sere Ny Vor Gal"),
            Mids = S("ga ze ro ma the ri"),
            EndsM = S("oth ath ius ex ar akar oroth eth ul rus vain gul"),
            EndsF = S("ith a ys ira ene essa iah ora yx ianne"),
            WordA = S("Bone Grave Rot Blood Hex Dread Soul Corpse Wither Crypt Plague Ash Death Gloom Pale Vile Woe Night Ruin Hollow Black Bile Sorrow Doom Shade Grief"),
            WordB = S("caller weaver binder monger wraith shroud maw thrall born song curse reaver mourn spawn bane eater whisper scourge mire grip touch rite vein harrow gaze kin"),
        };

        static readonly Theme Holy = new Theme
        {
            Name = "Holy",
            Starts = S("Aur Sol Cel Ser Ald Gal Theo Bene Luc Ore Rho Mira Isa Hel Ama Cas"),
            Mids = S("li ra ve di"),
            EndsM = S("ian ius ric ar ael ion ard en os us"),
            EndsF = S("a iel ine ana elle ia ora essa ise"),
            WordA = S("Dawn Light Sun Holy Bright Glory Faith Gold Oath Saint Halo Valor Star Sacred Pure White Lion Hope Grace Truth Silver High Day"),
            WordB = S("shield bringer guard hammer keeper ward born sworn mantle blade crest heart brand herald light crown wing mace song oath lance aegis grace vow beacon"),
        };

        static readonly Theme Arcane = new Theme
        {
            Name = "Arcane",
            Starts = S("Ael Zy Ith Myr Xe Ora Lys Esh Ka Ve Ry Sel Thy Ilu Cyr Za Nim"),
            Mids = S("la ri the sa vo"),
            EndsM = S("ion ar ex aris en oth us ander yr ian"),
            EndsF = S("ra ia ys ara ene ielle iss ena yne ira"),
            WordA = S("Storm Frost Flame Ember Star Rune Spell Void Arc Spark Glyph Ice Sky Moon Aether Astral Mana Mystic Thunder Prism Echo Sun Fate Dream Crystal Night"),
            WordB = S("weaver caller born fire flare bolt glass song shard binder mind light sight veil storm spire flux gaze ward pulse scribe mancer tide wisp glow rift"),
        };

        static readonly Theme Spirit = new Theme
        {
            Name = "Spirit",
            Starts = S("Shen Kai Ren Tao Jin Ry Ama Sora Hal Lan Mei Kel Tem Ura Zan Isa"),
            Mids = S("ka mi to ra"),
            EndsM = S("o an en ji ra u ar ten shin ki"),
            EndsF = S("a i ra mi ya na ri ka lei ne"),
            WordA = S("Iron Still Storm Jade Crane Tiger Eagle Spirit Wind River Thunder Calm Lotus Serpent Jaguar Mist Ember Dragon Willow Cloud Ox Silent Sun Moon Wolf Sky Stone Bamboo"),
            WordB = S("palm fist stride heart soul step kick breath sworn bloom born strike claw sky mantle dancer sage path flow whisper tide spirit echo leaf wave guard"),
        };

        public static readonly Theme[] AllThemes = { Brutal, Blade, Wild, Shadow, Dark, Holy, Arcane, Spirit };

        // One ARPG list for every game: each entry names the matching class in PoE / PoE2 / D4 / Last Epoch.
        // Game name rules differ only in length, which the "Max length" box covers.
        public static readonly List<GameClass> ArpgClasses = new List<GameClass>
        {
            C("Any class", AllThemes),
            C("Warrior / Barbarian / Marauder", Brutal),
            C("Duelist / Mercenary", Blade, Brutal),
            C("Ranger / Huntress", Wild, Blade),
            C("Rogue / Shadow", Shadow, Blade),
            C("Witch / Necromancer / Acolyte", Dark),
            C("Sorceress / Mage", Arcane),
            C("Templar / Paladin / Sentinel", Holy),
            C("Monk / Spiritborn", Spirit),
            C("Druid / Primalist", Wild, Brutal),
        };

        public const string ArpgRules =
            "Name limits: PoE / PoE2 3-23, Last Epoch ~20, Diablo IV ~12 (D4 and LE not checked in game). " +
            "PoE names are global, so short ones are usually taken.";

        static string[] S(string words) => words.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        static GameClass C(string name, params Theme[] themes) => new GameClass { Name = name, Themes = themes };
    }
}
