using System;
using System.Collections.Generic;
using System.Linq;

namespace ArpgNameRandomizer
{
    /// <summary>Word lists for one group-chat nickname mood.</summary>
    class Vibe
    {
        public string Name;
        public string[] Adjectives;   // Sleepy, Feral
        public string[] Nouns;        // Potato, Gremlin
        public string[] OfWhat;       // "Queen of ___": Naps, Chaos
        public string[] Doers;        // "Professional ___": Overthinker
        public override string ToString() => Name;
    }

    static class ChatData
    {
        static string[] S(string words) => words.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToArray();

        public static readonly Vibe[] Vibes =
        {
            new Vibe { Name = "Funny / chaos",
                Adjectives = S("Chaotic, Feral, Unhinged, Dramatic, Suspicious, Confused, Sneaky, Loud, Unbothered, Questionable, Wobbly, Rogue"),
                Nouns = S("Gremlin, Goblin, Raccoon, Potato, Pigeon, Toaster, Duck, Banana, Menace, Goose, Possum, Clown"),
                OfWhat = S("Chaos, Drama, Memes, Bad Decisions, Typos, Plot Twists, Hot Takes"),
                Doers = S("Overthinker, Meme Dealer, Drama Starter, Typo Maker, Chaos Coordinator, Hot Take Haver") },

            new Vibe { Name = "Cute",
                Adjectives = S("Tiny, Fluffy, Smol, Sunny, Soft, Bubbly, Sweet, Little, Cozy, Sparkly, Squishy, Baby"),
                Nouns = S("Bunny, Mochi, Kitten, Peach, Dumpling, Panda, Muffin, Bean, Cupcake, Duckling, Bun, Hamster"),
                OfWhat = S("Hugs, Sunshine, Stickers, Sparkles, Good Vibes, Cuddles"),
                Doers = S("Hug Giver, Sticker Sender, Cuddler, Heart Reactor, Compliment Giver") },

            new Vibe { Name = "Food",
                Adjectives = S("Spicy, Crispy, Cheesy, Salty, Saucy, Crunchy, Hungry, Extra, Midnight, Buttery, Toasted, Sticky"),
                Nouns = S("Noodle, Taco, Dumpling, Boba, Nugget, Pancake, Burrito, Waffle, Fries, Mochi, Pickle, Meatball"),
                OfWhat = S("Snacks, Noodles, Leftovers, Bubble Tea, Midnight Snacks, Second Helpings"),
                Doers = S("Snack Thief, Food Critic, Noodle Slurper, Menu Reader, Fridge Raider, Dessert Hunter") },

            new Vibe { Name = "Sleepy / lazy",
                Adjectives = S("Sleepy, Lazy, Snoozing, Offline, Yawning, Tired, Comfy, Slow, Horizontal, Napping, Drowsy, Grumpy"),
                Nouns = S("Sloth, Koala, Pillow, Blanket, Potato, Cat, Panda, Turtle, Couch, Snail, Bear, Pajama"),
                OfWhat = S("Naps, Pillows, Seen Messages, Late Replies, Snooze Buttons, Five More Minutes"),
                Doers = S("Napper, Procrastinator, Ghoster, Seen Leaver, Lurker, Snooze Presser, Late Replier") },

            new Vibe { Name = "Gamer",
                Adjectives = S("AFK, Lagging, Tryhard, Noob, Clutch, Salty, Respawning, Cracked, Casual, Sweaty, Glitched, Speedrun"),
                Nouns = S("Healer, Tank, Camper, NPC, Noob, Gremlin, Goblin, Sidekick, Minion, Speedrunner, Sniper, Loot Goblin"),
                OfWhat = S("Loot, Lag, Respawns, Side Quests, Rage Quits, Loading Screens"),
                Doers = S("Button Masher, Rage Quitter, Loot Hoarder, Backseat Gamer, Lag Blamer, Patch Note Reader") },

            new Vibe { Name = "Cool",
                Adjectives = S("Iron, Shadow, Midnight, Golden, Savage, Silent, Electric, Legendary, Smooth, Ice, Neon, Wild"),
                Nouns = S("Wolf, Tiger, Phoenix, Shark, Falcon, Viper, Legend, Ghost, Storm, Dragon, Panther, Rebel"),
                OfWhat = S("Storms, Shadows, Thunder, Speed, Style, Sunglasses"),   // not "the Night": "Lady of the Night" reads badly
                Doers = S("Trendsetter, Vibe Curator, Heartbreaker, Night Rider, Rule Breaker") },
        };

        public static readonly List<Vibe> VibeChoices =
            new[] { new Vibe { Name = "Any vibe" } }.Concat(Vibes).ToList();

        // Gendered titles ("King Noodle") and suffixes ("Meme Lord"); the neutral ones are used for everyone.
        public static readonly string[] TitlesAny = S("Captain, Chief, Professor, Doctor, Agent, General, Boss, Grandmaster, Supreme Leader");
        public static readonly string[] TitlesM = S("King, Lord, Prince, Sir, Mr., Duke, Big Bro");
        public static readonly string[] TitlesF = S("Queen, Lady, Princess, Madam, Miss, Duchess, Big Sis");
        public static readonly string[] RolesAny = S("Boss, Master, Enjoyer, Hunter, Wizard, Legend, Specialist, Expert");
        public static readonly string[] RolesM = S("Lord, King");
        public static readonly string[] RolesF = S("Queen, Lady");
        // "___ of Naps" only reads right after a real rank, not "Mr." or "Big Sis".
        public static readonly string[] OfTitlesAny = S("Captain, Chief, General, Grandmaster, Supreme Leader, Professor");
        public static readonly string[] OfTitlesM = S("King, Lord, Prince, Duke");
        public static readonly string[] OfTitlesF = S("Queen, Lady, Princess, Duchess");
        public static readonly string[] DoerPrefixes = S("Professional, Certified, Chief, Part-time, Retired, Full-time");

        public const string Rules = "Words are joined without spaces (QueenOfNaps, SleepyPotato).";
    }

    /// <summary>All group-chat nicknames for one vibe / gender / length, handed out shuffled without repeats.</summary>
    class ChatPool : INamePool
    {
        readonly List<string> all = new List<string>();

        public ChatPool(Vibe vibe, Gender gender, int maxLen, string prefix)
        {
            prefix = (prefix ?? "").Trim();
            var vibes = vibe.Adjectives == null ? ChatData.Vibes : new[] { vibe };

            string[] titles = Gendered(ChatData.TitlesAny, ChatData.TitlesM, ChatData.TitlesF, gender);
            string[] roles = Gendered(ChatData.RolesAny, ChatData.RolesM, ChatData.RolesF, gender);
            string[] ofTitles = Gendered(ChatData.OfTitlesAny, ChatData.OfTitlesM, ChatData.OfTitlesF, gender);
            // "Any vibe" mixes words across moods too: "Fluffy Shark", "Feral Dumpling".
            var adjectives = vibes.SelectMany(v => v.Adjectives).Distinct().ToArray();
            var nouns = vibes.SelectMany(v => v.Nouns).Distinct().ToArray();
            var ofWhat = vibes.SelectMany(v => v.OfWhat).Distinct().ToArray();
            var doers = vibes.SelectMany(v => v.Doers).Distinct().ToArray();

            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Add(string phrase)
            {
                // Patterns are written as phrases, then joined without spaces, each word capitalised:
                // "Queen of Naps" -> QueenOfNaps, "Part-time Napper" -> PartTimeNapper, "Mr. Mochi" -> MrMochi.
                var words = phrase.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(w => w.Replace(".", ""))
                                  .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1))
                                  .ToArray();
                if (words.Distinct(StringComparer.OrdinalIgnoreCase).Count() < words.Length) return;   // "Boss Boss"
                string name = string.Concat(words);
                if (name.Length > maxLen) return;
                if (prefix.Length > 0 && !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
                if (set.Add(name)) all.Add(name);
            }

            foreach (var a in adjectives) foreach (var n in nouns) Add(a + " " + n);          // Sleepy Potato
            foreach (var t in titles) foreach (var n in nouns) Add(t + " " + n);              // Captain Noodle
            foreach (var n in nouns) foreach (var r in roles) Add(n + " " + r);               // Goblin Queen
            foreach (var a in adjectives) Add("The " + a + " One");                            // The Sleepy One
            foreach (var t in ofTitles) foreach (var o in ofWhat) Add(t + " of " + o);          // Lady of Naps
            foreach (var p in ChatData.DoerPrefixes) foreach (var d in doers) Add(p + " " + d); // Certified Overthinker
        }

        static string[] Gendered(string[] any, string[] m, string[] f, Gender g) =>
            g == Gender.Male ? any.Concat(m).ToArray() :
            g == Gender.Female ? any.Concat(f).ToArray() :
            any.Concat(m).Concat(f).ToArray();

        public long Total(Style s) => all.Count;

        public List<string> Take(int count, Style style, ISet<string> seen, Random rnd)
        {
            var left = all.Where(n => !seen.Contains(n)).ToList();
            var result = new List<string>();
            while (result.Count < count && left.Count > 0)
            {
                int i = rnd.Next(left.Count);
                result.Add(left[i]);
                seen.Add(left[i]);
                left[i] = left[left.Count - 1];
                left.RemoveAt(left.Count - 1);
            }
            return result;
        }
    }
}
