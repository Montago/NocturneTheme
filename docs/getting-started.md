---
title: Getting started
nav_order: 2
---

# Getting started

## Install

Nocturne.Wpf is a [NuGet package on nuget.org](https://www.nuget.org/packages/Nocturne.Wpf); each
[GitHub release](https://github.com/Montago/NocturneTheme/releases) has the `.nupkg` too.

```bash
dotnet add package Nocturne.Wpf
```

The project must target `net10.0-windows` or later with `<UseWPF>true</UseWPF>`.

## Merge the theme

Merge the one dictionary in `App.xaml`. It styles every standard control and defines the brushes,
fonts, icons and named styles.

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/Nocturne.Wpf;component/Styles/Nocturne.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

## The first window

Make the main window a `NocturneWindow`: it draws the Windows 11 backdrop, the tint and the title bar.

```xml
<nc:NocturneWindow x:Class="MyApp.MainWindow"
                   xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                   xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                   xmlns:nc="urn:nocturne-wpf"
                   Title="My app" Width="1100" Height="720">
    <StackPanel Margin="24">
        <TextBlock Style="{StaticResource Nocturne.Styles.TextBlock.Title}" Text="Settings" />
        <HeaderedContentControl Margin="0,20,0,0" Style="{StaticResource Nocturne.Styles.Card}" Header="SYNCHRONISATION">
            <StackPanel>
                <CheckBox Style="{StaticResource Nocturne.Styles.ToggleSwitch}" Content="Synchronise in the background" IsChecked="True" />
                <Button Margin="0,16,0,0" HorizontalAlignment="Left" Style="{StaticResource Nocturne.Styles.Button.Primary}" Content="Save" />
            </StackPanel>
        </HeaderedContentControl>
    </StackPanel>
</nc:NocturneWindow>
```

```csharp
public partial class MainWindow : Nocturne.Wpf.Controls.NocturneWindow
{
    public MainWindow() => InitializeComponent();
}
```

A plain `Window` works too, with the system title bar and no backdrop; give it the theme's ground
yourself with `Background="{DynamicResource Nocturne.Brushes.Window.Fallback}"`.

## Conventions

- XML namespace: `xmlns:nc="urn:nocturne-wpf"` covers `Nocturne.Wpf.Controls` and `Nocturne.Wpf.Helpers`.
- Standard controls need nothing: merging `Nocturne.xaml` gives them the Nocturne style. Variants are
  keyed styles named `Nocturne.Styles.<Control>.<Variant>`.
- Brushes are `Nocturne.Brushes.*`. Use `DynamicResource` for them in your own XAML, so they follow
  accent changes. Every key is listed in [Resource keys](resource-keys.md).
- Controls are made to sit on cards (`Nocturne.Styles.Card`) over the window, as in the pictures.
- Transitions are 120 ms on backgrounds and borders and 150 ms on switch knobs and chevrons.

## The demo

`src/Nocturne.Wpf.Demo` shows every control on five pages and has the window settings (backdrop,
tint, accent) in its side bar. Run it from Visual Studio or with:

```bash
dotnet run --project src/Nocturne.Wpf.Demo
```
