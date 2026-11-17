# Draws src/Nocturne.Wpf/icon.png, the NuGet package icon: a crescent moon in the accent greens on the
# theme's dark ground, with the soft light from the top-left corner. Drawn from scratch (MIT).
#   powershell -ExecutionPolicy Bypass -File tools/generate_icon.ps1

Add-Type -AssemblyName System.Drawing

$size = 256
$out = Join-Path $PSScriptRoot '..\src\Nocturne.Wpf\icon.png'

$bmp = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

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

# Ground: Window.Fallback (#1E2E25) towards Window.Tint (#101E16)
$tile = New-RoundedRect 8 8 240 240 52
$ground = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 0, 0), (New-Object System.Drawing.PointF $size, $size), ([System.Drawing.Color]::FromArgb(255, 0x1E, 0x2E, 0x25)), ([System.Drawing.Color]::FromArgb(255, 0x10, 0x1E, 0x16))
$g.FillPath($ground, $tile)

# The light from the top-left corner
# (a gradient brush repeats past its end points, so it spans the whole tile and fades out at 55 %)
$light = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 0, 0), (New-Object System.Drawing.PointF $size, $size), ([System.Drawing.Color]::FromArgb(36, 255, 255, 255)), ([System.Drawing.Color]::FromArgb(0, 255, 255, 255))
$blend = New-Object System.Drawing.Drawing2D.ColorBlend 3
$blend.Colors = @([System.Drawing.Color]::FromArgb(36, 255, 255, 255), [System.Drawing.Color]::FromArgb(0, 255, 255, 255), [System.Drawing.Color]::FromArgb(0, 255, 255, 255))
$blend.Positions = @([float]0, [float]0.55, [float]1)
$light.InterpolationColors = $blend
$g.FillPath($light, $tile)

# Edge like the cards: white at 11 %
$edge = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(28, 255, 255, 255)), 2
$g.DrawPath($edge, $tile)

# Crescent: a disc minus a disc moved up and to the right
$moon = New-Object System.Drawing.Drawing2D.GraphicsPath
$moon.AddEllipse(58, 58, 140, 140)
$cut = New-Object System.Drawing.Drawing2D.GraphicsPath
$cut.AddEllipse(104, 34, 128, 128)
$region = New-Object System.Drawing.Region $moon
$region.Exclude($cut)
$moonBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush (New-Object System.Drawing.PointF 58, 58), (New-Object System.Drawing.PointF 198, 198), ([System.Drawing.Color]::FromArgb(255, 0x3F, 0xD2, 0x7F)), ([System.Drawing.Color]::FromArgb(255, 0x1F, 0x9D, 0x57))
$g.FillRegion($moonBrush, $region)

$g.Dispose()
$bmp.Save([System.IO.Path]::GetFullPath($out), [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output "Wrote $out"
