using System;
using System.Collections.Generic;
using System.Linq;

namespace NameRandomizer
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
            new Vibe { Name = "Spooky",
                Adjectives = S("Spooky, Creepy, Haunted, Ghostly, Eerie, Cursed, Wicked, Shadowy, Gloomy, Grim, Hollow, Howling, Moonlit, Midnight, Undead, Wailing, Rattling, Shivering, Skeletal, Batty, Witchy, Ghoulish, Murky, Foggy, Misty, Phantom, Spectral, Sinister, Lurking, Creaky, Cobweb, Dusty, Moaning, Possessed, Pumpkin, Frightful, Freaky, Scary, Shrieking, Stitched, Bony, Toothy, Fanged, Bleak, Dread, Ominous, Restless, Hexed, Grave, Raven, Pale, Crooked, Twisted, Rotten, Moldy, Slimy, Batwing, Nightmare, Coffin, Crypt, Lantern, Candle, Cackling, Whispering, Haunting, Gothic, Vampire, Zombie, Cryptic, Shady"),
                Nouns = S("Ghost, Ghoul, Zombie, Vampire, Werewolf, Witch, Mummy, Skeleton, Spirit, Specter, Phantom, Wraith, Banshee, Goblin, Gremlin, Bat, Spider, Raven, Crow, Owl, Cat, Pumpkin, Skull, Bone, Coffin, Crypt, Tomb, Grave, Cauldron, Broom, Candle, Lantern, Cobweb, Gargoyle, Imp, Poltergeist, Haunt, Creep, Boogeyman, Scarecrow, Shadow, Moon, Fog, Hex, Curse, Potion, Spellbook, Reaper, Lich, Golem, Hound, Wolf, Hag, Troll, Ogre, Slime, Moth, Rat, Beetle, Worm, Toad, Spook, Shade, Nightmare, Gravedigger, Undertaker, Jackolantern, Broomstick, Cloak, Fang"),
                OfWhat = S("Shadows, Nightmares, Haunts, Ghosts, Bones, Cobwebs, Curses, Candles, Graves, Pumpkins, Screams, Spooks"),
                Doers = S("Ghost Hunter, Grave Digger, Spell Caster, Night Walker, Jump Scarer, Candle Lighter, Potion Brewer, Curse Breaker, Monster Spotter, Seance Holder") },

            new Vibe { Name = "Space / sci-fi",
                Adjectives = S("Stellar, Lunar, Solar, Galactic, Astro, Orbital, Starry, Nebula, Quantum, Zero Gravity, Rocket, Alien, Martian, Interstellar, Celestial, Plasma, Photon, Ion, Laser, Warp, Hyper, Meteor, Comet, Atomic, Robotic, Cyber, Digital, Neon, Chrome, Silver, Electric, Magnetic, Gravity, Void, Deep, Distant, Floating, Drifting, Spinning, Glowing, Radiant, Twinkling, Eclipse, Nova, Pulsar, Quasar, Saturn, Jupiter, Venus, Pluto, Mercury, Polar, Icy, Supersonic, Turbo, Future, Retro, Space, Moon, Sun, Star, Sky, UFO, Beam, Cosmic, Lightyear, Nebular, Holographic, Bionic"),
                Nouns = S("Astronaut, Alien, Martian, Robot, Android, Cyborg, Rocket, Shuttle, Satellite, Comet, Meteor, Asteroid, Planet, Moon, Star, Sun, Nebula, Galaxy, Quasar, Pulsar, Nova, Supernova, Orbit, Crater, Rover, Probe, Capsule, Starship, Spaceship, Cosmonaut, Pilot, Explorer, Voyager, Pioneer, Ranger, Drifter, Wanderer, Beacon, Beam, Laser, Photon, Proton, Neutron, Atom, Quark, Ion, Plasma, Vortex, Wormhole, Black Hole, Eclipse, Saturn, Jupiter, Pluto, Venus, Mars, Orion, Cosmo, Astro, Stardust, Starlight, Spacesuit, Helmet, Jetpack, Teleporter, Hologram, Droid, Saucer, UFO, Lightsail"),
                OfWhat = S("Stars, Planets, the Moon, Galaxies, Comets, Orbits, Rockets, Space, Asteroids, Eclipses, Starlight, Aliens"),
                Doers = S("Star Gazer, Moon Walker, Space Cadet, Rocket Builder, Planet Hopper, Comet Chaser, Alien Spotter, Orbit Breaker, Star Counter, Galaxy Explorer") },

            new Vibe { Name = "Music",
                Adjectives = S("Funky, Groovy, Jazzy, Bluesy, Rocking, Melodic, Musical, Harmonic, Acoustic, Electric, Loud, Bassy, Booming, Catchy, Chill, LoFi, Retro, Vintage, Vinyl, Disco, Dancing, Singing, Humming, Whistling, Drumming, Strumming, Rhythmic, Snappy, Bouncy, Smooth, Mellow, Soulful, Raspy, Sharp, Flat, Minor, Major, Encore, Platinum, Golden, Hip Hop, Punk, Metal, Indie, Pop, Classical, Lyrical, Karaoke, Offbeat, Upbeat, Neon, Synth, Analog, Remixed, Looped, Echoing, Amped, Tuned, Detuned, Shredding, Headbanging, Spinning, Rhyming, Freestyle, Backstage, Unplugged, Crooning, Jamming, Tempo, Tuneful"),
                Nouns = S("Drummer, Bassist, Guitarist, Pianist, Singer, Rapper, Rocker, DJ, Diva, Maestro, Conductor, Composer, Fiddler, Piper, Trumpet, Drum, Bass, Guitar, Piano, Violin, Cello, Flute, Harp, Banjo, Ukulele, Kazoo, Trombone, Tuba, Saxophone, Sax, Synth, Mic, Speaker, Amp, Record, Vinyl, Cassette, Mixtape, Playlist, Anthem, Ballad, Melody, Chorus, Verse, Beat, Bassline, Riff, Solo, Encore, Remix, Track, Tune, Jingle, Lyric, Note, Chord, Rhythm, Tempo, Groove, Disco, Jukebox, Headphones, Boombox, Tambourine, Maraca, Cymbal, Metronome, Busker, Bard, Choir, Band, Idol, Popstar, Rockstar"),
                OfWhat = S("Beats, Bass, Karaoke, Playlists, Encores, Mixtapes, Ballads, Riffs, Remixes, Rhythm, Vinyl, Jams"),
                Doers = S("Air Drummer, Shower Singer, Playlist Maker, Beat Dropper, Song Skipper, Lyric Forgetter, Bass Booster, Volume Raiser, Karaoke Hogger, Hum Machine") },

            new Vibe { Name = "Sports",
                Adjectives = S("Sporty, Speedy, Fast, Quick, Strong, Mighty, Athletic, Agile, Nimble, Flexible, Tough, Golden, Silver, Bronze, Olympic, Pro, Rookie, Varsity, Winning, Undefeated, Unbeaten, Dunking, Sprinting, Jumping, Flying, Kicking, Punching, Slamming, Bouncing, Tackling, Dribbling, Diving, Surfing, Skating, Climbing, Lifting, Running, Rowing, Cycling, Boxing, Racing, Hustling, Pumped, Buff, Swole, Benched, Offside, Overtime, Penalty, All Star, Final, Rapid, Blitz, Power, Sunday, Weekend, Fantasy, Iron, Turbo, Wild, Hattrick, Underdog, Sideline, Halftime, Bonus, Extra Time, Speedball"),
                Nouns = S("Striker, Goalie, Keeper, Kicker, Pitcher, Batter, Catcher, Quarterback, Linebacker, Sprinter, Runner, Racer, Swimmer, Diver, Surfer, Skater, Boarder, Climber, Lifter, Boxer, Wrestler, Fighter, Champ, Champion, Referee, Ref, Mascot, Rookie, Veteran, Allstar, MVP, Ace, Slugger, Dunker, Baller, Hooper, Golfer, Caddie, Jockey, Cyclist, Rower, Archer, Fencer, Gymnast, Yogi, Trainer, Spotter, Benchwarmer, Ball, Goal, Hoop, Net, Bat, Racket, Puck, Whistle, Trophy, Medal, Jersey, Sneaker, Dumbbell, Kettlebell, Treadmill, Stadium, Arena, Podium, Touchdown, Home Run, Slam Dunk, Hattrick, Underdog"),
                OfWhat = S("Goals, Gains, Trophies, Medals, Sprints, Slam Dunks, Touchdowns, Leg Day, the Gym, Cardio, Overtime, Comebacks"),
                Doers = S("Gym Rat, Couch Coach, Ball Hog, Score Keeper, Rep Counter, Sideline Yeller, Trophy Hunter, Marathon Runner, Protein Drinker, Warmup Skipper") },

            new Vibe { Name = "Nature",
                Adjectives = S("Rainy, Stormy, Windy, Cloudy, Foggy, Frosty, Icy, Breezy, Thundering, Drizzly, Muddy, Mossy, Leafy, Grassy, Woody, Rocky, Sandy, Dusty, Blooming, Budding, Flowering, Thorny, Prickly, Green, Golden, Amber, Autumn, Winter, Summer, Spring, Morning, Evening, Twilight, Dawn, Dusky, Mountain, River, Ocean, Coastal, Tropical, Arctic, Desert, Forest, Jungle, Meadow, Petal, Pine, Maple, Cedar, Willow, Oak, Birch, Fern, Lavender, Rose, Sunflower, Rainbow, Lightning, Thunder, Hailing, Wandering, Rustling, Babbling, Crackling, Glowing, Sunny, Snowy, Misty, Wild, Earthy"),
                Nouns = S("Acorn, Pinecone, Leaf, Petal, Sprout, Seedling, Sapling, Tree, Oak, Pine, Willow, Maple, Fern, Moss, Mushroom, Toadstool, Daisy, Tulip, Rose, Lily, Lotus, Sunflower, Dandelion, Clover, Cactus, Bamboo, Ivy, Vine, Thistle, Pebble, Boulder, Rock, Mountain, Hill, Valley, River, Creek, Brook, Lake, Pond, Ocean, Wave, Tide, Reef, Island, Beach, Dune, Desert, Forest, Jungle, Meadow, Glade, Grove, Cloud, Raincloud, Raindrop, Puddle, Rainbow, Storm, Thunder, Lightning, Breeze, Gust, Tornado, Snowflake, Snowman, Icicle, Frost, Hail, Sunbeam, Sunrise, Sunset, Moonrise, Firefly, Ladybug, Bumblebee, Butterfly, Caterpillar, Dragonfly, Hedgehog, Squirrel, Chipmunk, Deer, Fox, Owl"),
                OfWhat = S("Leaves, Flowers, Rain, Storms, Thunder, Puddles, Mushrooms, the Forest, Seasons, Sunsets, Rainbows, Snow"),
                Doers = S("Tree Hugger, Plant Parent, Flower Sniffer, Puddle Jumper, Cloud Watcher, Rock Collector, Leaf Peeper, Bird Watcher, Weather Watcher, Mushroom Picker") },

            new Vibe { Name = "Fantasy / magic",
                Adjectives = S("Magic, Magical, Mystic, Enchanted, Arcane, Ancient, Elder, Elven, Dwarven, Royal, Noble, Holy, Sacred, Divine, Cursed, Hexed, Charmed, Bewitched, Glimmering, Shimmering, Sparkling, Glowing, Golden, Silver, Crystal, Emerald, Sapphire, Ruby, Obsidian, Runic, Rune, Legendary, Mythic, Epic, Heroic, Brave, Valiant, Bold, Fearless, Wandering, Questing, Dragon, Fairy, Pixie, Shadow, Moon, Sun, Star, Storm, Frost, Flame, Fire, Thunder, Wind, Earth, Spirit, Dream, Twilight, Forgotten, Lost, Hidden, Secret, Forbidden, Sleeping, Silent, Eternal, Immortal, Fabled, Wise, Gilded"),
                Nouns = S("Wizard, Witch, Sorcerer, Mage, Warlock, Druid, Paladin, Knight, Squire, Bard, Ranger, Rogue, Thief, Archer, Monk, Cleric, Elf, Dwarf, Gnome, Halfling, Orc, Goblin, Troll, Ogre, Giant, Dragon, Wyvern, Griffin, Phoenix, Unicorn, Pegasus, Hydra, Basilisk, Chimera, Kraken, Mermaid, Siren, Centaur, Minotaur, Fairy, Pixie, Sprite, Nymph, Dryad, Golem, Imp, Familiar, Wand, Staff, Scroll, Tome, Grimoire, Potion, Elixir, Amulet, Talisman, Rune, Crown, Throne, Sword, Shield, Castle, Tower, Dungeon, Quest, Kingdom, Realm, Oracle, Prophet, Sage, Seer, Alchemist, Enchanter, Summoner, Hero"),
                OfWhat = S("Dragons, Spells, Potions, Runes, the Realm, Wands, Quests, Legends, Scrolls, Crystals, Magic, Prophecies"),
                Doers = S("Spell Caster, Dragon Tamer, Potion Brewer, Quest Giver, Treasure Hunter, Dungeon Crawler, Wand Waver, Rune Reader, Myth Teller, Scroll Keeper") },

            new Vibe { Name = "Office / school",
                Adjectives = S("Busy, Overworked, Underpaid, Caffeinated, Decaf, Late, Early, Tardy, Punctual, Organized, Messy, Productive, Remote, Hybrid, Corporate, Junior, Senior, Lead, Approved, Pending, Urgent, Overdue, Unread, Muted, Unmuted, Typing, Scheduled, Studious, Nerdy, Geeky, Bookish, Brainy, Clever, Smart, Curious, Bored, Distracted, Stressed, Chill, Lunchtime, Monday, Friday, Weekend, Sticky, Stapled, Laminated, Printed, Copied, Highlighted, Graded, Excused, Detention, Homework, Honor, Gold Star, Overtime, Coffee, Tired, Sleepy, Hustling, Deskbound, Offline, Online, Recess, Snack Break"),
                Nouns = S("Intern, Manager, Coworker, Employee, Clerk, Secretary, Accountant, Analyst, Developer, Designer, Engineer, Teacher, Student, Nerd, Geek, Bookworm, Scholar, Tutor, Principal, Janitor, Librarian, Spreadsheet, Stapler, Printer, Laptop, Keyboard, Mouse, Monitor, Cubicle, Desk, Chair, Mug, Coffee, Espresso, Latte, Donut, Memo, Email, Meeting, Deadline, Report, Slide, Pencil, Pen, Eraser, Ruler, Notebook, Binder, Folder, Backpack, Locker, Lunchbox, Calculator, Highlighter, Crayon, Marker, Whiteboard, Chalkboard, Homework, Quiz, Exam, Essay, Textbook, Diploma, Graduate, Freshman, Sophomore, Hall Monitor, Paperclip, Clipboard"),
                OfWhat = S("Meetings, Deadlines, Emails, Spreadsheets, Coffee, Homework, Mondays, Paperwork, Sticky Notes, Group Projects, Overtime, Recess"),
                Doers = S("Email Ignorer, Meeting Skipper, Coffee Drinker, Deadline Dodger, Note Taker, Desk Napper, Stapler Thief, Pen Borrower, Homework Forgetter, Bell Watcher") },

            new Vibe { Name = "Tech / internet",
                Adjectives = S("Online, Offline, Viral, Trending, Wireless, Bluetooth, Digital, Virtual, Pixel, Glitchy, Buggy, Laggy, Buffering, Loading, Updating, Rebooting, Encrypted, Cloud, Smart, Turbo, Hyper, Cyber, Binary, Quantum, Analog, Retro, Vintage, Dialup, Broadband, Unlimited, Premium, Verified, Blocked, Pinned, Saved, Shared, Liked, Followed, Subscribed, Streaming, Uploading, Downloading, Scrolling, Swiping, Clicking, Typing, Emoji, Meme, Selfie, Hashtag, Chatty, Spammy, Nerdy, Techy, Robotic, Electric, Battery, Charging, Unplugged, Wired, Synced, Archived, Muted, Ghosted, Logged In"),
                Nouns = S("Robot, Bot, Cyborg, Android, Coder, Programmer, Gamer, Streamer, Vlogger, Blogger, Influencer, Poster, Lurker, Troll, Moderator, Admin, Noob, Newbie, Geek, Nerd, Pixel, Byte, Bit, Bug, Glitch, Virus, Cookie, Cache, Router, Modem, Wifi, Server, Cloud, Laptop, Tablet, Phone, Smartphone, Charger, Cable, Battery, Keyboard, Mouse, Webcam, Headset, Drone, Gadget, Widget, App, Emoji, Meme, Selfie, Hashtag, Avatar, Profile, Firewall, Algorithm, Download, Upload, Update, Reboot, Screenshot, Notification, Ringtone, Inbox, Spam, Browser, Tab, Popup, Hotspot, Gif"),
                OfWhat = S("Memes, Emojis, Notifications, Tabs, Wifi, Updates, Screenshots, Selfies, Hashtags, Pixels, Bugs, Cookies"),
                Doers = S("Tab Hoarder, Emoji Spammer, Doom Scroller, Link Sharer, Comment Reader, Update Skipper, Like Giver, Selfie Taker, Gif Sender, Meme Reposter") },
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

    /// <summary>
    /// All group-chat nicknames for one vibe / gender / length / prefix, handed out at random without repeats.
    ///
    /// "Any vibe" makes a few million names, too many to hold as strings (hundreds of MB). So each
    /// pattern ("SleepyPotato" = adjective + noun) is kept as its word lists, its size is counted from
    /// word lengths, and names are built only when picked. With a "starts with" prefix the pool is small,
    /// so it is listed outright.
    /// </summary>
    class ChatPool : INamePool
    {
        /// <summary>Pre + X + Mid + Y + Post, e.g. "" + Queen + "Of" + Naps + "". Y may be null.</summary>
        class Pattern
        {
            public string Pre = "", Mid = "", Post = "";
            public string[] X, Y;
            public int[] YUpTo;          // YUpTo[n] = Y words with length <= n
            public HashSet<string> YSet;
            public long Count;           // names that fit the current length
            public double Bias = 1;      // how often to pick this pattern, relative to its size
            public int FixedLen => Pre.Length + Mid.Length + Post.Length;
        }

        const int MinLen = 3;
        readonly int maxLen;
        readonly List<Pattern> patterns = new List<Pattern>();
        readonly List<string> listed;    // only when a prefix is set
        readonly long total;

        public ChatPool(Vibe vibe, Gender gender, int maxLen, string prefix)
        {
            this.maxLen = maxLen;
            prefix = (prefix ?? "").Trim();
            foreach (var p in BuildPatterns(vibe, gender))
            {
                var q = new Pattern { Pre = p.Pre, Mid = p.Mid, Post = p.Post, X = p.X, Y = p.Y, YUpTo = p.YUpTo, YSet = p.YSet, Bias = p.Bias };
                q.Count = CountFits(q);
                if (q.Count > 0) patterns.Add(q);
            }
            if (prefix.Length > 0)
            {
                listed = ListWithPrefix(prefix);
                total = listed.Count;
            }
            else total = patterns.Sum(p => p.Count);
        }

        // ---- word lists per vibe / gender, built once ----

        static readonly Dictionary<string, List<Pattern>> cache = new Dictionary<string, List<Pattern>>();

        // "Loot Goblin" -> "LootGoblin", "Part-time" -> "PartTime", "Mr." -> "Mr"
        static string Norm(string phrase) => string.Concat(
            phrase.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(w => w.Replace(".", ""))
                  .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1)));

        static string[] Words(IEnumerable<string> phrases) => phrases.Select(Norm).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        static IEnumerable<string> Gendered(string[] any, string[] m, string[] f, Gender g) =>
            g == Gender.Male ? any.Concat(m) :
            g == Gender.Female ? any.Concat(f) :
            any.Concat(m).Concat(f);

        static List<Pattern> BuildPatterns(Vibe vibe, Gender gender)
        {
            string key = vibe.Name + "|" + gender;
            if (cache.TryGetValue(key, out var cached)) return cached;

            var vibes = vibe.Adjectives == null ? ChatData.Vibes : new[] { vibe };
            var titles = Words(Gendered(ChatData.TitlesAny, ChatData.TitlesM, ChatData.TitlesF, gender));
            var roles = Words(Gendered(ChatData.RolesAny, ChatData.RolesM, ChatData.RolesF, gender));
            var ofTitles = Words(Gendered(ChatData.OfTitlesAny, ChatData.OfTitlesM, ChatData.OfTitlesF, gender));
            // "Any vibe" mixes words across moods too: "FluffyShark", "HauntedTaco".
            var adjectives = Words(vibes.SelectMany(v => v.Adjectives));
            var nouns = Words(vibes.SelectMany(v => v.Nouns));
            var ofWhat = Words(vibes.SelectMany(v => v.OfWhat));
            var doers = Words(vibes.SelectMany(v => v.Doers));
            var prefixes = Words(ChatData.DoerPrefixes);

            Pattern P(string[] x, string[] y, string pre = "", string mid = "", string post = "", double bias = 1)
            {
                var p = new Pattern { Pre = pre, Mid = mid, Post = post, X = x, Y = y, Bias = bias };
                if (y != null)
                {
                    p.YUpTo = new int[64];
                    foreach (var w in y) p.YUpTo[Math.Min(w.Length, 63)]++;
                    for (int i = 1; i < 64; i++) p.YUpTo[i] += p.YUpTo[i - 1];
                    p.YSet = new HashSet<string>(y, StringComparer.OrdinalIgnoreCase);
                }
                return p;
            }

            var list = new List<Pattern>
            {
                P(adjectives, nouns),                     // SleepyPotato
                P(titles, nouns),                         // CaptainNoodle
                P(nouns, roles),                          // GoblinQueen
                P(adjectives, null, "The", "", "One"),    // TheSleepyOne
                P(ofTitles, ofWhat, "", "Of"),            // QueenOfNaps
                P(prefixes, doers),                       // CertifiedOverthinker
                // TacoGoblin. Half of all names, but the most random-sounding (PetalQuark), so it is
                // picked about a third as often as its size alone would give.
                P(nouns, nouns, bias: 0.35),
            };
            cache[key] = list;
            return list;
        }

        // ---- counting and picking ----

        bool Fits(int len) => len >= MinLen && len <= maxLen;

        long CountFits(Pattern p)
        {
            long n = 0;
            foreach (var x in p.X)
            {
                int used = p.FixedLen + x.Length;
                if (p.Y == null) { if (Fits(used)) n++; continue; }
                int lo = Math.Max(MinLen - used, 1), hi = Math.Min(maxLen - used, 63);
                if (hi < lo) continue;
                n += p.YUpTo[hi] - p.YUpTo[lo - 1];
                // The same word twice ("NoobNoob") is not a name.
                if (p.YSet.Contains(x) && x.Length >= lo && x.Length <= hi) n--;
            }
            return n;
        }

        // One random name of a pattern (uniform over the names that fit), or null after too many misses.
        string Pick(Pattern p, Random rnd)
        {
            for (int i = 0; i < 500; i++)
            {
                string x = p.X[rnd.Next(p.X.Length)];
                string y = p.Y == null ? "" : p.Y[rnd.Next(p.Y.Length)];
                if (p.Y != null && string.Equals(x, y, StringComparison.OrdinalIgnoreCase)) continue;
                string name = p.Pre + x + p.Mid + y + p.Post;
                if (Fits(name.Length)) return name;
            }
            return null;
        }

        public long Total(Style s) => total;

        public string RandomName(Style s, Random rnd)
        {
            if (listed != null) return listed.Count == 0 ? null : listed[rnd.Next(listed.Count)];
            if (total == 0) return null;
            // Pick a pattern by its size (times its bias), then a name inside it.
            double weightSum = patterns.Sum(p => p.Count * p.Bias);
            for (int tries = 0; tries < 20; tries++)
            {
                double r = rnd.NextDouble() * weightSum;
                foreach (var p in patterns)
                {
                    double w = p.Count * p.Bias;
                    if (r < w) { var n = Pick(p, rnd); if (n != null) return n; break; }
                    r -= w;
                }
            }
            return null;
        }

        public List<string> Take(int count, Style style, ISet<string> seen, Random rnd)
        {
            var result = new List<string>();
            int misses = 0;
            while (result.Count < count)
            {
                string n = misses < 2000 ? RandomName(style, rnd) : null;
                if (n != null && seen.Contains(n)) { misses++; continue; }
                if (n == null)
                {
                    // Nearly everything was shown: list what is left (only happens on a small pool).
                    var left = (listed ?? ListAll()).Where(x => !seen.Contains(x)).ToList();
                    if (left.Count == 0) break;
                    n = left[rnd.Next(left.Count)];
                }
                seen.Add(n);
                result.Add(n);
            }
            return result;
        }

        IEnumerable<string> Names(Pattern p, string prefix)
        {
            foreach (var x in p.X)
            {
                string head = p.Pre + x + p.Mid;
                // Skip a whole row early when its start already rules the prefix out.
                if (prefix.Length > 0 && !(head.Length >= prefix.Length
                        ? head.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        : prefix.StartsWith(head, StringComparison.OrdinalIgnoreCase))) continue;
                foreach (var y in p.Y ?? new[] { "" })
                {
                    if (p.Y != null && string.Equals(x, y, StringComparison.OrdinalIgnoreCase)) continue;
                    string name = head + y + p.Post;
                    if (!Fits(name.Length)) continue;
                    if (prefix.Length > 0 && !name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                    yield return name;
                }
            }
        }

        List<string> ListWithPrefix(string prefix)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return patterns.SelectMany(p => Names(p, prefix)).Where(set.Add).ToList();
        }

        List<string> ListAll() => ListWithPrefix("");
    }
}
