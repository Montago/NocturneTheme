---
title: Reference
parent: Nocturne for the web
nav_order: 5
---

# Reference

## Tag helpers

Every tag helper, with the page that describes it. Their attributes are the WPF property names in
kebab case.

| Tag helper | Writes | Page |
| --- | --- | --- |
| `nc-button`, `nc-toggle-button`, `nc-repeat-button` | `<button>` or `<a>` `.nc-button` | [Buttons](buttons.md) |
| `nc-hyperlink` | `<a class="nc-hyperlink">` | [Buttons](buttons.md#hyperlink) |
| `nc-drop-down-button`, `nc-split-button` | `.nc-drop-down-button`, `.nc-split-button` with a popover menu | [Buttons](buttons.md#dropdownbutton) |
| `nc-tile`, `nc-badged` | `.nc-tile`, `.nc-badged` | [Buttons](buttons.md#tile) |
| `nc-text-box`, `nc-password-box`, `nc-label` | `<input class="nc-text-box">`, `<label class="nc-label">` | [Text and input](inputs.md) |
| `nc-combo-box`, `nc-combo-box-item` | `<select class="nc-combo-box">` | [Text and input](inputs.md#combobox) |
| `nc-numeric-up-down` | `.nc-numeric-up-down` | [Text and input](inputs.md#numericupdown) |
| `nc-date-picker`, `nc-time-picker`, `nc-color-picker` | `.nc-date-picker`, `.nc-time-picker`, `.nc-color-picker` | [Text and input](inputs.md#datepicker) |
| `nc-auto-suggest-box`, `nc-search-combo-box` | `.nc-auto-suggest-box`, `.nc-search-combo-box` | [Text and input](inputs.md#autosuggestbox) |
| `nc-check-box`, `nc-radio-button`, `nc-toggle-switch` | `<label>` around a native input | [Selection](selection.md) |
| `nc-slider`, `nc-range-slider`, `nc-rating` | `.nc-slider`, `.nc-range-slider`, `.nc-rating` | [Selection](selection.md#slider) |
| `nc-list-box`, `nc-list-box-item` | `.nc-list-box` | [Lists and data](lists.md#listbox) |
| `nc-list-view`, `nc-grid-view-column` | `.nc-list-view` with a table | [Lists and data](lists.md#listview) |
| `nc-tree-view`, `nc-tree-view-item` | `<ul class="nc-tree-view">` | [Lists and data](lists.md#treeview) |
| `nc-data-grid`, `nc-data-grid-text-column` | `table.nc-data-grid` | [Lists and data](lists.md#datagrid) |
| `nc-nocturne-grid`, `nc-nocturne-grid-column`, `nc-nocturne-grid-check-box-column` | `.nc-nocturne-grid` | [Lists and data](lists.md#nocturnegrid) |
| `nc-calendar`, `nc-scroll-viewer` | `.nc-calendar`, `.nc-scroll-viewer` | [Lists and data](lists.md#calendar) |
| `nc-tab-control`, `nc-tab-item`, `nc-tab-control-tag` | `.nc-tab-control` | [Navigation and layout](navigation.md#tabcontrol) |
| `nc-hamburger-menu`, `nc-hamburger-menu-item` | `nav.nc-hamburger-menu` | [Navigation and layout](navigation.md#hamburgermenu) |
| `nc-expander`, `nc-accordion`, `nc-group-box` | `<details>`, `.nc-accordion`, `<fieldset>` | [Navigation and layout](navigation.md#expander) |
| `nc-menu`, `nc-menu-item`, `nc-context-menu`, `nc-separator` | `nav.nc-menu`, popover menus, `<hr>` | [Navigation and layout](navigation.md#menu) |
| `nc-tool-bar-tray`, `nc-tool-bar` | `.nc-tool-bar` | [Navigation and layout](navigation.md#toolbar) |
| `nc-status-bar`, `nc-status-bar-item` | `footer.nc-status-bar` | [Navigation and layout](navigation.md#statusbar) |
| `nc-grid-splitter` | `.nc-grid-splitter` | [Navigation and layout](navigation.md#gridsplitter) |
| `nc-progress-bar`, `nc-progress-ring` | `<progress>`, `.nc-progress-ring` | [Progress and status](status.md) |
| `nc-busy-indicator`, `nc-info-bar` | `.nc-busy-indicator`, `.nc-info-bar` | [Progress and status](status.md#busyindicator) |
| `nc-flyout` | `aside.nc-flyout` | [Window and dialogs](window.md#flyout) |
| `nc-text-block`, `nc-card`, `nc-tag`, `nc-pill`, `nc-badge`, `nc-icon-tile` | text and surfaces | [Typography and surfaces](typography.md) |
| `nc-stroke-icon`, `nc-icon` | `<svg>` with a symbol of the sprite | [Typography and surfaces](typography.md#line-icons) |
| `nc-tool-tip="…"` on any element | `data-nc-tool-tip` | [Navigation and layout](navigation.md#tooltip) |

## Attributes every control takes

The control tag helpers take three attributes of WPF's `Control`:

| Attribute | Default | Description |
| --- | --- | --- |
| `is-enabled` | `true` | `false` shows the control disabled (`disabled`, or `aria-disabled` where the element has no `disabled`) |
| `tool-tip` | | the [tool tip](navigation.md#tooltip) under the pointer (`data-nc-tool-tip`) |
| `validation-error` | | marks the control invalid (`aria-invalid="true"`) and shows the message in a callout under it while it has the focus or the mouse (`data-nc-error`) |

The tag helpers that only arrange other elements or draw a surface take none of them: `nc-tile`,
`nc-info-bar`, `nc-group-box`, `nc-card`, `nc-tag`, `nc-pill`, `nc-badge`, `nc-icon-tile`, `nc-icon`,
`nc-stroke-icon`, `nc-status-bar`, `nc-tool-bar`, `nc-tool-bar-tray`, `nc-context-menu`, `nc-separator`,
and the items and columns of the lists (`nc-combo-box-item`, `nc-list-box-item` and `nc-tab-item` take
`is-enabled`).

`class`, `style`, `id`, `aria-*`, `data-*` and other plain attributes stay on the element the tag helper
writes; for text boxes, numeric boxes and combo boxes they go on the `<input>` or `<select>`.

## Data attributes

`nocturne.js` works from classes and these attributes, so plain HTML can use them too.

| Attribute | On | Does |
| --- | --- | --- |
| `data-nc-open="id"` | a button | opens (or closes) that flyout, or opens that `<dialog>` |
| `data-nc-close` | a button in a flyout or dialog | closes it |
| `data-nc-auto-close="ms"` | `.nc-flyout` | closes it after that many ms |
| `data-nc-dismiss` | a button in an info bar | hides the bar |
| `data-nc-clear` | a button in `.nc-text-box-host` | empties the box |
| `data-nc-reveal` | a button in `.nc-text-box-host` | shows the password while pressed |
| `data-nc-step="1"` / `"-1"` | a button in `.nc-numeric-up-down` | steps the number |
| `data-nc-toggle` | a button | flips `aria-pressed` (ToggleButton) |
| `data-nc-repeat` | a button | clicks again while held (RepeatButton) |
| `data-nc-close-tab`, `data-nc-add-tab` | parts of document tabs | close a tab, ask for a new one |
| `data-nc-toggle-pane`, `data-nc-pane-switch` | a button, a check box | fold the window's hamburger menu |
| `data-nc-context-menu="id"` | any element | opens that context menu on right-click |
| `data-nc-align="right"` | `.nc-context-menu` | lines a menu up with the right edge of its button |
| `data-nc-tool-tip="text"` | any element | the tool tip |
| `aria-invalid="true"` + `data-nc-error="message"` | a field | the red frame and the error callout |
| `data-nc-culture`, `data-nc-first-day-of-week`, `data-nc-date-format` | a calendar, date picker or container | the culture, first day and format of the dates inside |
| `data-nc-accent` | a radio button | chooses the accent named by its value |
| `data-nc-accent-button="teal"` | a button | chooses that accent |
| `data-href="url"` | a data grid row | opens the address when the row is clicked (Ctrl: a new tab) |
| `data-sort-value` | a data grid cell | the value the column sorts by |
| `data-nc-sort="false"`, `data-nc-resize="false"` | a data grid or a `<th>` | turn sorting or resizing off |
| `data-busy="true"` | `.nc-busy-indicator` | shows it busy |
| `data-accent="name"` | `<html>` | the accent the page starts in |

## The script

`nocturne.js` sets everything up on `DOMContentLoaded`. For controls added later (a partial view loaded
with `fetch`), call `Nocturne.init(element)`.

| `window.Nocturne` | |
| --- | --- |
| `setAccent(nameOrColor)` | `'blue'`, `'green'`, `'orange'`, `'yellow'`, `'teal'`, `'system'` or `'#rrggbb'`; remembered in the browser |
| `NocturneTheme.accent`, `NocturneTheme.accents`, `NocturneTheme.onAccentChanged(fn)` | the current accent, the named ones, a listener |
| `init(element)` | sets up the controls inside an element added after the page loaded |
| `showDialog(options)`, `showMessage(title, message)` | a [dialog](window.md#dialogs); a promise of `'affirmative'` or `'negative'` |
| `openFlyout(idOrElement)`, `closeFlyout(idOrElement)` | a [flyout](window.md#flyout) |
| `formatDate(date, culture, format)`, `parseDate(text, culture, format)` | dates as the pickers write and read them |
| `nocturneGrid(idOrElement)` | the [API of a NocturneGrid](lists.md#from-script) |

## Events

The controls raise the usual `input` and `change` events on their inputs, and these, which bubble:

| Event | Raised by | `event.detail` |
| --- | --- | --- |
| `nc-change` | `nc-combo-box` | the chosen value |
| | `nc-list-box`, `nc-list-view`, `nc-tree-view`, `nc-data-grid` | the selected item or row element |
| | `nc-slider` | the value |
| | `nc-range-slider` | `{ lowerValue, upperValue }` |
| | `nc-rating` | the value |
| | `nc-calendar`, `nc-date-picker` | the `Date` (or `null`) |
| | `nc-time-picker` | the time as `HH:mm` |
| | `nc-color-picker` | the colour |
| | `nc-auto-suggest-box` | the suggestion taken |
| | `nc-search-combo-box` | `{ selectedItem, selectedValue }` |
| | `nc-tab-control` | `{ index, header, name }` |
| | `nc-toggle-button` | `true` or `false` |
| `nc-tab-closing` (cancelable), `nc-tab-closed` | document tabs | `{ index, name }`, `{ index }` |
| `nc-tab-add` | document tabs | |
| `nc-sorting` (cancelable), `nc-sorted` | `nc-data-grid`, `nc-nocturne-grid` | the column and direction; the sort |
| `nc-filtered` | `nc-nocturne-grid` | `{ count, totalCount }` |
| `nc-page-changed` | `nc-nocturne-grid` | `{ pageIndex, pageCount }` |
| `nc-selected-item-changed` | `nc-nocturne-grid` | `{ oldValue, newValue }` |
| `nc-load-failed` | `nc-nocturne-grid` in server mode | `{ error }` |
| `nc-open`, `nc-close` | `nc-flyout`; `nc-close` also `nc-info-bar` | |
| `nc-accent-changed` | `document` | the accent |

## NocturneGrid's server mode

`NocturneGridServer.Query(source, request, params string[] bindings)` answers a grid with
`items-source-url` from rows in memory ([Server mode](lists.md#server-mode)). For a database, read the
request and send back the same response yourself:

| `NocturneGridRequest` | |
| --- | --- |
| `PageIndex`, `PageSize` | the page asked for |
| `SearchText` | the words of the search |
| `GroupMemberPath` | the binding the rows are grouped by |
| `Culture` | the culture the grid formats and reads values in |
| `Columns` | each column's `Binding`, `StringFormat`, `IsCheckBox`, `TrueText`, `FalseText`, `IsSearchable`, `FilterText`, `FilterDates` |
| `Sorts` | `[{ Binding, Direction }]`, first column first |
| `ValuesFor`, `ValuesLimit` | set when a header filter asks for the values of that column instead of a page |

| `NocturneGridResponse` | |
| --- | --- |
| `Rows` | the page's rows, one cell per column, in the form the tag helper writes them |
| `TotalCount`, `FilteredCount` | the rows in all, and after the search and filters |
| `PageIndex` | the page sent (it may differ from the one asked for) |
| `Groups` | `[{ Key, Name, Start, Count }]` for a grouped grid |
| `Values`, `Dates` | the answer to `ValuesFor`: `[{ Text, Count }]`, or `[{ Day, Count }]` for dates |

## The CSS

`nocturne.css` starts with `@layer theme, base, nocturne-base, components, utilities;` and puts its rules
in `theme` (the tokens), `nocturne-base` and `components`, so Tailwind's utilities and any rule outside a layer win over them.
Class names follow the WPF controls and style keys:

| WPF | CSS |
| --- | --- |
| a control (`ComboBox`) | `.nc-combo-box` |
| a style key (`Nocturne.Styles.Button.Primary`) | `.nc-button.nc-button--primary` |
| a part (the title of a `Tile`) | `.nc-tile__title` |
| a state | ARIA and native states: `[aria-selected="true"]`, `[aria-pressed="true"]`, `:disabled`, `[open]` |

The colours are the [custom properties](theming.md#css-custom-properties) of `nocturne.tokens.css`,
generated from Nocturne.Wpf's XAML.
