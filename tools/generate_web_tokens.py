"""Writes src/Nocturne.Web/wwwroot/css/nocturne.tokens.css, the CSS custom properties of Nocturne for
the web, from the WPF dictionaries:

    Styles/Brushes.xaml     every brush as --nc-<key> (Nocturne.Brushes.Surface1.Border -> --nc-surface1-border)
    Styles/Fonts.xaml       --nc-font-family, --nc-font-family-mono, --nc-font-size-<name>
    Controls/NocturneTheme.cs   the accent palettes, one [data-accent="<name>"] block each (blue in :root)
    Styles/Icons.xaml       --nc-glyph-<name>: the line icons the CSS draws as masks (GLYPHS)

The web version has no window backdrop, so nothing on the page is see-through to the desktop:
    --nc-window-ground   Window.Fallback with Window.Tint over it at the window's TintOpacity (0.5),
                         the ground NocturneWindow paints over Remote Desktop
    --nc-popup           Nocturne.Brushes.Popup flattened onto that ground (solid)
The other surfaces keep their alpha over the solid ground, as in WPF.

Run it after changing a brush, a font or a palette, then python tools/bundle_web_css.py:
    python tools/generate_web_tokens.py           write the file
    python tools/generate_web_tokens.py --check   exit with 1 when the file is out of date
"""
import os
import re
import sys
import xml.etree.ElementTree as ET

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WPF = os.path.join(ROOT, 'src', 'Nocturne.Wpf')
OUT = os.path.join(ROOT, 'src', 'Nocturne.Web', 'wwwroot', 'css', 'nocturne.tokens.css')
NS = '{http://schemas.microsoft.com/winfx/2006/xaml/presentation}'
X = '{http://schemas.microsoft.com/winfx/2006/xaml}'
TINT_OPACITY = 0.5  # NocturneWindow.TintOpacity's default
DEFAULT_ACCENT = 'blue'


def kebab(key):
    """Nocturne.Brushes.Surface1.Border -> surface1-border; Button.Primary.Hover -> button-primary-hover."""
    for prefix in ('Nocturne.Brushes.', 'Nocturne.Colors.', 'Nocturne.Font.', 'Nocturne.Fonts.'):
        if key.startswith(prefix):
            key = key[len(prefix):]
    key = re.sub(r'(?<=[a-z0-9])(?=[A-Z])', '-', key)
    return key.replace('.', '-').lower()


def argb(text):
    """#AARRGGBB or #RRGGBB -> (r, g, b, a) with a in 0..255."""
    h = text.strip().lstrip('#')
    if len(h) == 6:
        h = 'FF' + h
    if len(h) != 8:
        raise ValueError(text)
    return int(h[2:4], 16), int(h[4:6], 16), int(h[6:8], 16), int(h[0:2], 16)


def css(c):
    r, g, b, a = c
    return f'#{r:02X}{g:02X}{b:02X}' + ('' if a == 255 else f'{a:02X}')


def over(top, bottom, opacity=1.0):
    """top (with its alpha times opacity) over an opaque bottom."""
    a = top[3] / 255 * opacity
    return tuple(round(top[i] * a + bottom[i] * (1 - a)) for i in range(3)) + (255,)


def angle(start, end):
    """The CSS angle of a LinearGradientBrush from StartPoint to EndPoint (relative box)."""
    (x0, y0), (x1, y1) = start, end
    table = {(0, 1): '180deg', (1, 0): '90deg', (1, 1): 'to bottom right', (0, -1): '0deg', (-1, 0): '270deg'}
    key = (round(x1 - x0), round(y1 - y0))
    if key not in table:
        raise ValueError(f'gradient direction {start} -> {end}')
    return table[key]


def point(text, default):
    if not text:
        return default
    x, y = text.split(',')
    return float(x), float(y)


def gradient(stops, direction='180deg'):
    parts = ', '.join(f'{css(c)} {round(o * 100, 2):g}%' for o, c in stops)
    return f'linear-gradient({direction}, {parts})'


def read_brushes():
    tree = ET.parse(os.path.join(WPF, 'Styles', 'Brushes.xaml'))
    out = {}
    for el in tree.getroot():
        key = el.get(X + 'Key')
        if key is None:
            continue
        tag = el.tag.replace(NS, '')
        if tag == 'Color':
            out[key] = ('color', argb(el.text))
        elif tag == 'SolidColorBrush':
            out[key] = ('solid', argb(el.get('Color')))
        elif tag == 'LinearGradientBrush':
            stops = [(float(s.get('Offset')), argb(s.get('Color'))) for s in el.iter(NS + 'GradientStop')]
            direction = angle(point(el.get('StartPoint'), (0, 0)), point(el.get('EndPoint'), (1, 1)))
            out[key] = ('gradient', (stops, direction))
        else:
            raise ValueError(f'Brushes.xaml: {tag} {key}')
    return out


def read_fonts():
    tree = ET.parse(os.path.join(WPF, 'Styles', 'Fonts.xaml'))
    out = []
    for el in tree.getroot():
        key = el.get(X + 'Key')
        tag = el.tag.split('}')[-1]
        if tag == 'FontFamily':
            # An embedded font (/Nocturne.Wpf;component/Fonts/#Inter) is its family name on the web,
            # where nocturne.src.css declares it with @font-face
            families = [f.strip().split('#')[-1] for f in el.text.split(',')]
            generic = 'monospace' if key.endswith('Mono') else 'system-ui, sans-serif'
            value = ', '.join(f'"{f}"' if ' ' in f else f for f in families) + ', ' + generic
            out.append(('font-' + kebab(key), value))
        elif tag == 'Double':
            out.append(('font-' + kebab(key), f'{float(el.text):g}px'))
    return out


PALETTE = re.compile(r'\[NocturneAccent\.(\w+)\] = Palette\.FromHex\((.*?)\),\s*$', re.S | re.M)
ARG = re.compile(r'(\w+): (\("[^"]+", "[^"]+"\)|"[^"]+")')


def read_palettes():
    with open(os.path.join(WPF, 'Controls', 'NocturneTheme.cs'), encoding='utf-8') as f:
        text = f.read()
    out = {}
    for name, body in PALETTE.findall(text):
        values = {}
        for arg, value in ARG.findall(body):
            hexes = re.findall(r'#[0-9A-Fa-f]{6}', value)
            values[arg] = tuple(argb(h) for h in hexes) if len(hexes) > 1 else argb(hexes[0])
        out[name.lower()] = values
    if DEFAULT_ACCENT not in out:
        raise ValueError('NocturneTheme.cs: no blue palette')
    return out


def with_alpha(c, a):
    return c[:3] + (a,)


def accent_tokens(p):
    """What NocturneTheme.Apply sets, plus the solid ground and popup of the web version."""
    ground = over(p['tint'], p['fallback'], TINT_OPACITY)
    popup = over(with_alpha(p['popup'], 0xF7), ground)
    vertical = lambda top, bottom: gradient([(0, top), (1, bottom)])
    return [
        ('color-accent', css(p['accent'])),
        ('color-accent-light', css(p['light'])),
        ('accent', css(p['accent'])),
        ('accent-light', css(p['light'])),
        ('accent-text', css(p['text'])),
        ('accent-text-hover', css(p['textHover'])),
        ('text-on-accent', css(p['onAccent'])),
        ('button-primary', vertical(*p['primary'])),
        ('button-primary-hover', vertical(*p['primaryHover'])),
        ('button-primary-pressed', css(p['primaryPressed'])),
        ('window-fallback', css(p['fallback'])),
        ('window-tint', css(p['tint'])),
        ('window-ground', css(ground)),
        ('popup', css(popup)),
        ('title-bar-accent', vertical(with_alpha(p['accent'], 0x4D), with_alpha(p['accent'], 0x38))),
        ('title-bar-accent-border', css(with_alpha(p['light'], 0x38))),
    ]


# Line icons of Styles/Icons.xaml that the CSS draws as masks (check marks, chevrons, the × of a field),
# with the stroke thickness the WPF templates give them on the 24 x 24 grid
GLYPHS = {
    'Check': 3, 'ChevronDown': 1.8, 'ChevronUp': 1.8, 'ChevronRight': 1.8, 'ChevronLeft': 1.8,
    'Close': 2, 'Search': 2, 'Eye': 1.6, 'Calendar': 1.6, 'Clock': 1.6, 'Filter': 1.8, 'Plus': 2,
    'Error': 1.8, 'Menu': 1.6,
}


def read_glyphs():
    tree = ET.parse(os.path.join(WPF, 'Styles', 'Icons.xaml'))
    paths = {el.get(X + 'Key', '').replace('Nocturne.Icons.', ''): el.text.strip() for el in tree.getroot()}
    out = []
    for name, width in GLYPHS.items():
        svg = (f"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='black' "
               f"stroke-width='{width:g}' stroke-linecap='round' stroke-linejoin='round'><path d='{paths[name]}'/></svg>")
        data = svg.replace('<', '%3C').replace('>', '%3E').replace('#', '%23').replace('"', "'")
        out.append(('glyph-' + kebab(name), f'url("data:image/svg+xml,{data}")'))
    return out


def render():
    brushes = read_brushes()
    palettes = read_palettes()
    accent_names = {name for name, _ in accent_tokens(palettes[DEFAULT_ACCENT])}

    lines = ['/* Generated by tools/generate_web_tokens.py from src/Nocturne.Wpf (Styles/Brushes.xaml, Styles/Fonts.xaml,',
             '   Controls/NocturneTheme.cs); do not edit. */', '',
             '/* Neutral values (Brushes.xaml, Fonts.xaml) with the blue accent, the default */', '',
             ':root {']
    for name, value in read_fonts():
        lines.append(f'  --nc-{name}: {value};')
    for key, (kind, value) in brushes.items():
        name = kebab(key) if kind != 'color' else 'color-' + kebab(key)
        if name in accent_names:
            continue
        if kind == 'gradient':
            stops, direction = value
            text = gradient(stops, direction)
        else:
            text = css(value)
        lines.append(f'  --nc-{name}: {text};')
    for name, value in accent_tokens(palettes[DEFAULT_ACCENT]):
        lines.append(f'  --nc-{name}: {value};')
    lines.append('  /* Line icons (Styles/Icons.xaml) as masks: mask: var(--nc-glyph-check) center / 14px no-repeat */')
    for name, value in read_glyphs():
        lines.append(f'  --nc-{name}: {value};')
    lines.append('}')
    for accent, palette in palettes.items():
        if accent == DEFAULT_ACCENT:
            continue
        lines += ['', f'/* {accent.capitalize()} accent (NocturneTheme.SetAccent(NocturneAccent.{accent.capitalize()})) */',
                  f':root[data-accent="{accent}"] {{']
        lines += [f'  --nc-{name}: {value};' for name, value in accent_tokens(palette)]
        lines.append('}')
    return '\n'.join(lines) + '\n'


def main():
    text = render()
    if '--check' in sys.argv:
        if not os.path.exists(OUT) or open(OUT, encoding='utf-8').read() != text:
            print('nocturne.tokens.css is out of date; run python tools/generate_web_tokens.py')
            sys.exit(1)
        return
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    # CRLF, as Git checks text files out on Windows (core.autocrlf), so a run leaves no diff
    with open(OUT, 'w', encoding='utf-8', newline='\r\n') as f:
        f.write(text)
    print('wrote', os.path.relpath(OUT, ROOT))


if __name__ == '__main__':
    main()
