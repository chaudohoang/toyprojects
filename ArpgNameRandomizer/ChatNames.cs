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
                Adjectives = S("Chaotic, Feral, Unhinged, Dramatic, Suspicious, Confused, Sneaky, Loud, Unbothered, Questionable, Wobbly, Rogue, Cursed, Silly, Goofy, Wacky, Clumsy, Bonkers, Spicy, Petty, Sassy, Nosy, Bossy, Grumbly, Squeaky, Zesty, Rowdy, Sus, Bizarre, Shady, Awkward, Random, Sketchy, Cheeky, Absurd, Baffled, Bumbling, Cranky, Daffy, Dizzy, Dorky, Frantic, Frazzled, Fussy, Giddy, Gloopy, Grumpy, Hyper, Jittery, Kooky, Loopy, Lumpy, Madcap, Manic, Meddling, Nutty, Odd, Peculiar, Puzzled, Quirky, Rambunctious, Ridiculous, Rubbery, Scruffy, Shifty, Snarky, Sneezy, Sloppy, Squawking, Startled, Stinky, Twitchy, Unruly, Weird, Wonky, Zany, Bewildered, Derpy, Flailing, Honking, Itchy, Jumpy, Moody, Noisy, Smug, Spooky, Unstable, Bouncy, Baffling, Clueless"),
                Nouns = S("Gremlin, Goblin, Raccoon, Potato, Pigeon, Toaster, Duck, Banana, Menace, Goose, Possum, Clown, Llama, Walrus, Gecko, Moose, Platypus, Weasel, Turnip, Pickle, Seagull, Squirrel, Ferret, Noodle, Yeti, Troll, Gnome, Chicken, Penguin, Lobster, Badger, Rascal, Bandit, Weirdo, Muppet, Armadillo, Baboon, Beetle, Blobfish, Cactus, Camel, Cockatoo, Coconut, Crab, Cricket, Dingo, Dodo, Donkey, Eggplant, Emu, Flamingo, Frog, Gargoyle, Gerbil, Goat, Gorilla, Hippo, Hyena, Iguana, Jellyfish, Kazoo, Lemur, Mongoose, Moth, Narwhal, Ostrich, Parrot, Pelican, Pufferfish, Pumpkin, Rooster, Sasquatch, Skunk, Spatula, Sock, Teapot, Tomato, Toucan, Trombone, Tuba, Turkey, Unicorn, Vulture, Wombat, Zucchini, Bagpipe, Anchovy, Biscuit, Meatloaf, Sprout, Walnut"),
                OfWhat = S("Chaos, Drama, Memes, Bad Decisions, Typos, Plot Twists, Hot Takes, Mischief, Gossip, Nonsense, Shenanigans, Voice Notes, Screenshots, Random Facts, Caps Lock"),
                Doers = S("Overthinker, Meme Dealer, Drama Starter, Typo Maker, Chaos Coordinator, Hot Take Haver, Gossip Collector, Screenshot Taker, Prankster, Troublemaker, Joke Teller, Sticker Spammer, Voice Note Sender") },

            new Vibe { Name = "Cute",
                Adjectives = S("Tiny, Fluffy, Smol, Sunny, Soft, Bubbly, Sweet, Little, Cozy, Sparkly, Squishy, Baby, Cuddly, Fuzzy, Happy, Giggly, Rosy, Dainty, Pastel, Snuggly, Bouncy, Twinkly, Lovely, Gentle, Cheery, Peachy, Honey, Sugar, Pocket, Mini, Chubby, Dreamy, Wiggly, Precious, Adorable, Angelic, Blushing, Bright, Bubblegum, Candy, Charming, Comfy, Cotton, Darling, Delightful, Dimpled, Fairy, Feathery, Fluttery, Friendly, Glowy, Golden, Huggable, Jelly, Joyful, Kind, Lacy, Lavender, Lucky, Merry, Milky, Minty, Moonlit, Nibbly, Peppy, Petite, Plushy, Polka, Puffy, Purring, Rainbow, Shiny, Silky, Smiley, Snowy, Sprinkle, Starry, Strawberry, Sugary, Teeny, Tender, Toasty, Velvet, Waddling, Wee, Whiskered, Winky, Yummy, Cottony, Twinkle"),
                Nouns = S("Bunny, Mochi, Kitten, Peach, Dumpling, Panda, Muffin, Bean, Cupcake, Duckling, Bun, Hamster, Puppy, Otter, Cookie, Marshmallow, Teddy, Chick, Lamb, Fawn, Pudding, Cherry, Daisy, Star, Button, Kitty, Pup, Bear, Pebble, Sprout, Biscuit, Bubble, Petal, Corgi, Angel, Blossom, Buttercup, Candy, Caramel, Cinnamon, Clover, Cocoa, Cub, Cupid, Darling, Dewdrop, Dimple, Dolly, Dove, Duckie, Fairy, Feather, Firefly, Fluff, Gumdrop, Honeybee, Jellybean, Joey, Kiwi, Koala, Ladybug, Lollipop, Lovebug, Macaron, Mittens, Moonbeam, Nugget, Peanut, Penguin, Piglet, Pixie, Plum, Poppy, Pumpkin, Puff, Quokka, Raindrop, Robin, Seal, Snowflake, Snuggle, Sparrow, Sprinkles, Squirrel, Sugarplum, Sunbeam, Sweetpea, Tadpole, Tulip, Whisker"),
                OfWhat = S("Hugs, Sunshine, Stickers, Sparkles, Good Vibes, Cuddles, Rainbows, Smiles, Cookies, Blankets, Hearts, Flowers, Giggles"),
                Doers = S("Hug Giver, Sticker Sender, Cuddler, Heart Reactor, Compliment Giver, Flower Picker, Cookie Baker, Smile Maker, Blanket Sharer, Good Morning Texter") },

            new Vibe { Name = "Food",
                Adjectives = S("Spicy, Crispy, Cheesy, Salty, Saucy, Crunchy, Hungry, Extra, Midnight, Buttery, Toasted, Sticky, Sweet, Sour, Smoky, Garlic, Fried, Grilled, Juicy, Steamy, Frozen, Sugary, Chunky, Glazed, Loaded, Tangy, Zesty, Fluffy, Hot, Minty, Honey, Peppery, Fizzy, Roasted, Baked, Bitter, Boiled, Bubbly, Buttered, Candied, Caramel, Charred, Chewy, Chilled, Chocolate, Cinnamon, Creamy, Crumbly, Curry, Dipped, Doughy, Drizzled, Flaky, Fresh, Fruity, Ginger, Gooey, Greasy, Hearty, Iced, Jammy, Lemon, Lime, Malted, Mashed, Melted, Messy, Milky, Nutty, Pickled, Plump, Poached, Powdered, Puffed, Rich, Ripe, Savory, Scrambled, Seared, Shredded, Sizzling, Smashed, Soggy, Spiced, Stuffed, Sugared, Syrupy, Tasty, Tender, Truffle, Vanilla, Wasabi, Whipped, Yummy"),
                Nouns = S("Noodle, Taco, Dumpling, Boba, Nugget, Pancake, Burrito, Waffle, Fries, Mochi, Pickle, Meatball, Sushi, Ramen, Bagel, Donut, Pretzel, Nacho, Pizza, Burger, Hotdog, Croissant, Muffin, Toast, Biscuit, Cupcake, Brownie, Popcorn, Cookie, Bao, Pho, Kimchi, Churro, Omelette, Sandwich, Apple, Avocado, Bacon, Banana, Bean, Berry, Brisket, Broccoli, Bun, Butter, Cake, Candy, Carrot, Cheddar, Cheese, Cherry, Chili, Chip, Chowder, Cocoa, Coconut, Corn, Crepe, Crouton, Cucumber, Curry, Custard, Egg, Falafel, Fudge, Gelato, Gnocchi, Gravy, Gumbo, Hummus, Jelly, Kebab, Ketchup, Lasagna, Lemon, Lettuce, Macaron, Mango, Melon, Mustard, Oatmeal, Onion, Pasta, Peanut, Pepper, Pie, Pudding, Ravioli, Risotto, Salsa, Samosa, Sausage, Scone, Spaghetti, Tofu, Tortilla, Truffle, Udon, Wonton, Yogurt"),
                OfWhat = S("Snacks, Noodles, Leftovers, Bubble Tea, Midnight Snacks, Second Helpings, Pizza, Desserts, Buffets, Hot Sauce, Takeout, Free Samples, Ramen, Cheese"),
                Doers = S("Snack Thief, Food Critic, Noodle Slurper, Menu Reader, Fridge Raider, Dessert Hunter, Buffet Raider, Pizza Eater, Food Photographer, Taste Tester, Recipe Hoarder, Takeout Orderer, Spice Lover") },

            new Vibe { Name = "Sleepy / lazy",
                Adjectives = S("Sleepy, Lazy, Snoozing, Offline, Yawning, Tired, Comfy, Slow, Horizontal, Napping, Drowsy, Grumpy, Dozy, Groggy, Mellow, Chill, Snoring, Weary, Idle, Sluggish, Relaxed, Late, Loafing, Bedtime, Hibernating, Lounging, Quiet, Sunday, Cozy, Dreamy, Blinking, Blurry, Calm, Cuddled, Curled, Dazed, Dimmed, Dormant, Dreaming, Droopy, Dusky, Easygoing, Exhausted, Fading, Floppy, Foggy, Gentle, Hazy, Heavy, Hushed, Lethargic, Listless, Lowkey, Misty, Muted, Nocturnal, Nodding, Paused, Peaceful, Plush, Pooped, Resting, Restful, Serene, Shuffling, Silent, Slumbering, Snug, Snuggled, Soft, Sprawled, Still, Stretchy, Swaddled, Tucked, Unhurried, Unplugged, Wiped, Woozy, Yawny, Zonked, Zen, Snoozy, Mopey, Bleary, Mushy"),
                Nouns = S("Sloth, Koala, Pillow, Blanket, Potato, Cat, Panda, Turtle, Couch, Snail, Bear, Pajama, Hammock, Duvet, Slipper, Sleeper, Napper, Dormouse, Hedgehog, Seal, Walrus, Capybara, Owl, Moon, Cloud, Lump, Beanbag, Loaf, Sofa, Mattress, Teddy, Kitty, Alarm, Armchair, Bathrobe, Bed, Bolster, Bunk, Burrow, Catnap, Cocoon, Comforter, Cot, Cushion, Daydream, Den, Doze, Dreamer, Fleece, Futon, Hoodie, Lamb, Lantern, Lazybones, Lounger, Lullaby, Marmot, Moonbeam, Nap, Nest, Nightcap, Nightlight, Night Owl, Opossum, Pug, Quilt, Recliner, Robe, Sandman, Siesta, Sleepyhead, Slumber, Snooze, Snorer, Sock, Sunbeam, Tortoise, Wombat, Yawn, Starfish, Manatee, Hound, Bulldog, Lazybug, Snoozer, Pillowfort, Earmuff, Bunny"),
                OfWhat = S("Naps, Pillows, Seen Messages, Late Replies, Snooze Buttons, Five More Minutes, Sleep, Blankets, Weekends, Lazy Sundays, Dreams, Sofas, Excuses, Tomorrow"),
                Doers = S("Napper, Procrastinator, Ghoster, Seen Leaver, Lurker, Snooze Presser, Late Replier, Couch Potato, Blanket Hogger, Dream Chaser, Excuse Maker, Alarm Ignorer") },

            new Vibe { Name = "Gamer",
                Adjectives = S("AFK, Lagging, Tryhard, Noob, Clutch, Salty, Respawning, Cracked, Casual, Sweaty, Glitched, Speedrun, Pixel, Hardcore, Ranked, Unranked, Epic, Critical, Buffed, Nerfed, OP, Laggy, Turbo, Combo, Bonus, Retro, Arcade, Elite, Boosted, Wiped, Agile, Armored, Bugged, Camping, Carried, Charged, Cloaked, Crafted, Crit, Cursed, Dashing, Diamond, Digital, Double, Enchanted, Endgame, Enraged, Equipped, Fast, Final, Frenzied, Godly, Gold, Grinding, Heroic, Hidden, Idle, Infinite, Instant, Leveled, Lucky, Maxed, Mighty, Modded, Mythic, Offline, Online, Pixelated, Platinum, Power, Questing, Raging, Rare, Rerolled, Respawned, Rookie, Rushing, Sneaky, Solo, Spawned, Speedy, Stacked, Stealthy, Streaming, Tactical, Tilted, Ultimate, Unlocked, Upgraded, Veteran, Wired"),
                Nouns = S("Healer, Tank, Camper, NPC, Noob, Gremlin, Goblin, Sidekick, Minion, Speedrunner, Sniper, Loot Goblin, Mage, Rogue, Paladin, Bard, Gamer, Player, Pro, Grinder, Looter, Raider, Support, Carry, Main, Smurf, Bot, Avatar, Knight, Archer, Quester, Alchemist, Assassin, Barbarian, Berserker, Brawler, Builder, Champion, Cleric, Clicker, Controller, Crafter, Druid, Duelist, Dungeon, Explorer, Farmer, Fighter, Gladiator, Golem, Guardian, Gunner, Headset, Hero, Joystick, Keyboard, Lurker, Medic, Miner, Monk, Mouse, Necro, Ninja, Orc, Pixel, Potion, Quest, Ranger, Reaper, Rookie, Scout, Shaman, Slime, Spawn, Squire, Streamer, Striker, Summoner, Thief, Troll, Valkyrie, Warlock, Warrior, Zombie, Turret, Dragon, Dwarf"),
                OfWhat = S("Loot, Lag, Respawns, Side Quests, Rage Quits, Loading Screens, Patch Notes, Grinding, Headshots, Speedruns, Crits, Combos, Pixels, Achievements"),
                Doers = S("Button Masher, Rage Quitter, Loot Hoarder, Backseat Gamer, Lag Blamer, Patch Note Reader, Side Quester, Achievement Hunter, Late Night Gamer, Tutorial Skipper, Cutscene Skipper, Item Hoarder") },

            new Vibe { Name = "Cool",
                Adjectives = S("Iron, Shadow, Midnight, Golden, Savage, Silent, Electric, Legendary, Smooth, Ice, Neon, Wild, Cosmic, Atomic, Turbo, Frozen, Stealth, Steel, Crimson, Chrome, Mystic, Lunar, Solar, Thunder, Blazing, Rapid, Royal, Titan, Phantom, Cyber, Rogue, Dark, Night, Alpha, Ace, Arctic, Black, Blitz, Bold, Burning, Chilled, Classic, Cold, Daring, Deadly, Diamond, Elite, Fearless, Fierce, Fire, Flash, Flying, Frost, Galactic, Ghost, Glacier, Gold, Hyper, Infinite, Inferno, Jet, Laser, Lightning, Lone, Lucky, Magnetic, Maverick, Mega, Mighty, Neo, Nitro, Nova, Obsidian, Onyx, Primal, Prime, Quantum, Radiant, Raging, Rebel, Red, Sharp, Shining, Slick, Sonic, Stellar, Storm, Street, Swift, Ultra, Untamed, Venom, Wicked, Zero"),
                Nouns = S("Wolf, Tiger, Phoenix, Shark, Falcon, Viper, Legend, Ghost, Storm, Dragon, Panther, Rebel, Hawk, Eagle, Cobra, Raven, Lion, Jaguar, Rider, Blade, Comet, Rocket, Knight, Ninja, Samurai, Ranger, Titan, Bolt, Fox, Orca, Lynx, Vortex, Spark, Ace, Alpha, Apex, Arrow, Avalanche, Badger, Bandit, Beast, Blaze, Blizzard, Bullet, Cheetah, Cyclone, Dagger, Eclipse, Ember, Enigma, Flash, Fury, Gargoyle, Gator, Glacier, Grizzly, Hammer, Hurricane, Inferno, Jet, Kraken, Laser, Leopard, Maverick, Meteor, Mustang, Nomad, Nova, Outlaw, Pegasus, Phantom, Pirate, Predator, Pulse, Puma, Python, Racer, Rogue, Saber, Scorpion, Shadow, Sniper, Specter, Stallion, Stingray, Striker, Tempest, Thunder, Tornado, Typhoon, Valkyrie, Vandal, Venom, Warrior, Wraith, Yeti, Zephyr"),
                OfWhat = S("Storms, Shadows, Thunder, Speed, Style, Sunglasses, Lightning, Fire, Ice, Steel, Rockets, Midnight, Stars, Victory"),   // not "the Night": "Lady of the Night" reads badly
                Doers = S("Trendsetter, Vibe Curator, Heartbreaker, Night Rider, Rule Breaker, Speed Demon, Risk Taker, Show Stealer, Game Changer, Record Breaker") },
        };

        public static readonly List<Vibe> VibeChoices =
            new[] { new Vibe { Name = "Any vibe" } }.Concat(Vibes).ToList();

        // Gendered titles ("King Noodle") and suffixes ("Meme Lord"); the neutral ones are used for everyone.
        public static readonly string[] TitlesAny = S("Captain, Chief, Professor, Doctor, Agent, General, Boss, Grandmaster, Supreme Leader, Commander, Admiral, Coach, Detective, Sergeant, Mayor, Sheriff, Officer, Lil, Big, Mega, Ultra, Super");
        public static readonly string[] TitlesM = S("King, Lord, Prince, Sir, Mr., Duke, Big Bro, Baron, Count, Emperor, Uncle");
        public static readonly string[] TitlesF = S("Queen, Lady, Princess, Madam, Miss, Duchess, Big Sis, Baroness, Countess, Empress, Auntie");
        public static readonly string[] RolesAny = S("Boss, Master, Enjoyer, Hunter, Wizard, Legend, Specialist, Expert, Champion, Ninja, Whisperer, Overlord, Fan, Lover, Collector, Hoarder, Knight");
        public static readonly string[] RolesM = S("Lord, King, Prince, Emperor");
        public static readonly string[] RolesF = S("Queen, Lady, Princess, Empress");
        // "___ of Naps" only reads right after a real rank, not "Mr." or "Big Sis".
        public static readonly string[] OfTitlesAny = S("Captain, Chief, General, Grandmaster, Supreme Leader, Professor, Commander, Admiral, Mayor, Sheriff, Guardian");
        public static readonly string[] OfTitlesM = S("King, Lord, Prince, Duke, Baron, Count, Emperor");
        public static readonly string[] OfTitlesF = S("Queen, Lady, Princess, Duchess, Baroness, Countess, Empress");
        public static readonly string[] DoerPrefixes = S("Professional, Certified, Chief, Part-time, Retired, Full-time, Licensed, Official, Senior, Junior, Elite, Amateur");

        public const string Rules = "Words are joined without spaces (QueenOfNaps, SleepyPotato).";
    }

    /// <summary>All group-chat nicknames for one vibe / gender / length, handed out shuffled without repeats.</summary>
    class ChatPool : INamePool
    {
        readonly List<string> all = new List<string>();

        // Every nickname for a vibe / gender / prefix regardless of length, built once and cached:
        // "Any vibe" is several hundred thousand names, so stepping the max length only re-filters.
        static readonly Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();

        public ChatPool(Vibe vibe, Gender gender, int maxLen, string prefix)
        {
            prefix = (prefix ?? "").Trim();
            string key = vibe.Name + "|" + gender + "|" + prefix.ToLowerInvariant();
            if (!cache.TryGetValue(key, out var everything)) cache[key] = everything = Build(vibe, gender, prefix);
            foreach (var n in everything) if (n.Length <= maxLen) all.Add(n);
        }

        // "Loot Goblin" -> "LootGoblin", "Part-time" -> "PartTime", "Mr." -> "Mr"
        static string Norm(string phrase) => string.Concat(
            phrase.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(w => w.Replace(".", ""))
                  .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));

        static string[] Words(IEnumerable<string> phrases) => phrases.Select(Norm).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        static List<string> Build(Vibe vibe, Gender gender, string prefix)
        {
            var vibes = vibe.Adjectives == null ? ChatData.Vibes : new[] { vibe };
            string[] titles = Words(Gendered(ChatData.TitlesAny, ChatData.TitlesM, ChatData.TitlesF, gender));
            string[] roles = Words(Gendered(ChatData.RolesAny, ChatData.RolesM, ChatData.RolesF, gender));
            string[] ofTitles = Words(Gendered(ChatData.OfTitlesAny, ChatData.OfTitlesM, ChatData.OfTitlesF, gender));
            // "Any vibe" mixes words across moods too: "FluffyShark", "FeralDumpling".
            var adjectives = Words(vibes.SelectMany(v => v.Adjectives));
            var nouns = Words(vibes.SelectMany(v => v.Nouns));
            var ofWhat = Words(vibes.SelectMany(v => v.OfWhat));
            var doers = Words(vibes.SelectMany(v => v.Doers));
            var prefixes = Words(ChatData.DoerPrefixes);

            var list = new List<string>();
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void Add(string a, string b, string c = null)
            {
                // No word twice ("BossBoss", "NoobNoob").
                if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return;
                if (c != null && (string.Equals(a, c, StringComparison.OrdinalIgnoreCase) || string.Equals(b, c, StringComparison.OrdinalIgnoreCase))) return;
                string name = a + b + c;
                if (prefix.Length > 0 && !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
                if (set.Add(name)) list.Add(name);
            }

            foreach (var a in adjectives) foreach (var n in nouns) Add(a, n);          // SleepyPotato
            foreach (var t in titles) foreach (var n in nouns) Add(t, n);              // CaptainNoodle
            foreach (var n in nouns) foreach (var r in roles) Add(n, r);               // GoblinQueen
            foreach (var a in adjectives) Add("The", a, "One");                        // TheSleepyOne
            foreach (var t in ofTitles) foreach (var o in ofWhat) Add(t, "Of", o);     // QueenOfNaps
            foreach (var p in prefixes) foreach (var d in doers) Add(p, d);            // CertifiedOverthinker
            // Two nouns: "Any vibe" pairs across moods, the others within one ("TacoGoblin", "PillowSloth").
            foreach (var n1 in nouns) foreach (var n2 in nouns) Add(n1, n2);
            return list;
        }

        static IEnumerable<string> Gendered(string[] any, string[] m, string[] f, Gender g) =>
            g == Gender.Male ? any.Concat(m) :
            g == Gender.Female ? any.Concat(f) :
            any.Concat(m).Concat(f);

        public long Total(Style s) => all.Count;

        public string RandomName(Style s, Random rnd) => all.Count == 0 ? null : all[rnd.Next(all.Count)];

        public List<string> Take(int count, Style style, ISet<string> seen, Random rnd)
        {
            var result = new List<string>();
            while (result.Count < count)
            {
                string n = null;
                for (int i = 0; i < 200 && n == null && all.Count > 0; i++)
                {
                    string pick = all[rnd.Next(all.Count)];
                    if (!seen.Contains(pick)) n = pick;
                }
                if (n == null)
                {
                    var left = all.Where(x => !seen.Contains(x)).ToList();
                    if (left.Count == 0) break;
                    n = left[rnd.Next(left.Count)];
                }
                seen.Add(n);
                result.Add(n);
            }
            return result;
        }
    }
}
