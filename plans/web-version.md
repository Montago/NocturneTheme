# Plan: Nocturne.Web

Status: built and merged into master (2026-11-19, PR #3): the package (CSS, script, tag helpers,
NocturneGrid with server mode, icons), the demo pages, the gallery, the MVC sample, the tools
(`generate_web_tokens.py`, `bundle_web_css.py`, `web_snapshots.py`), CI checks and `web-v*` release
tags. How it works and its rules are in CLAUDE.md ("Nocturne.Web"). Left: docs and the first release,
then the differences and checks below.

Decided: no transparency to the desktop (opaque ground, solid popups); alpha is kept on hover, card
and selection surfaces.

## Docs and the first release

- A web section in `docs/` (Just the Docs) with the same page names as the WPF docs: the tag
  helpers, classes, `Nocturne.setAccent` and the icons. `PACKAGE.md` points at
  `https://montago.github.io/NocturneTheme/web.html`.
- Pictures: `web_snapshots.py` writes `docs/screenshots/web/` (`PACKAGE.md` shows
  `web/controls.png`) and `docs/screenshots/samples/web-*.png`. Hook it into `tools/update_docs.py`
  and the `/update-docs` skill, with a check that every web control is documented.
- nuget.org: the Trusted Publishing policy must allow the new package `Nocturne.Web` before the first
  `web-v0.1.0` tag.

## Differences from WPF still open

- The ListView sample has no file icons.
- NocturneGrid's filter counts differ from WPF's (the logic came from JetPack.Web); make it count
  as `NocturneGrid.cs` does.
- The BusyIndicator does not blur the content behind it (WPF does).
- A grouped NocturneGrid marks every other row in a group `is-alternate`, but the class draws nothing.
  Compare with the WPF `nocturnegrid-groups` picture.
- Chromium draws text a little heavier than WPF.

## Not checked yet

- The web pictures have not been compared with WPF again since the switch to Inter (7d4757d).
- By hand in a browser: the keyboard on the pickers (Alt+Down / F4, arrows, Enter, Escape), focus in
  the flyout and dialog, and the folded HamburgerMenu.
- Performance of the alpha surfaces on a long NocturneGrid and over Remote Desktop, in Firefox and
  Safari as well as Edge.
