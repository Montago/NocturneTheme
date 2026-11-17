# Draws src/Nocturne.Wpf/icon.png, the NuGet package icon: three four-pointed stars in one colour family
# on a dark rounded tile, each casting a long shadow towards the bottom right that fades out.
# Drawn from scratch (MIT).
#   powershell -ExecutionPolicy Bypass -File tools/generate_icon.ps1 [-Palette navy|green] [-Out path]

param(
    [ValidateSet('navy', 'green')] [string]$Palette = 'navy',
    [string]$Out = (Join-Path $PSScriptRoot '..\src\Nocturne.Wpf\icon.png')
)

Add-Type -AssemblyName System.Drawing

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }

$colours = @{
    navy  = @{ Top = '#14386B'; Bottom = '#0C2A55'; Shadow = '#061633'; Stars = @('#E4EFFC', '#C9DCF4', '#B3CBEC') }
    green = @{ Top = '#1E3A2B'; Bottom = '#11261A'; Shadow = '#07140D'; Stars = @('#E2F8EC', '#C2F0D6', '#9FE6BE') }
}[$Palette]

$size = 256
$scale = 2                    # drawn at 512 px, then scaled down for smooth edges
$s = $size * $scale

function New-RoundedRect([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = 2 * $r
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

# A four-pointed star with concave sides: arms rx wide and ry tall, waist w from the centre
function New-Star([float]$cx, [float]$cy, [float]$rx, [float]$ry, [float]$w) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $top = New-Object System.Drawing.PointF $cx, ($cy - $ry)
    $right = New-Object System.Drawing.PointF ($cx + $rx), $cy
    $bottom = New-Object System.Drawing.PointF $cx, ($cy + $ry)
    $left = New-Object System.Drawing.PointF ($cx - $rx), $cy
    $ne = New-Object System.Drawing.PointF ($cx + $w), ($cy - $w)
    $se = New-Object System.Drawing.PointF ($cx + $w), ($cy + $w)
    $sw = New-Object System.Drawing.PointF ($cx - $w), ($cy + $w)
    $nw = New-Object System.Drawing.PointF ($cx - $w), ($cy - $w)
    $p.AddBezier($top, $ne, $ne, $right)
    $p.AddBezier($right, $se, $se, $bottom)
    $p.AddBezier($bottom, $sw, $sw, $left)
    $p.AddBezier($left, $nw, $nw, $top)
    $p.CloseFigure()
    return $p
}

# Stars in 256 px units: centre x, y, half width, half height
$stars = @(
    @{ X = 120; Y = 100; Rx = 60; Ry = 66 },
    @{ X = 154; Y = 176; Rx = 36; Ry = 40 },
    @{ X = 74;  Y = 152; Rx = 22; Ry = 25 }
)

$tile = New-RoundedRect (8 * $scale) (8 * $scale) (240 * $scale) (240 * $scale) (52 * $scale)

# Shadow layer: every star swept towards the bottom right, as one solid shape
$shadowBmp = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sg = [System.Drawing.Graphics]::FromImage($shadowBmp)
$sg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$sg.SetClip($tile)
$shadowBrush = New-Object System.Drawing.SolidBrush (C $colours.Shadow)
foreach ($st in $stars) {
    for ($i = 0; $i -le 300 * $scale; $i += 2) {
        $sg.FillPath($shadowBrush, (New-Star ($st.X * $scale + $i) ($st.Y * $scale + $i) ($st.Rx * $scale) ($st.Ry * $scale) (4.5 * $scale)))
    }
}
$sg.Dispose()

# Fade the shadow along the diagonal: dark next to the stars, gone by the bottom-right corner
$rect = New-Object System.Drawing.Rectangle 0, 0, $s, $s
$data = $shadowBmp.LockBits($rect, [System.Drawing.Imaging.ImageLockMode]::ReadWrite, $shadowBmp.PixelFormat)
$bytes = New-Object byte[] ($data.Stride * $s)
[System.Runtime.InteropServices.Marshal]::Copy($data.Scan0, $bytes, 0, $bytes.Length)
for ($y = 0; $y -lt $s; $y++) {
    $row = $y * $data.Stride
    for ($x = 0; $x -lt $s; $x++) {
        $k = $row + 4 * $x + 3
        if ($bytes[$k] -eq 0) { continue }
        $t = (($x + $y) / $scale - 230) / 250
        if ($t -lt 0) { $t = 0 } elseif ($t -gt 1) { $t = 1 }
        $bytes[$k] = [byte]($bytes[$k] * 0.9 * (1 - $t))
    }
}
[System.Runtime.InteropServices.Marshal]::Copy($bytes, 0, $data.Scan0, $bytes.Length)
$shadowBmp.UnlockBits($data)

# Compose: ground, shadow, stars
$big = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($big)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)
$ground = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 0, 0), (New-Object System.Drawing.PointF 0, $s), (C $colours.Top), (C $colours.Bottom)
$g.FillPath($ground, $tile)
$g.DrawImage($shadowBmp, 0, 0)
for ($n = 0; $n -lt $stars.Count; $n++) {
    $st = $stars[$n]
    $brush = New-Object System.Drawing.SolidBrush (C $colours.Stars[$n])
    $g.FillPath($brush, (New-Star ($st.X * $scale) ($st.Y * $scale) ($st.Rx * $scale) ($st.Ry * $scale) (4.5 * $scale)))
}
$g.Dispose()

$bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.DrawImage($big, 0, 0, $size, $size)
$g.Dispose()

$bmp.Save([System.IO.Path]::GetFullPath($Out), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose(); $big.Dispose(); $shadowBmp.Dispose()
Write-Output "Wrote $Out"
