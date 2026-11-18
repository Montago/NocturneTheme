# Plan: Nocturne.Web

Status: planned, nothing built yet (2026-11-18).

A web version of the Nocturne theme, built the way JetPack.Web is built in
`D:\Develop\JetpackTheme` (Razor class library with generated CSS, tag helpers and one
dependency-free script). It lives in this repo as a package of its own.

## No transparency

In WPF every surface is a white or black brush with alpha over the window's backdrop
(Acrylic / Mica). The web version has none of that:

- **No backdrop.** No `backdrop-filter`, no blur, nothing shows through the page. The page paints
  an opaque ground: `Window.Fallback` mixed with `Window.Tint` at 0.5, as WPF does over Remote
  Desktop (`#171E29` for blue). The token generator works it out per accent.
- **Popup surfaces are solid.** `Popup` (`#F71F2633`), menus, drop-downs, dialog and flyout get
  their colour flattened against the ground.
- **The light from the top-left corner stays,** drawn dithered on a `<canvas>` behind the page
  (like JetPack's vignette) and redrawn 150 ms after a resize. A plain CSS gradient this faint
  shows bands.
- **Open: alpha on hover, selected and card surfaces** (`Surface1` = `#0EFFFFFF` and so on).
  Keeping alpha over the solid ground looks the same as WPF and stacks correctly (a card inside a
  card). Decision: test it; if alpha performs well enough (scrolling long lists, NocturneGrid,
  animations), keep alpha. Otherwise flatten the tokens to solid colours in the generator.

Bonus: the WPF `--snapshot` already renders with the opaque ground, so the WPF gallery pictures
are the reference for the web pictures.

## Layout (mirrors JetPack)

```
src/Nocturne.Web/              Razor class library, net10.0, NuGet package Nocturne.Web
  wwwroot/css/nocturne.src.css      @layer order + imports (Tailwind and own utilities can override)
  wwwroot/css/nocturne.tokens.css   generated from Styles/*.xaml and the accent palettes
  wwwroot/css/controls/*.css        one file per Controls.*.xaml (text, buttons, toggles, range,
                                    containers, lists, calendar, menu, pickers, overlays, datagrid, window)
  wwwroot/css/nocturne.css          bundled; sites link it; never edited by hand
  wwwroot/js/nocturne.js            all behaviour, no dependencies
  wwwroot/img/icons.svg             generated sprite (the MDI set in icons/icons.json)
  TagHelpers/                       <nc-*> with WPF property names (is-enabled, button-style, ...)
  Grid/NocturneGridServer.cs        server mode of NocturneGrid
src/Nocturne.Web.Demo/         the six demo pages as .cshtml + /Demo/Gallery (one sample per gallery Tag)
samples/Nocturne.Web.Samples.Mvc   the admin sample as an MVC site (links its SampleData.cs)
tools/generate_web_tokens.py, bundle_web_css.py, web_snapshots.py; generate_icons.py also writes icons.svg
```

Naming as in JetPack: `ComboBox` -> `.nc-combo-box`, `Nocturne.Styles.Button.Primary` ->
`.nc-button--primary`, custom properties `--nc-*`, accent on `html[data-accent]`.

## Tokens and accents

- `generate_web_tokens.py` reads `Brushes.xaml`, `Fonts.xaml` and the palettes in
  `Controls/NocturneTheme.cs` (blue, green, orange, yellow, red). It writes `:root` and one
  `[data-accent=...]` block per accent, including the computed solid grounds and popup colours.
  CI checks that the tokens and `nocturne.css` are up to date.
- `NocturneTheme.setAccent('green' | '#hex' | 'system')` in JS. Hex and `system` use a port of
  the `System` derivation in `NocturneTheme.cs` (ground from the hue, white or dark `OnAccent` by
  luminance). `system` reads CSS `AccentColor` where the browser has it, else blue. The choice
  is kept in `localStorage`.
- Font: Segoe UI with `system-ui` as fallback; `--nc-font-family` can be overridden.

## What changes on the web

- Gone: `Backdrop`, `TintOpacity`, `UseBackdropInRemoteSession`, the RDP logic, `CornerStyle`,
  caption buttons, the WM_ messages.
- NocturneWindow becomes `<nc-window>`: a page frame with title bar (Seamless, Separated, Accent)
  and Left/Right window commands.
- `ChainMouseWheel` is not needed (browsers chain scrolling). Auto-hiding scroll bars: styled
  scroll bars plus a hover class.
- Popups use the Popover API with JS placement as fallback. The dialog is `<dialog>`. Flyout moves
  focus in and gives it back, as in WPF.
- RichTextBox: at most a basic `contenteditable` version, only if wanted.

## Phases

1. **Skeleton and tools:** projects, token generator, bundler, CI checks, the light canvas and
   ground, demo shell with the Tokens page, `web-v*` tags in `release.yml`. Test alpha vs. solid
   surfaces here (see above).
2. **Basic controls:** typography, icons, buttons (all variants, hyperlink, tile, badged),
   check box / radio / switch, text fields with the validation callout, password box, slider,
   progress bar.
3. **Containers and lists:** TabControl (segmented with the gliding indicator, document tabs),
   expander, accordion, group box, card, ListBox (+ Navigation), ListView, TreeView, InfoBar,
   BusyIndicator, ProgressRing, tool tip, StatusBar, ToolBar, GridSplitter.
4. **Popups and pickers:** ComboBox, Menu and ContextMenu (text menu too), DropDown/SplitButton,
   Calendar and DatePicker, TimePicker, ColorPicker, AutoSuggestBox, SearchComboBox, NumericUpDown,
   RangeSlider, Rating. Keyboard as in WPF (Alt+Down / F4, arrows, Enter, Escape).
5. **Window and overlays:** `nc-window` with the title bar styles, HamburgerMenu (also folded),
   dialog, flyout.
6. **NocturneGrid and DataGrid:** filters, sorting, grouping with alternating rows, paging, client
   and server mode. The rules stay the same in `NocturneGrid.cs`, the JS and `NocturneGridServer.cs`.
7. **Samples, pictures and docs:** the MVC sample; `web_snapshots.py` (headless Edge) compared with
   the WPF gallery; `docs/web/` with the same page names; `PACKAGE.md` and `CLAUDE.md` updated
   (docs only at release, as the rule says).

After each phase: build, gallery pictures into a scratch folder, compare with the WPF picture of
the same name.
