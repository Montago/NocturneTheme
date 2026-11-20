---
title: Controls
parent: Nocturne for the web
nav_order: 3
has_children: true
---

# Controls

Every control of the WPF library has a CSS class, a tag helper with the WPF name and, where it needs
one, behaviour in `nocturne.js`. Native elements get the look from their class
(`<button class="nc-button">`, `<select class="nc-combo-box">`, `<details class="nc-expander">`), so they
still work without the script.

| Page | Controls |
| --- | --- |
| [Window and dialogs](window.md) | the window frame, title bar styles, window commands, the light, `nc-flyout`, dialogs |
| [Buttons](buttons.md) | `nc-button` and its variants, `nc-toggle-button`, `nc-repeat-button`, `nc-hyperlink`, `nc-drop-down-button`, `nc-split-button`, `nc-tile`, `nc-badged` |
| [Text and input](inputs.md) | `nc-text-box`, `nc-password-box`, `nc-label`, errors, `nc-combo-box`, `nc-numeric-up-down`, `nc-date-picker`, `nc-time-picker`, `nc-color-picker`, `nc-auto-suggest-box`, `nc-search-combo-box` |
| [Selection](selection.md) | `nc-check-box`, `nc-radio-button`, `nc-toggle-switch`, `nc-slider`, `nc-range-slider`, `nc-rating` |
| [Lists and data](lists.md) | `nc-list-box`, `nc-list-view`, `nc-tree-view`, `nc-data-grid`, `nc-nocturne-grid`, `nc-calendar`, `nc-scroll-viewer` |
| [Navigation and layout](navigation.md) | `nc-tab-control`, `nc-hamburger-menu`, `nc-expander`, `nc-accordion`, `nc-group-box`, `nc-menu`, `nc-context-menu`, `nc-tool-bar`, `nc-status-bar`, `nc-grid-splitter`, tool tips |
| [Progress and status](status.md) | `nc-progress-bar`, `nc-progress-ring`, `nc-busy-indicator`, `nc-info-bar` |
| [Typography and surfaces](typography.md) | text styles, `nc-card`, `nc-tag`, `nc-pill`, `nc-badge`, `nc-icon-tile`, `nc-stroke-icon`, `nc-icon` |

## On every page

- Each section has a picture of the control, the tag helper, the HTML it writes (for pages without
  Razor) and a table of its attributes. The picture comes from the web demo's gallery page
  (`/Demo/Gallery`), with drop-downs and menus open where the control has one, on the window's
  fallback ground as the WPF pictures are.
- Attributes are the WPF property names in kebab case; enum values are the WPF names
  (`button-style="Primary"`, `orientation="Vertical"`).
- Most control tag helpers take `is-enabled="false"`, `tool-tip="…"` and `validation-error="…"`, so the
  tables leave them out (the [reference](reference.md#attributes-every-control-takes) lists the
  exceptions).
- `class`, `style`, `id` and other plain attributes you write on a tag helper stay on the element it
  writes (for text boxes and combo boxes: on the `<input>` or `<select>`).
- Value controls raise `nc-change` with the new value in `event.detail`; the
  [reference](reference.md#events) lists them.

The [WPF pages](../controls.md) describe the same controls for the desktop.
