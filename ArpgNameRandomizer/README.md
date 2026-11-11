# Name Randomizer (ArpgNameRandomizer)

Small WinForms tool (.NET Framework 4.7.2) that makes names, one at a time. Pick a **Category**:

## ARPG character

- **Class** sets the flavour. One list covers every game: "Witch / Necromancer / Acolyte" goes dark, "Warrior / Barbarian / Marauder" goes brutal, "Sorceress / Mage" goes arcane, and so on. "Any class" mixes them all.
- **Style**: made-up names (`Morvath`), compounds (`Bonecaller`), name + title (`KaelDuskborn`), or a mix. Letters only, never underscores.
- **Max length** (shared with group chat names) covers each game's limit: PoE / PoE2 3-23, Last Epoch ~20, Diablo IV ~12 (the D4 and LE limits were not checked in game).

## Group chat nickname

- **Vibe**: Funny / chaos, Cute, Food, Sleepy / lazy, Gamer, Cool, or Any (which mixes words across vibes: "Fluffy Shark").
- Patterns like `TacoGoblin`, `LilNoodle`, `SleepyPotato`, `CaptainNoodle`, `GoblinQueen`, `QueenOfNaps`, `TheUnhingedOne`, `CertifiedOverthinker`. Words are joined without spaces.
- Gender picks King/Lord vs Queen/Lady style titles; Any uses all of them.

## Both

- Generate (or Enter / Space) gives the next name. Click the name to copy it. **Add to favourites** saves it to `favorites.txt` next to the exe; Delete removes one.
- Gender, max length (3-30, one value for both categories) and "starts with" options.
- **Add a number (Name07)**: two digits at the end, 100 versions of every name. The name part gets 2 letters less so it still fits the max length. Diablo IV allows letters only; whether PoE / Last Epoch accept digits was not checked in game.
- **No repeats while the app is open**: it lists every name the settings can make (at max length 14: about 17.8 million ARPG names for "Any class", 0.7-6.4 million per class; about 446,000 chat nicknames for "Any vibe", 17,000-24,000 per vibe; with a number, about 178 million and 30 million) and hands them out shuffled, skipping names already shown and favourites. Nothing is saved, so restarting starts a fresh shuffle.
- Last-used options are saved to `settings.txt` next to the exe.

Run `bin\Debug\net472\ArpgNameRandomizer.exe`. Build with `dotnet build` in this folder (close the app first, it locks the exe).

## Adding words

- ARPG: `NameData.cs`. Each `Theme` has syllables and compound halves; `ArpgClasses` maps class choices to themes.
- Group chat: `ChatNames.cs`. Each `Vibe` has adjectives, nouns, "___ of X" endings and "Professional ___" jobs.
