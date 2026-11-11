using System.Collections.Generic;
using System.Linq;

namespace NameRandomizer
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
            Starts = S("Gor Kra Thro Bru Grom Dur Rag Hro Ulf Bor Vor Kar Thu Grak Mor Hal Skar Tor Bra Rog Haz Brak Drog Gul Krom Mag Org Rok Stur Thar Ung Var Yor Zug Bal Dag Grun Hag Jor Kol Nar Ruk Sven Tyr Ux Bjar Har"),
            Mids = S("ga ru ka do ma tha gri bo ra du ko lo"),
            EndsM = S("ak gar rok nar dur mund grim thar ok ulf rik gorn an ath ag ug or ush orm ek un ald arn ost"),
            EndsF = S("a ga hild ra run dis ka rid wyn ja gerd laug dra na thra unn ey frid"),
            WordA = S("Skull Blood Iron War Axe Bone Rage Storm Stone Grim Red Wolf Bear Doom Ash Gore Thunder Scar Hell Rust Flesh Bull Boar Ruin Black Steel Gut Brute Ox Troll Giant Ogre Gravel Flint Crag Bronze Battle Siege Spike Horn Tusk Tooth Raw Cinder Grizzly Boulder Chain Brawl Mammoth Granite"),
            WordB = S("splitter breaker crusher hide fist maw cleaver hammer born howl tusk jaw brand rend fang guard mane ripper smasher bane horn slayer skull wall brow roar gash back chewer eater stomper bellow grip hand heart bash mauler ram gore crown banner quake sunder tooth spine wrath scar"),
        };

        static readonly Theme Blade = new Theme
        {
            Name = "Blade",
            Starts = S("Ves Cal Dar Ren Lio Mar Sev Ard Cass Teo Val Ezr Lu Ros Fen Sil Kas Cor Dom Rafe Ad Bel Cy Dan Ed Gil Jac Leo Ny Or Pell Quin Ral Sam Tav Vic Wil Zan Aur Bas Elo Mir"),
            Mids = S("ri lo ve ta ni ra le mo sa di"),
            EndsM = S("ian ric ius an en ar ello ov as ard io ain iel on ero ent iano ias ot ence ez or"),
            EndsF = S("a ia ine ette ra elle issa ana ira essa ena ora ise ella ina eya ara isse"),
            WordA = S("Steel Silver Crimson Quick Duel Fell Keen Gilt Scar Bright Edge Sharp Iron Swift Fair Gold Rose True Last High Red Bold Lion Ruby Azure Velvet Dagger Saber Feint Glint Gleam Grace Proud Free Wind Quill Ace Ivory Brass Copper Noble Dusk Star"),
            WordB = S("blade edge lash parry strike brand mark wind step thrust spur cut guard dancer point song heart wing flash hilt foil sworn sabre feather glint wit grace rush dart stride lunge sash spark stance rose crest vow riposte"),
        };

        static readonly Theme Wild = new Theme
        {
            Name = "Wild",
            Starts = S("Ael Syl Thal Lyr Ari Fen Ny El Cael Rhi Tam Isil Ro Wren Bri Ash Eir Bryn Ced Dryn Eo Fae Gwen Hollin Ivo Ka Mae Nim Oran Pip Quell Tal Ula Vyn Yew Ryn Saf Lin Ore"),
            Mids = S("la wy ri tha ve na lo mi do fe"),
            EndsM = S("an wyn dor ric en as ion orn ith eth ar ell avan in ros iel und orin al"),
            EndsF = S("a wyn ra ia elle iss ara enne lia ae wen ina ira yn aya ila ora"),
            WordA = S("Thorn Oak Moss Hawk Wolf Fern Wild Briar Raven Storm Elk Bramble Hollow Sky Swift Fox Pine Elm Stag Owl Dew Sun Ivy Willow Lynx Falcon Cedar Birch Rowan Heather Thistle River Brook Hare Badger Bear Moon Dawn Leaf Green Glen Meadow Antler Wren Sparrow Rain Bark"),
            WordB = S("arrow bough song shot fletch glade wood wind runner stalker leaf horn feather tracker root bow strider whisper mane hunter claw path bloom dancer eye quill trail fang call sight heart born ward cloak hide howl seeker keeper warden singer walker"),
        };

        static readonly Theme Shadow = new Theme
        {
            Name = "Shadow",
            Starts = S("Vex Nyx Sha Kae Zel Ves Ix Mor Sly Rav Cy Zor Ky Vy Nez Ish Az Bly Dex Esk Fyx Jyn Kez Lyx Myr Nok Ryk Vael Xan Zyl Sin Tes Nyl Rhe"),
            Mids = S("ra li sa ne vi ky zo ri"),
            EndsM = S("is ex ar ius an el ith oth ryn ix ek az os yr en un as ik"),
            EndsF = S("a ys ira ess ae yne ix ara eth issa iel yra eni ova iss ysa"),
            WordA = S("Night Shade Shadow Silent Black Dusk Ghost Veil Smoke Hollow Grey Mist Murk Venom Raven Ink Dark Hush Soot Crow Moon Blind Quiet Nether Coal Tar Pitch Ash Hidden Masked Whisper Gloom Dim Faint Sly Thief Viper Spider Web Rat Fog Cinder Quick"),
            WordB = S("blade step walker cloak fang knife whisper stalker veil sting cut mark eye thorn dagger shade slip mask hand hood lurker needle wisp coil stab trick runner blink shiv spike prowl sneak grin glare stitch hook wire smile skulk"),
        };

        static readonly Theme Dark = new Theme
        {
            Name = "Dark",
            Starts = S("Mor Vel Nec Xal Azh Lil Zar Bel Mal Ker Ves Dra Thra Ul Sar Sere Ny Vor Gal Bael Ez Fel Isk Jha Kul Lor Naz Oth Pyr Tel Uth Vyr Xer Yth Zeth Mord Sev"),
            Mids = S("ga ze ro ma the ri no xa ve lu"),
            EndsM = S("oth ath ius ex ar akar oroth eth ul rus vain gul agor ezar ion ok oz rath aul ix"),
            EndsF = S("ith a ys ira ene essa iah ora yx ianne ethra ysse ara ina ova elith isra"),
            WordA = S("Bone Grave Rot Blood Hex Dread Soul Corpse Wither Crypt Plague Ash Death Gloom Pale Vile Woe Night Ruin Hollow Black Bile Sorrow Doom Shade Grief Ghoul Lich Tomb Carrion Marrow Leech Blight Fester Wraith Bleak Dusk Raven Mourn Cinder Ichor Gall Venom Skull Hag"),
            WordB = S("caller weaver binder monger wraith shroud maw thrall born song curse reaver mourn spawn bane eater whisper scourge mire grip touch rite vein harrow gaze kin feeder speaker keeper howl kiss wail gloom chant mark cradle grasp lantern bloom ward hex shade"),
        };

        static readonly Theme Holy = new Theme
        {
            Name = "Holy",
            Starts = S("Aur Sol Cel Ser Ald Gal Theo Bene Luc Ore Rho Mira Isa Hel Ama Cas Ade Clar Dav Ev Fid Gab Hon Jul Leon Mae Nath Oli Pax Rae Seb Tob Ure Vin Zeph Ansel"),
            Mids = S("li ra ve di la mi ne ro"),
            EndsM = S("ian ius ric ar ael ion ard en os us aric elm old ert an im ias"),
            EndsF = S("a iel ine ana elle ia ora essa ise abel ette ina ena iva ara ira aya"),
            WordA = S("Dawn Light Sun Holy Bright Glory Faith Gold Oath Saint Halo Valor Star Sacred Pure White Lion Hope Grace Truth Silver High Day Ivory Crown Choir Altar Temple Heaven Mercy Honor Noble Glimmer Sky Morning Shield Lily Dove Gleam Amber Pearl"),
            WordB = S("shield bringer guard hammer keeper ward born sworn mantle blade crest heart brand herald light crown wing mace song oath lance aegis grace vow beacon bearer flame hope wall path star call mark glow banner"),
        };

        static readonly Theme Arcane = new Theme
        {
            Name = "Arcane",
            Starts = S("Ael Zy Ith Myr Xe Ora Lys Esh Ka Ve Ry Sel Thy Ilu Cyr Za Nim Aru Fey Gyr Jael Kyr Lir Nyr Ost Pha Quor Rhe Sae Tir Wyl Yv Zir"),
            Mids = S("la ri the sa vo ne mi ze lo"),
            EndsM = S("ion ar ex aris en oth us ander yr ian elm is oran eus ix aen"),
            EndsF = S("ra ia ys ara ene ielle iss ena yne ira essa ova ithra ynn aeth ella"),
            WordA = S("Storm Frost Flame Ember Star Rune Spell Void Arc Spark Glyph Ice Sky Moon Aether Astral Mana Mystic Thunder Prism Echo Sun Fate Dream Crystal Night Comet Nova Ether Lumen Quartz Mirror Chaos Time Sigil Cinder Blaze Tempest Static Gale Aura Ink Shadow"),
            WordB = S("weaver caller born fire flare bolt glass song shard binder mind light sight veil storm spire flux gaze ward pulse scribe mancer tide wisp glow rift caster seer sage smith wright wing tongue eye heart cipher lore gate mark script"),
        };

        static readonly Theme Spirit = new Theme
        {
            Name = "Spirit",
            Starts = S("Shen Kai Ren Tao Jin Ry Ama Sora Hal Lan Mei Kel Tem Ura Zan Isa Ai Bao Chi Dai En Fu Gen Hiro Jun Kou Lin Min Nao Ori Po Rin Shu Tai Wei Yu"),
            Mids = S("ka mi to ra na shi ro yu"),
            EndsM = S("o an en ji ra u ar ten shin ki ro kan sei to jin mon"),
            EndsF = S("a i ra mi ya na ri ka lei ne ko yu mei rin sa"),
            WordA = S("Iron Still Storm Jade Crane Tiger Eagle Spirit Wind River Thunder Calm Lotus Serpent Jaguar Mist Ember Dragon Willow Cloud Ox Silent Sun Moon Wolf Sky Stone Bamboo Cherry Koi Mountain Ocean Phoenix Quiet Monkey Mantis Panda Rain Silk Temple Wave Peach Pine Bronze"),
            WordB = S("palm fist stride heart soul step kick breath sworn bloom born strike claw sky mantle dancer sage path flow whisper tide spirit echo leaf wave guard fang wing gaze will mind way focus form roar dance hand root"),
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
            "Limits: PoE 3-23, Last Epoch ~20, Diablo IV ~12 and letters only (no numbers). " +
            "Not checked in game: the D4 / LE limits, and numbers in PoE / LE.";

        static string[] S(string words) => words.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        static GameClass C(string name, params Theme[] themes) => new GameClass { Name = name, Themes = themes };
    }
}
