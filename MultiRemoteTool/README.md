# Multi Remote Tool

A single-file C# WinForms address book that launches **any** remote-desktop client. One tool, one
list of machines, whichever client you happen to use for each of them — RustDesk, AnyDesk,
UltraViewer, TightVNC, UltraVNC, Radmin. Nothing is hard-coded: each client is described by a
small INI file you can add yourself.

## Build

```
build.bat
```

Uses `csc.exe` from .NET Framework 4.x (`%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\`).
No Visual Studio, no NuGet, no project file. `app.ico` next to the source gets embedded
automatically.

`build.bat` ends with `pause`, so to call the compiler directly:

```
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /platform:anycpu `
    /optimize+ /out:MultiRemoteTool.exe /r:System.dll /r:System.Windows.Forms.dll `
    /r:System.Drawing.dll /r:System.Core.dll /win32icon:app.ico MultiRemoteTool.cs
```

Output: `MultiRemoteTool.exe` — portable, keeps all its state in files next to itself.

## How it works

**One platform = one INI + one CSV.**

```
MultiRemoteTool.exe
RustDesk.ini            <- how to launch this client
contacts_RustDesk.csv   <- who to launch it against
settings.txt            <- which platforms are shown, and in what order
```

The app scans its own folder for `*.ini`, shows each valid one as a row in the left sidebar,
and loads that platform's CSV into the list on the right. Click a machine, press **Connect**,
and it runs the platform's executable with the command line built from that row.

To add a client the app has never heard of, write a four-line INI. There is no plugin API and
nothing to recompile.

## Platform INI

```ini
[Platform]
Name=RustDesk
ExeName=C:\Program Files\RustDesk\rustdesk.exe
CommandLine=--connect {id} --password {pw}
```

| Key | Required | Meaning |
|---|---|---|
| `Name` | yes | Shown in the sidebar. Also decides the file names: `<Name>.ini` and `contacts_<Name>.csv`, with anything that isn't a letter, digit, `_` or `-` replaced by `_`. |
| `ExeName` | yes | Full path, or a bare file name to be searched for (see below). |
| `CommandLine` | yes | Argument template. See placeholders. |
| `BookFileName` | no | Use this CSV instead of `contacts_<Name>.csv`. Point two platforms at the same file and they share one address book. |
| `ProbePort` | no | Only meaningful with `{ip}`. See the resolution section. |

### Placeholders

| Token | Replaced with |
|---|---|
| `{id}` | The contact's Remote ID, with all whitespace stripped (so `1 444 941 743` works as typed). |
| `{pw}` | The contact's password. |
| `{exe}` | Full path to the resolved executable. Only needed for pipe commands. |
| `{ip}` | An address resolved from the contact's Remote ID at connect time — see below. |

`<REMOTE-ID>` and `<REMOTE-PASSWORD>` still work as legacy spellings of `{id}` and `{pw}`.

If the template contains a `|`, the whole thing is run through `cmd.exe /c` so the shell can
handle the pipe. That is how AnyDesk gets its password on stdin:

```ini
CommandLine=echo {pw} | "{exe}" {id} --with-password
```

### Finding the executable

A bare `ExeName` is searched for in this order, first hit wins:

1. System `PATH`
2. Program Files, Program Files (x86), Local AppData — the root itself, then **one** level of
   subdirectory (so `Program Files\TightVNC\tvnviewer.exe` is found)
3. The app's own folder

That one-level limit matters. UltraVNC installs to `Program Files\uvnc\UltraVNC\` — two levels
deep — so a bare `vncviewer.exe` can never resolve to the installed copy and always falls through
to the bundled one in step 3. That is deliberate: the bundled viewers make the folder
self-contained, and `UltraVNC.ini` / `TightVNC.ini` use bare names for exactly this reason.

An absolute path is used as-is and skips the search.

## Contacts CSV

```
Name,RemoteID,Password
I5-3470,1 444 941 743,Chau2803
```

Three columns, no header row, UTF-8. A two-column row is accepted and gets an empty password —
`contacts_VNC.csv` is two-column, since the VNC viewers prompt instead. Quoted fields and `""`
escapes are handled.

> **Watch out for empty passwords in the template.** If the CSV has no password, `--password {pw}`
> becomes a bare `--password` and swallows whatever argument comes next. Leave the flag out
> entirely for books that don't carry passwords.

## settings.txt

```
LoadedPlatforms=UltraVNC,TightVNC,AnyDesk,UltraViewer,RustDesk,RustDesk-IP
```

A **strict whitelist**, not just an order. An INI sitting in the folder but missing from this list
is invisible in the UI. On a first run with no `settings.txt`, everything found on disk is shown.

The **first entry is the platform selected at startup**, and the app rewrites this line when it
exits — so launching the app to test something dirties this file in git. Check `git status`
afterwards.

## Special platforms: `{ip}` and name → address resolution

Most platforms connect to an ID that never changes. `RustDesk-IP` is different: its address book
holds **machine names**, and the actual address is worked out each time you connect.

```ini
[Platform]
Name=RustDesk-IP
ExeName=C:\Program Files\RustDesk\rustdesk.exe
CommandLine=--connect {ip}:21118 --password {pw}
ProbePort=21118
```

```
Victus16,victus16,Chau2803
```

The point is that a laptop on the office LAN today and tethered from home tomorrow has two
different addresses, and neither belongs in a file you have to maintain. Put the name in the book
and let the tool find the machine.

Any platform whose `CommandLine` contains `{ip}` gets this treatment. There is no flag to turn it
on — the placeholder *is* the switch.

### What happens on Connect

**1. Short-circuit.** If the Remote ID already parses as an IP address, it is used as-is. The
book can mix names and literal addresses freely.

**2. Gather candidates** — three sources, run **concurrently**:

| Source | Gives you |
|---|---|
| `Dns.GetHostAddresses(name)` | Whatever DNS answers — with Tailscale MagicDNS active, usually just the `100.x` address |
| `tailscale ip -4 <name>` | The peer's tailnet address |
| `tailscale status`, scraped for `direct <ip>:41641` | The peer's **LAN** address |

That third one is the load-bearing trick. **Once MagicDNS is answering for a name, DNS returns
only the `100.x` address** — the LAN IP is simply not visible through normal lookup. Tailscale's
own status output is the only place it appears, and only while that peer has a live direct
connection. If Tailscale is relaying through DERP, there is no LAN candidate at all.

`tailscale.exe` is looked for under `Program Files\Tailscale\` and `Program Files (x86)\Tailscale\`.
If it isn't installed, those two sources are skipped and resolution falls back to DNS alone.

Loopback and `169.254.x.x` link-local addresses are dropped; duplicates are merged.

**3. Rank.** Candidates sort by kind, best first:

| Rank | Range | Label |
|---|---|---|
| 0 | `10/8`, `172.16/12`, `192.168/16` | LAN |
| 1 | `100.64/10` (CGNAT) | Tailscale |
| 2 | anything else | routed |

LAN wins because a local link is faster and doesn't depend on the Tailscale service being up.
To invert the preference, swap the two return values in `HostResolver.Rank()`.

**4. Probe.** Every candidate is TCP-connected on `ProbePort` **at the same time**, with a 700 ms
timeout. The winner is **the best-ranked address that actually answered** — not the one that
answered first. Parallelism only caps the wait at one timeout for the whole set instead of one
timeout each; it never decides the winner.

With `ProbePort` unset or `0`, no probing happens and the best-ranked candidate is taken on faith.

**5. Launch, or explain.** The chosen address fills `{ip}`, and the status bar shows it:

```
Connecting via RustDesk-IP to victus16 [192.168.100.49]...
```

If nothing answered, the launch is **aborted** with a dialog listing every address tried and the
port none of them answered on — rather than handing the client an address that cannot work and
letting it hang.

### What to expect

Measured against a live tailnet:

| Case | Result | Time |
|---|---|---|
| On the LAN, direct connection up | LAN address, beating its tailnet one | ~50 ms |
| Remote, or relayed via DERP | Tailscale address | ~40 ms |
| Machine offline | aborts, lists what was tried | ~750 ms |
| Name doesn't exist | aborts | ~2.8 s |

**The same machine will resolve differently at different times**, depending on whether Tailscale
currently has a direct connection to it. That is correct behaviour, not a bug — the whole point is
to use the best path available right now.

### Why port 21118

RustDesk's Direct IP Access listens on 21118, and that number is not a guess: `src/client.rs`
routes a direct-IP peer through `check_port(peer, RELAY_PORT + 1)`, and `RELAY_PORT` is 21117
(`hbb_common/src/config.rs`). `check_port` only appends a port when the address doesn't already
carry one, and `is_ipv4_str` accepts the `host:port` form — so both `{ip}` and `{ip}:21118` are
valid, and the explicit form is used here to keep the port visible.

Direct IP Access must be **enabled on the target machine** (Settings → Security). Note that it is
unencrypted, which is fine over a tailnet or a trusted LAN and not fine over the open internet.

## Using it

- **Click a row, then Connect** — or just double-click the row.
- **The search box doubles as an address bar.** Type anything that isn't in the book and Connect
  arms against it directly. On an `{ip}` platform a typed *name* goes through resolution too, so
  you can reach a machine you never added.
- **Add Contact / Edit / Delete** edit the CSV in place. **Reload** re-reads it from disk, for when
  you've edited the file by hand.
- **Import / Export** (top bar) move the contacts CSV. **Import Platform / Export Platform**
  (sidebar) move an INI *and* its CSV together, so a platform can be carried to another PC.
- The badge on each sidebar row is that platform's contact count.
- Selecting a sidebar row reveals its action buttons: ✎ edit, ↑ export, – remove from list (keeps
  the files on disk), ✕ delete permanently.

## Bundled platforms

| Platform | Executable | Notes |
|---|---|---|
| `RustDesk` | installed RustDesk | Connects by RustDesk ID |
| `RustDesk-IP` | installed RustDesk | Connects by machine name → resolved address |
| `AnyDesk` | installed AnyDesk | Password piped on stdin |
| `UltraViewer` | installed UltraViewer | |
| `TightVNC` | bundled `tvnviewer.exe` | Shares `contacts_VNC.csv` |
| `UltraVNC` | bundled `vncviewer.exe` | Shares `contacts_VNC.csv` |

`TightVNC` and `UltraVNC` both set `BookFileName=contacts_VNC.csv`, so the same machine list serves
both viewers and you can try either against a host without duplicating anything.

The two VNC viewers are committed to the repo so the folder runs on a machine with neither
installed. UltraVNC's viewer is genuinely standalone — its imports are Windows system DLLs only,
and it writes `options.vnc` beside itself rather than into the registry. Its command-line options
**merge with** saved GUI defaults rather than replacing them, so a setting saved once sticks
regardless of what the INI passes.
