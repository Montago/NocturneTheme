# Plan: Nocturne.Web

Status: built and merged into master (2026-11-19, PR #3): the package (CSS, script, tag helpers,
NocturneGrid with server mode, icons), the demo pages, the gallery, the MVC sample, the tools
(`generate_web_tokens.py`, `bundle_web_css.py`, `web_snapshots.py`), CI checks and `web-v*` release
tags. Docs written and the docs split per library as JetPack's (2026-11-20); first release
`web-v0.1.0`. How it works and its rules are in CLAUDE.md ("Nocturne.Web"). Left: the differences and
checks below.

Decided: no transparency to the desktop (opaque ground, solid popups); alpha is kept on hover, card
and selection surfaces.

## Docs and the first release (done 2026-11-20)

- `docs/web.md` and `docs/web/*.md` (same page names as the WPF docs, as JetPack's web docs);
  `docs/index.md` compares the three packages, the WPF pages sit under `wpf.md`.
- `tools/update_docs.py --only web` runs `web_snapshots.py` and checks that every tag helper is
  mentioned in `docs/web/`; the `/update-docs` skill describes it.

## For the next release's docs

Remove each line once it is in the docs.
- `docs/web/lists.md`, TreeView: a click on the chevron folds an item (it did nothing before 0.1.1), a
  double-click on its text folds it too, Left / Right fold it shut and open (or move to the parent /
  first child), and the arrow keys skip items in folded branches.

## Differences from WPF still open

- The ListView sample has no file icons.
- NocturneGrid's filter counts differ from WPF's (the logic came from JetPack.Web); make it count
  as `NocturneGrid.cs` does.
- A grouped NocturneGrid marks every other row in a group `is-alternate`, but the class draws nothing.
  Compare with the WPF `nocturnegrid-groups` picture.
- Chromium draws text a little heavier than WPF.

## Not checked yet

- The web pictures have not been compared with WPF again since the switch to Inter (7d4757d).
- By hand in a browser: the keyboard on the pickers (Alt+Down / F4, arrows, Enter, Escape), focus in
  the flyout and dialog, and the folded HamburgerMenu.
- Performance of the alpha surfaces on a long NocturneGrid and over Remote Desktop, in Firefox and
  Safari as well as Edge.
