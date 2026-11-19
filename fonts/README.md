# Fonts

`inter/` holds the theme's font, **Inter** 4.1 by The Inter Project Authors
(https://github.com/rsms/inter), licensed under the SIL Open Font License 1.1 (`inter/LICENSE.txt`).
The files are the static, hinted TTFs of the release's `extras/ttf/` folder, unchanged: Regular,
Medium, SemiBold and Bold, the weights the theme uses.

- Nocturne.Wpf embeds them as resources (`Fonts/`, `pack://application:,,,/Nocturne.Wpf;component/Fonts/#Inter`).
  WPF cannot use the axes of a variable font, so it takes one static file per weight.
- Nocturne.Avalonia embeds them as Avalonia resources (`avares://Nocturne.Avalonia/Assets/Fonts#Inter`).
- Nocturne.Web serves the release's variable web fonts instead (`src/Nocturne.Web/wwwroot/fonts/`).
