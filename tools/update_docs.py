"""Refreshes the generated parts of the documentation.

Usage (from the repository root):

    python tools/update_docs.py              build, render and copy everything
    python tools/update_docs.py --check      only the checks, no build or screenshots

Steps:
  1. Builds the demo and the sample apps into a scratch folder (never into bin/, which Visual Studio
     may have locked).
  2. Runs each with --snapshot and checks that no picture is blank. RenderTargetBitmap returns
     transparent images while the screen is off or the session is locked.
  3. Copies the page pictures to docs/screenshots, the gallery to docs/screenshots/controls and the
     sample apps' pages to docs/screenshots/samples.
  4. Regenerates docs/resource-keys.md.
  5. Checks that every {StaticResource} in Styles/ is defined earlier in the same file, that every
     image the docs reference exists, that every gallery and sample picture is shown on a docs page,
     that every gallery picture still has its sample, and that every public control class is
     mentioned on a docs page.

Prints a report and exits with 1 when a step or a check fails. Needs only the Python standard library
and the .NET SDK.
"""
import argparse
import filecmp
import glob
import os
import re
import shutil
import struct
import subprocess
import sys
import tempfile
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DOCS = os.path.join(ROOT, 'docs')
SHOTS = os.path.join(DOCS, 'screenshots')
DEMO = 'src/Nocturne.Wpf.Demo'
EXE = 'Nocturne.Wpf.Demo'
# (project, exe, snapshot subfolder): the demo renders into the snapshot folder itself, the sample
# apps into samples/ (admin-*.png and portal-*.png)
APPS = [
    (DEMO, EXE, ''),
    ('samples/Nocturne.Wpf.Samples.Admin', 'Nocturne.Wpf.Samples.Admin', 'samples'),
    ('samples/Nocturne.Wpf.Samples.Portal', 'Nocturne.Wpf.Samples.Portal', 'samples'),
]
GALLERY = os.path.join(ROOT, 'src', 'Nocturne.Wpf.Demo', 'Views', 'GalleryPage.xaml')

# Demo pictures the docs use (the rest of --snapshot is for checking the look while working).
# accent-system.png is left out: it shows whatever accent the machine's Windows uses.
DEMO_PAGES = [
    'controls.png', 'more.png', 'custom.png', 'grid.png', 'tokens.png', 'dialog.png', 'flyout.png',
    'accent-green.png', 'accent-orange.png', 'accent-yellow.png', 'accent-red.png',
]


class Report:
    def __init__(self):
        self.errors = []
        self.warnings = []
        self.added = []
        self.changed = []

    def error(self, text):
        self.errors.append(text)
        print('  ERROR ' + text)

    def warn(self, text):
        self.warnings.append(text)
        print('  warning ' + text)


def step(text):
    print('\n== ' + text)


# ---------------------------------------------------------------- build and render

def build_and_render(scratch, report):
    snap = os.path.join(scratch, 'snap')
    shutil.rmtree(snap, ignore_errors=True)
    for project, exe, subfolder in APPS:
        out = os.path.join(scratch, 'bin', exe)
        step(f'Building {project}')
        result = subprocess.run(['dotnet', 'build', os.path.join(ROOT, project), '-c', 'Release', '-o', out, '-nologo', '-v', 'q'],
                                capture_output=True, text=True)
        if result.returncode != 0:
            report.error(f'build of {project} failed:\n{result.stdout[-3000:]}{result.stderr[-2000:]}')
            return None

        step(f'Rendering {exe} --snapshot')
        folder = os.path.join(snap, subfolder)
        try:
            subprocess.run([os.path.join(out, exe + '.exe'), '--snapshot', folder], timeout=600, check=False)
        except subprocess.TimeoutExpired:
            report.error(f'{exe} --snapshot did not finish within 10 minutes')
            return None
        error_file = os.path.join(folder, 'error.txt')
        if os.path.exists(error_file):
            report.error(f'{exe} --snapshot failed:\n' + open(error_file, encoding='utf-8', errors='replace').read())
            return None

    pictures = glob.glob(os.path.join(snap, '**', '*.png'), recursive=True)
    print(f'  {len(pictures)} pictures')
    blank = [p for p in pictures if is_blank(p)]
    if blank:
        report.error(f'{len(blank)} of {len(pictures)} pictures are blank. The screen is probably off or the session '
                     'locked; turn the screen on (or connect with RDP) and run again.')
        return None
    wheel = os.path.join(snap, 'wheel-check.txt')
    if os.path.exists(wheel) and not open(wheel, encoding='utf-8').read().startswith('PASS'):
        report.error('wheel-check.txt: ' + open(wheel, encoding='utf-8').read())
    return snap


def is_blank(path):
    """True when every pixel of an RGBA PNG is fully transparent (filtered bytes of a blank image are all 0)."""
    with open(path, 'rb') as f:
        data = f.read()
    pos, idat, height = 8, b'', 0
    while pos < len(data):
        length, kind = struct.unpack('>I4s', data[pos:pos + 8])
        chunk = data[pos + 8:pos + 8 + length]
        if kind == b'IHDR':
            height = struct.unpack('>II', chunk[:8])[1]
        elif kind == b'IDAT':
            idat += chunk
        pos += 12 + length
    raw = zlib.decompress(idat)
    stride = len(raw) // height if height else 0
    return all(not any(raw[row * stride + 1:(row + 1) * stride]) for row in range(height))


def copy_pictures(snap, report):
    step('Copying pictures to docs/screenshots')
    plan = [(os.path.join(snap, name), os.path.join(SHOTS, name)) for name in DEMO_PAGES]
    for folder in ('controls', 'samples'):
        plan += [(source, os.path.join(SHOTS, folder, os.path.basename(source)))
                 for source in glob.glob(os.path.join(snap, folder, '*.png'))]

    for source, target in plan:
        if not os.path.exists(source):
            report.error(f'expected picture {os.path.basename(source)} was not rendered')
            continue
        os.makedirs(os.path.dirname(target), exist_ok=True)
        name = os.path.relpath(target, DOCS).replace('\\', '/')
        if not os.path.exists(target):
            report.added.append(name)
        elif not filecmp.cmp(source, target, shallow=False):
            report.changed.append(name)
        else:
            continue
        shutil.copyfile(source, target)
    print(f'  {len(report.added)} new, {len(report.changed)} changed, {len(plan) - len(report.added) - len(report.changed)} unchanged')


# ---------------------------------------------------------------- checks

def regenerate_resource_keys(report):
    step('Regenerating docs/resource-keys.md')
    result = subprocess.run([sys.executable, os.path.join(ROOT, 'tools', 'generate_resource_keys.py')], capture_output=True, text=True)
    if result.returncode != 0:
        report.error('generate_resource_keys.py failed:\n' + result.stdout + result.stderr)


def check_static_resources(report):
    """StaticResource and BasedOn across the Controls.*.xaml files fail at runtime (each is loaded on its own)."""
    step('Checking {StaticResource} order in Styles/')
    key_pattern = re.compile(r'x:Key="([^"{]+)"')
    use_pattern = re.compile(r'\{StaticResource\s+([^}\s]+)\}')
    count = 0
    for path in sorted(glob.glob(os.path.join(ROOT, 'src', 'Nocturne.Wpf', 'Styles', '*.xaml'))):
        text = open(path, encoding='utf-8').read()
        # Blank out comments (usage examples), keeping the line numbers
        text = re.sub(r'<!--.*?-->', lambda m: re.sub(r'[^\n]', ' ', m.group(0)), text, flags=re.S)
        defined = {}
        for m in key_pattern.finditer(text):
            defined.setdefault(m.group(1), m.start())
        for m in use_pattern.finditer(text):
            key = m.group(1)
            if key.startswith('{') or key.startswith('x:'):
                continue
            count += 1
            line = text.count('\n', 0, m.start()) + 1
            where = f'{os.path.relpath(path, ROOT)}:{line}'
            if key not in defined:
                report.error(f'{where}: StaticResource {key} is not defined in this file (use DynamicResource)')
            elif defined[key] > m.start():
                report.error(f'{where}: StaticResource {key} is used before it is defined')
    print(f'  {count} references checked')


def doc_pages():
    return sorted(glob.glob(os.path.join(DOCS, '*.md')))


def check_doc_images(report):
    step('Checking images in docs/')
    referenced = set()
    for page in doc_pages():
        name = os.path.basename(page)
        for m in re.finditer(r'!\[[^\]]*\]\((screenshots/[^)\s]+)\)', open(page, encoding='utf-8').read()):
            referenced.add(m.group(1))
            if not os.path.exists(os.path.join(DOCS, m.group(1))):
                report.error(f'{name} shows {m.group(1)}, which does not exist')
    for root_md in ('README.md', os.path.join('src', 'Nocturne.Wpf', 'PACKAGE.md')):
        path = os.path.join(ROOT, root_md)
        if os.path.exists(path):
            text = open(path, encoding='utf-8').read()
            for m in re.finditer(r'docs/(screenshots/[\w/.-]+\.png)', text):
                referenced.add(m.group(1))
                if not os.path.exists(os.path.join(DOCS, m.group(1))):
                    report.error(f'{root_md} shows docs/{m.group(1)}, which does not exist')

    for picture in sorted(glob.glob(os.path.join(SHOTS, 'controls', '*.png')) + glob.glob(os.path.join(SHOTS, 'samples', '*.png'))):
        name = os.path.relpath(picture, DOCS).replace(os.sep, '/')
        if name not in referenced:
            report.warn(f'{name} is not shown on any docs page')

    # Every gallery picture still has its sample
    tags = set(re.findall(r'Style="\{StaticResource Demo\.Sample\}" Tag="([^"]+)"', open(GALLERY, encoding='utf-8').read()))
    for picture in glob.glob(os.path.join(SHOTS, 'controls', '*.png')):
        tag = os.path.splitext(os.path.basename(picture))[0]
        if tag not in tags:
            report.warn(f'screenshots/controls/{tag}.png has no sample in GalleryPage.xaml any more')
    print(f'  {len(referenced)} references, {len(tags)} gallery samples')


def check_controls_documented(report):
    """Every public control class should be mentioned on a docs page."""
    step('Checking that every control is documented')
    pages = '\n'.join(open(p, encoding='utf-8').read() for p in doc_pages())
    classes = []
    for path in glob.glob(os.path.join(ROOT, 'src', 'Nocturne.Wpf', 'Controls', '*.cs')):
        classes += re.findall(r'^public (?:sealed |abstract |partial )*class (\w+)\s*:\s*(?:\w+)', open(path, encoding='utf-8').read(), re.M)
    # Building blocks of templates and NocturneGrid that apps do not use directly
    internal = {'AccordionPanel', 'GridCellText', 'GridCheckBox', 'GridColumnHeadersPresenter', 'TabPanelBorder',
                # data classes of NocturneGrid's filters, groups and events
                'GridDateFilterNode', 'GridFilterOption', 'GridFilteredEventArgs', 'GridGroupRow'}
    for name in sorted(set(classes) - internal):
        if f'`{name}`' not in pages and f'nc:{name}' not in pages:
            report.warn(f'control {name} is not mentioned in docs/')
    print(f'  {len(set(classes) - internal)} controls')


# ---------------------------------------------------------------- main

def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument('--check', action='store_true', help='only run the checks; no build, no screenshots')
    parser.add_argument('--scratch', default=os.path.join(tempfile.gettempdir(), 'nocturne-docs'),
                        help='folder for the build and the raw snapshots (default: %(default)s)')
    args = parser.parse_args()
    report = Report()

    if not args.check:
        snap = build_and_render(args.scratch, report)
        if snap:
            copy_pictures(snap, report)
    regenerate_resource_keys(report)
    check_static_resources(report)
    check_doc_images(report)
    check_controls_documented(report)

    step('Summary')
    for title, items in (('New pictures', report.added), ('Changed pictures', report.changed)):
        if items:
            print(f'{title}:')
            for item in items:
                print('  docs/' + item)
    print(f'{len(report.errors)} error(s), {len(report.warnings)} warning(s)')
    if not args.check:
        print(f'Raw snapshots: {os.path.join(args.scratch, "snap")}')
    return 1 if report.errors else 0


if __name__ == '__main__':
    sys.exit(main())
