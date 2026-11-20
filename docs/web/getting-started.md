---
title: Getting started
parent: Nocturne for the web
nav_order: 1
---

# Getting started

## Install

Nocturne.Web is a [NuGet package on nuget.org](https://www.nuget.org/packages/Nocturne.Web); each
`web-v*` [GitHub release](https://github.com/Montago/NocturneTheme/releases) has the `.nupkg` too.

```bash
dotnet add package Nocturne.Web
```

It is a Razor class library: its CSS, script, icons and font are served from
`~/_content/Nocturne.Web/` by the static files middleware (`app.UseStaticFiles()` or
`app.MapStaticAssets()`), and its tag helpers work in MVC views and Razor Pages.

Add the tag helpers in `_ViewImports.cshtml`:

```cshtml
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, Nocturne.Web
@using Nocturne.Web.TagHelpers
```

The `@using` lets views name enum values such as `NocturneIconKind.Bell` in C#; attributes like
`icon="Bell"` work without it.

## Load the theme

Load the stylesheet and the script in the layout's `<head>`. Leave `defer` off the script: it sets the
stored accent before the page is drawn, so the page never flashes in another colour. Everything else
starts on `DOMContentLoaded`.

```html
<link rel="stylesheet" href="~/_content/Nocturne.Web/css/nocturne.css" asp-append-version="true" />
<script src="~/_content/Nocturne.Web/js/nocturne.js" asp-append-version="true"></script>
```

Load your own stylesheet after these. Nocturne's rules sit in cascade layers, so any rule outside a
layer wins over them without `!important`.

## The window

Make `<body>` the window: the opaque ground, the light from the top-left corner, a 44px title bar and
the content under it. The browser draws the real caption buttons, so the page has none.

```cshtml
<body class="nc-window">
    <canvas class="nc-window__light" aria-hidden="true"></canvas>

    <header class="nc-title-bar">
        <span class="nc-title-bar__icon"></span>
        <span class="nc-title-bar__title">My app</span>
        <div class="nc-window-commands">
            <nc-button button-style="IconSubtle" stroke-icon="Sync" aria-label="Synchronise" tool-tip="Synchronise" />
        </div>
    </header>

    <div class="nc-window__body">
        <main class="nc-window__content">
            @RenderBody()
        </main>
    </div>
</body>
```

[Window and dialogs](window.md) has the rest: commands before the title, the title bar styles, a
navigation pane, the status bar, flyouts and dialogs.

## The first page

Write the controls as you would in XAML, with the property names in kebab case:

```cshtml
<h1 class="nc-text-block--title">Settings</h1>

<nc-card header="SYNCHRONISATION">
    <nc-toggle-switch name="Background" is-checked="true">Synchronise in the background</nc-toggle-switch>
    <nc-label content="Server" target="Server" />
    <nc-text-box id="Server" name="Server" text="intranet.local" clear-button="true" />
    <nc-button button-style="Primary" type="submit">Save</nc-button>
</nc-card>
```

Each tag helper writes ordinary HTML with Nocturne classes (`<label class="nc-toggle-switch"><input
type="checkbox" role="switch">…`), so the pages work without the script where HTML can: links,
buttons, check boxes, text boxes, `<select>`, `<details>`. The script adds drop-downs, calendars,
sliders, menus, tab switching, flyouts, dialogs and the grids.

## Forms and model binding

The field tag helpers take `asp-for` like ASP.NET Core's own, and then write the name, id, value and
validation attributes from the model:

```cshtml
<nc-label content="Hours per week" target="HoursPerWeek" />
<nc-numeric-up-down asp-for="HoursPerWeek" minimum="0" maximum="40" interval="0.5" />
<span class="nc-validation-message" asp-validation-for="HoursPerWeek"></span>

<nc-check-box asp-for="IsActive" content="Active" />
<nc-toggle-switch asp-for="SendReports">Send weekly reports</nc-toggle-switch>
<nc-combo-box asp-for="Department" asp-items="Model.Departments" placeholder="Choose a department" />
```

`asp-for` works on `nc-text-box`, `nc-password-box`, `nc-numeric-up-down`, `nc-combo-box`,
`nc-check-box` and `nc-toggle-switch`. The others post with `name`: `nc-date-picker` posts its date as
`yyyy-MM-dd`, `nc-time-picker` as `HH:mm`, `nc-slider` its value, `nc-range-slider` the fields
`<name>.LowerValue` and `<name>.UpperValue`, `nc-list-box` the selected item's value.

A field that failed validation on the server (`input-validation-error`) gets the red frame; so does any
field with `aria-invalid="true"`. With `validation-error="…"` on a tag helper, the message also shows
in a callout under the field while it has the focus or the mouse (see [Errors](inputs.md#errors)).
`.nc-validation-message` is the red text under a field for `asp-validation-for`.

## Plain HTML

The CSS and the script do not need ASP.NET Core. Copy `nocturne.css` (it carries the tokens),
`nocturne.js`, `img/icons.svg` and `fonts/` from the package's `staticwebassets` folder, and write the
markup the tag helpers would write. Each control page shows that HTML under the Razor example.

## Tailwind

`nocturne.tailwind.css` maps the tokens to Tailwind v4 theme variables, so utilities such as
`text-nc-secondary`, `bg-nc-surface`, `border-nc-stroke` or `font-nc` follow the theme and the accent.
Load Nocturne first: its `@layer` statement puts the Nocturne layers between Tailwind's, so utilities
win over the controls.

```css
@import "<path>/nocturne.css";
@import "tailwindcss";
@import "<path>/nocturne.tailwind.css";
```

The Tailwind browser build cannot import files; copy the `@theme` block of `nocturne.tailwind.css` into
a `<style type="text/tailwindcss">` element instead, as the [MVC sample](samples.md#the-mvc-sample) does.
