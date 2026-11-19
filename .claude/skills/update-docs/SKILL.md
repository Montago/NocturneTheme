---
name: update-docs
description: Bring the Nocturne documentation up to date with the code - screenshots of the demo and the control gallery, docs/resource-keys.md, and the docs pages for anything that changed. Use before a release (a v*.*.* or avalonia-v*.*.* tag), or when the user asks to update the docs or the screenshots. Not after ordinary code changes - docs and screenshots change only for releases.
---

# Update the documentation

The mechanical part is `tools/update_docs.py`; your part is to look at the result and make the text
match the code. Work in the repository root.

## 1. Run the script

```bash
python tools/update_docs.py
```

For a release of one library, add `--only wpf` or `--only avalonia`: only that library's pictures are
rendered and copied (Nocturne.Avalonia's go to `docs/screenshots/avalonia/`, drawn headless, so a
locked screen does not matter there), and `--only avalonia` leaves `docs/resource-keys.md` alone.

It builds the demo and the two sample apps into a scratch folder (never into `bin/`, which Visual
Studio may have locked), runs them with `--snapshot`, copies the page pictures to `docs/screenshots/`,
the gallery to `docs/screenshots/controls/` and the sample apps' pages to `docs/screenshots/samples/`,
regenerates `docs/resource-keys.md` and runs the checks. It takes a minute or two.

- **"pictures ... are blank"**: the screen is off or the session is locked, and WPF then renders
  nothing. Ask the user to turn the screen on or connect with RDP, then run it again. Do not commit
  anything from a blank run.
- **Build errors**: fix them first; the docs must describe code that builds.
- **StaticResource errors**: a `{StaticResource}` in `Styles/` that is not defined earlier in the same
  file fails at runtime. Change it to `DynamicResource` (or move the resource) as CLAUDE.md describes.
- `python tools/update_docs.py --check` runs only the checks (no build, no pictures), which is enough
  after text-only edits.

## 2. Look at what changed

The script lists new and changed pictures. Endless animations are frozen and the gallery uses fixed
dates, so a picture only changes when the look changed. Read every new or changed PNG and check it
against the change that caused it: no clipped text, no blank areas, open drop-downs where the gallery
marks them (`demo:Gallery.Open`), popups inside the picture. If a picture is wrong, fix the cause (the
style, or the sample in `src/Nocturne.Wpf.Demo/Views/GalleryPage.xaml`) and run the script again.

## 3. Check the text against the code

Docs change only for releases, so the code changes since the last commit that touched `docs/` are
the ones the text has not caught up with yet:

```bash
git log -1 --format="%h %s" -- docs
git diff --stat <that commit>..HEAD -- src samples
git log --oneline <that commit>..HEAD
git diff HEAD -- docs/resource-keys.md
```

Then, for each change:

- **New or renamed control, property or style key**: the matching page in `docs/` (see the table in
  `docs/controls.md`) describes it, with a XAML example and, for properties, the table row. Give a new
  control a sample in `GalleryPage.xaml` (`Style="{StaticResource Demo.Sample}" Tag="<file name>"`)
  and show `screenshots/controls/<file name>.png` under its heading. The script warns about controls
  that no page mentions and gallery pictures that no page shows.
- **Changed look or behaviour**: the description under the control's heading still matches the new
  picture (colours, sizes, what happens on hover or click).
- **Removed things**: no page mentions them any more.
- **Sample apps** (`samples/`): `docs/samples.md` describes both apps page by page with their
  pictures from `screenshots/samples/` (the script warns about pictures no page shows). If the page
  does not exist yet, write it: how to run each app, then one section per page saying which controls
  it shows, with its picture.
- `README.md` and `src/Nocturne.Wpf/PACKAGE.md` (the nuget.org page) still match, including the list
  of own controls.
- **Nocturne.Avalonia**: `docs/avalonia.md` (window, accents, how it differs from WPF),
  `docs/avalonia-controls.md` (one section per gallery sample of `src/Nocturne.Avalonia.Demo/Views/GalleryPage.axaml`,
  pictures from `screenshots/avalonia/controls/`), `docs/avalonia-samples.md` and
  `src/Nocturne.Avalonia/PACKAGE.md`. The script warns about Avalonia controls those pages do not mention
  and pictures no page shows.
- `CLAUDE.md` gets any new pitfall that was found along the way.

Everything in the repository is English, also when the user writes in Danish.

## 4. Finish

Run `python tools/update_docs.py --check` once more; it must end with 0 errors. Tell the user what
changed (pictures and pages), what you fixed, and anything you could not verify. Commit or push only
when the user asks; the docs update is one commit of its own (e.g. "Update docs and screenshots for
Nocturne.Wpf 1.2.3"), made before the release tag.
