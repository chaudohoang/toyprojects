# ARPG Name Randomizer

Small WinForms tool (.NET Framework 4.7.2) that makes character names for ARPGs.

- **Game**: Path of Exile, Path of Exile 2, Diablo IV, Last Epoch. Each game has its own class list and name rules (max length, underscores).
- **Class** picks the flavour: Marauder/Warrior → brutal, Witch/Necromancer → dark, Sorceress/Mage → arcane, Monk/Spiritborn → spirit, and so on. "Any class" mixes them all.
- **Style**: made-up names (`Morvath`), compounds (`Bonecaller`), name + title (`KaelDuskborn`, or `Kael_Duskborn` in PoE), or a mix.
- Gender, max length, "starts with" and count options.
- Click a name to copy it. Double-click (or press F) to add it to favourites, saved to `favorites.txt` next to the exe. Delete removes a favourite.
- Last-used options are saved to `settings.txt` next to the exe.

Run `bin\Debug\net472\ArpgNameRandomizer.exe`. Build with `dotnet build` in this folder.

## Adding a game or class

Everything lives in `NameData.cs`: add a `G(...)` entry with its length limits and classes, mapping each class to one or more themes. Add a new `Theme` if none of the existing word lists fit.

The Diablo IV (12) and Last Epoch (20) length limits were not checked in game. If a game rejects a name, fix the number there.
