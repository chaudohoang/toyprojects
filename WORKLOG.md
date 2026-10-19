# Work log

Newest first. One section per working day. Keep entries short: what changed, where, and
anything that would be expensive to rediscover. Code detail belongs in git; this is the
"why / what happened" that git messages don't carry.

---

## 2026-10-19

- **Moved `CLAUDE.md` and `WORKLOG.md` from `E:\Github` into `toyprojects`** so both travel
  with a `git pull` instead of living only on the main PC. `<root>` now means "wherever
  `toyprojects` is cloned", so a PC that clones elsewhere needs no edit.
  - **Trade-off accepted:** this file only auto-loads for a session started *inside*
    `toyprojects`. Working at `<root>` on the upstream clones (YgoMaster, the Yu-Gi-Oh
    recomps) no longer picks it up. Worth it, since that is where almost no work happens.
  - Two things stay machine-specific and need checking on a new PC: the deploy folder path
    (`E:\PortableApps\MultiRemoteTool`) and the `C:\Program Files\...` paths inside the
    tool INIs, which depend on what is installed there.

## 2026-10-17

- **`MultiRemoteTool`: new `RustDesk-IP` platform — book holds PC names, address is found
  at connect time** (`8b09629`). New `{ip}` placeholder in the command template; a
  platform using it resolves the contact's Id to an address before launching, so a machine
  moving between the LAN and the tailnet needs no book edit.
  - `HostResolver` gathers candidates from three sources **run concurrently**: DNS,
    `tailscale ip -4 <name>`, and the `direct <lan-ip>:41641` field of `tailscale status`.
    Serially an unknown name cost 5.5s before the error dialog; concurrently 2.8s, and a
    live host resolves in ~40-50ms.
  - **The `tailscale status` scrape is the important one.** Once MagicDNS is answering,
    `Dns.GetHostAddresses` returns *only* the 100.x address, so that status line is the
    only way to learn a peer's LAN IP. It is present only while that peer has a live
    direct connection — if Tailscale is relaying through DERP, there is no LAN candidate
    at all and the tailnet address is used. Expect the same host to resolve LAN one minute
    and Tailscale the next; that is correct behaviour, not a bug. (`i5-3470` did exactly
    this during testing.)
  - **Selection is by rank, not by who answers first.** Candidates sort LAN
    (10/8, 172.16/12, 192.168/16) before Tailscale (100.64/10) before anything else, then
    all are TCP-probed on `ProbePort` in parallel; the best-ranked address that *answered*
    wins. Parallelism only caps the wait at one timeout — it does not decide the winner.
    To invert the preference, swap the two return values in `HostResolver.Rank()`.
  - **Port 21118 is confirmed, not guessed.** RustDesk `src/client.rs:441` routes a
    direct-IP peer through `check_port(peer, RELAY_PORT + 1)` and `RELAY_PORT` is 21117
    (`hbb_common/src/config.rs:121`). `is_ipv4_str("1.2.3.4:90")` is true in their test
    suite, so the explicit `host:port` form is valid. Probing the tailnet confirmed 21118
    open on the live peers, i.e. Direct IP Access is already enabled on Chau's machines.
  - `ProbePort` is a new optional INI key, editable in the platform dialog and carried
    through platform edit and export. Platforms without `{ip}` parse to `ProbePort=0` and
    skip resolution entirely — all five pre-existing platforms verified unaffected.
  - **Not tested end-to-end:** no live RustDesk session was opened. Resolution, INI
    parsing, the final argument string and GUI startup are verified; RustDesk's own
    handling of those args is not. Chau said he would test it himself.

- **`MultiRemoteTool`: sidebar no longer truncates long platform names** (`6b39548`).
  "RustDesk-IP" needs 81px at Segoe UI 9.5 Bold; a *selected* row offered 71, so it drew
  as "RustDes...". Two causes: the contact-count badge reserved a flat 40px regardless of
  content (a 1-2 digit badge is 19-25px), and unselected rows reserved nothing at all, so
  a long enough name could run under the badge. Now both reserve `BadgeWidth()`, the same
  helper the badge draws with. Sidebar also went 240 -> 264px, leaving 104px for the name
  with the four action buttons showing.

- **`MultiRemoteTool`: README** (`6b5d536`). Platform model, every INI key and placeholder, the
  executable search order, the UI, and a long `{ip}` section. Copied to the deploy folder too, so
  it now exists in two places and can drift — update both.

- **`MultiRemoteTool`: `{ip}` is now visible in the UI** (`8f99afe`). It was the only placeholder
  with a side effect yet looked like the equal of `{pw}`, so a resolving platform was
  indistinguishable from a plain one without opening the edit dialog and already knowing what
  `{ip}` meant. Header subtitle now reads `rustdesk.exe ✓ · name → IP` in amber with the full
  behaviour in its tooltip; status bar says the Remote ID is a machine name; the dialog gives
  `{ip}` its own amber line.
  - **The header subtitle has very little room** — it shares its line with the search box, and at
    the 820px minimum window width `titleWidth` collapses to **60px** (186px at 1020). Even
    `rustdesk.exe ✓` is 86px, so it already clipped at minimum size before this change. Anything
    added there must be short; detail belongs in a tooltip.
  - Found while testing: the **Probe Port hint had been clipping its last word** since it was
    added (288px of text in a 268px label). Reworded.
  - Verified by rendering `PlatformDialog` directly through reflection in an `-STA` pwsh process
    and screenshotting it — much faster than driving the real UI, and it catches wrap/overlap
    that measurement alone misses.

- **Trap, cost ~10 min:** launching `MultiRemoteTool.exe` to smoke-test **rewrites
  `settings.txt`** — it saves the selected platform as the first entry of
  `LoadedPlatforms`. This silently dirtied the repo copy during testing. Check
  `git status` after any GUI test run.

## 2026-10-16

- **`MultiRemoteTool`: `VNC` platform split into `TightVNC` + `UltraVNC`** (`7d8ae3a`),
  after Chau installed UltraVNC 1.8.3. Both set `BookFileName=contacts_VNC.csv` so they
  share one address book — that INI key is the mechanism for pointing two platforms at the
  same CSV.
  - UltraVNC arg translation from the TightVNC line: `-autoscaling` for `-scale=auto`, and
    **no fullscreen flag at all** — UltraVNC's `-fullscreen` is a bare flag with no `=no`
    form, unlike TightVNC's. Added `-quickoption 1` and `-disablesponsor`.
  - **No `-password`:** `contacts_VNC.csv` carries no passwords, and an empty `{pw}` would
    make `-password` swallow the next argument. Needs a third CSV column first.
  - **UltraVNC viewer runs standalone** — its PE import table is Windows system DLLs only,
    and it launched from a folder containing nothing but the exe. The companion files in
    its install dir belong to the *server* or are optional (`SecureVNCPlugin64.dsm` loads
    only with `-dsmplugin`). So `vncviewer.exe` is bundled in the repo and the deploy
    folder, referenced by bare name.
  - **Bare name is deliberate:** `ResolveExe` scans only one directory level under Program
    Files, and UltraVNC installs two deep (`Program Files\uvnc\UltraVNC\`), so a bare name
    cannot accidentally resolve to the installed copy — it falls through to the app folder.
  - UltraVNC writes `options.vnc` next to the exe rather than only to `HKCU`, so the
    bundled copy is genuinely portable. Its command-line options **merge with** saved
    defaults rather than replacing them — a setting saved in the GUI sticks regardless of
    args.
  - `LoadedPlatforms` in `settings.txt` is a strict whitelist: a new `.ini` on disk stays
    invisible until it is listed there, and the first entry is the auto-selected platform.

## 2026-10-09

- **`toyprojects`: deleted the stale `ftprecovery-no-duplicate-manifests` branch**, leaving
  `main` as the only branch locally and on GitHub. It pointed at `89f580a`, the same commit
  as `main`, so nothing was lost.
