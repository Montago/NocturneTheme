"""Regenerates docs/resource-keys.md from the theme dictionaries.

Usage (from the repository root): python tools/generate_resource_keys.py
"""
import glob
import os
import re
import sys
import xml.etree.ElementTree as ET

ROOT = sys.argv[1] if len(sys.argv) > 1 else os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
STYLES = os.path.join(ROOT, 'src', 'Nocturne.Wpf', 'Styles')
X = '{http://schemas.microsoft.com/winfx/2006/xaml}'


def local(tag):
    return tag.split('}')[-1]


def describe(el):
    t = local(el.tag)
    if t == 'SolidColorBrush':
        value = el.get('Color') or ''
        return 'SolidColorBrush', f"`{value}`" if not value.startswith('{') else f"{value}"
    if t in ('LinearGradientBrush', 'RadialGradientBrush'):
        stops = [f"`{s.get('Color')}` {s.get('Offset') or '0'}" for s in el.iter() if local(s.tag) == 'GradientStop']
        return t, ' → '.join(stops)
    if t in ('Color', 'FontFamily', 'Double', 'String', 'Thickness', 'CornerRadius', 'FontWeight'):
        return t, f"`{(el.text or '').strip()}`"
    if t == 'DropShadowEffect':
        parts = [f"{k} {el.get(k)}" for k in ('Color', 'BlurRadius', 'ShadowDepth', 'Direction', 'Opacity') if el.get(k)]
        return 'DropShadowEffect', ', '.join(parts)
    return t, ''


def table(file, title, intro):
    rows = []
    for el in ET.parse(os.path.join(STYLES, file)).getroot():
        key = el.get(X + 'Key')
        if key:
            kind, value = describe(el)
            rows.append(f"| `{key}` | {kind} | {value} |")
    return f"## {title}\n\n{intro}\n\n| Key | Type | Value |\n| --- | --- | --- |\n" + '\n'.join(rows) + '\n'


def icons():
    keys = re.findall(r'x:Key="(Nocturne\.Icons\.[^"]+)"', open(os.path.join(STYLES, 'Icons.xaml'), encoding='utf-8').read())
    return ("## Icons\n\nLine icons (24×24 geometries) for `nc:StrokeIcon` and `ControlHelper.Icon`, as in "
            "[Typography and surfaces](typography.md#icons).\n\n" + ', '.join(f"`{k}`" for k in keys) + '\n')


def style_keys():
    out = []
    for f in sorted(glob.glob(os.path.join(STYLES, 'Controls.*.xaml'))):
        s = open(f, encoding='utf-8').read()
        for m in re.finditer(r'<Style\b[^>]*>', s):
            tag = m.group(0)
            key = re.search(r'x:Key="(Nocturne\.Styles\.[^"]+)"', tag)
            target = re.search(r'TargetType="([^"]+)"', tag)
            if key:
                out.append(f"| `{key.group(1)}` | `{target.group(1) if target else ''}` | {os.path.basename(f)} |")
    return out


doc = ["---", "title: Resource keys", "parent: Reference", "grand_parent: Nocturne for WPF", "nav_order: 1", "---", "", "# Resource keys", "",
       "Every brush, colour, font size, icon and named style the theme defines, generated from the XAML in",
       "`src/Nocturne.Wpf/Styles` by `tools/generate_resource_keys.py`. Reference brushes with",
       "`{DynamicResource ...}` so they follow accent changes.", "",
       "To change a value app-wide, redefine the key in `App.xaml` after merging `Nocturne.xaml`, for example",
       "`<SolidColorBrush x:Key=\"Nocturne.Brushes.Error\" Color=\"#FFFF6F66\" />`. The accent brushes are",
       "rewritten by `NocturneTheme.SetAccent`, so change those with an accent instead "
       "([Theme and accents](theming.md)).", ""]
doc.append(table('Brushes.xaml', 'Brushes and colours',
                 'Values shown are the default blue accent. `NocturneTheme.SetAccent` writes new values for the accent '
                 'brushes, `Text.OnAccent`, the primary button, `Popup`, `Window.Tint` and `Window.Fallback`.'))
doc.append(table('Fonts.xaml', 'Fonts', 'The font family and the type scale. Override `Nocturne.Fonts.Family` to use another font.'))
doc.append(icons())
doc.append("## Named styles\n\nEvery standard control also gets an implicit style (no key needed). These keyed styles are the\n"
           "variants and building blocks; use them with `Style=\"{StaticResource ...}\"` or as `BasedOn`.\n\n"
           "| Key | Target type | File |\n| --- | --- | --- |\n" + '\n'.join(style_keys()) + '\n')
# CRLF, as Git checks text files out on Windows (core.autocrlf), so a run leaves no diff
open(os.path.join(ROOT, 'docs', 'resource-keys.md'), 'w', encoding='utf-8', newline='\r\n').write('\n'.join(doc))
print('ok')
