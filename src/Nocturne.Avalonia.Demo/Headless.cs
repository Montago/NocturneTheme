using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Nocturne.Avalonia.Demo;

/// <summary>
/// The command-line commands that draw pictures instead of showing the demo: <c>--snapshot</c>
/// (<see cref="Snapshot"/>), <c>--render</c> and <c>--render-gallery</c> (<see cref="RenderCommand"/>). They run on Avalonia's
/// headless platform with Skia, so they need no screen and work the same on Windows, macOS and Linux.
/// Popups are drawn into the window (OverlayPopups), so they show in the pictures.
/// </summary>
internal static class Headless
{
    public static bool TryRun(string[] args, out int exitCode)
    {
        exitCode = 0;
        if (args.Length == 0 || args[0] is not ("--snapshot" or "--render" or "--render-gallery"))
            return false;

        AppBuilder.Configure<App>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false, OverlayPopups = true })
            .SetupWithoutStarting();
        // The headless platform cannot extend the client area into the frame; draw the title bar anyway.
        Controls.NocturneWindow.AlwaysDrawTitleBar = true;
        AnimationClock.Take();

        exitCode = args[0] switch
        {
            "--snapshot" => Snapshot.Run(args),
            "--render-gallery" => RenderCommand.RunGallery(args),
            _ => RenderCommand.Run(args),
        };
        return true;
    }

    /// <summary>
    /// Lets layout, bindings and the renderer settle, then takes the window's picture. Each frame
    /// moves the clock on by 1/60 s; the default (half a second) lets the theme's 120/150 ms transitions finish,
    /// also those that start a few frames late (a popup that opens).
    /// </summary>
    public static WriteableBitmap Capture(Window window, int frames = 30)
    {
        for (var i = 0; i < frames; i++)
        {
            AnimationClock.Frame(TimeSpan.FromSeconds(1.0 / 60));
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            AnimationClock.Settle();
        }
        return window.CaptureRenderedFrame()
               ?? throw new InvalidOperationException("The headless renderer returned no frame.");
    }

    /// <summary>Runs the dispatcher for a while of real time, so its timers fire, and moves the animation clock on as much.</summary>
    public static void Wait(TimeSpan time)
    {
        using (var stop = new CancellationTokenSource(time))
            Dispatcher.UIThread.MainLoop(stop.Token);
        AnimationClock.Frame(time);
        Dispatcher.UIThread.RunJobs();
    }

    /// <summary>
    /// Animation time for the headless commands. It moves only in <see cref="Capture"/> (1/60 s a frame) and
    /// <see cref="Wait"/>, so transitions and endless animations (ProgressRing, the indeterminate ProgressBar) stand
    /// at the same moment in every run, as in WPF's pictures. Avalonia 12 keeps its animation clock internal, so this
    /// reaches into <c>MediaContext</c>:
    /// <list type="bullet">
    /// <item><c>_time</c>, the stopwatch the clock reads, is swapped for a stopped one that is moved by hand;</item>
    /// <item><c>_animationsTimer</c>, a DispatcherTimer that asks for the next animation frame, is fired by hand
    /// (<c>RunJobs</c> never fires a timer);</item>
    /// <item>a committed frame is reported done from the thread pool, and while one is outstanding no animation frame
    /// is asked for, so <see cref="Settle"/> waits for it.</item>
    /// </list>
    /// Without this, animations stop after a frame or two. If a later Avalonia renames these fields, a note says so
    /// and the pictures show whatever moment comes.
    /// </summary>
    private static class AnimationClock
    {
        private const System.Reflection.BindingFlags Flags =
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static;

        private static readonly System.Diagnostics.Stopwatch Time = new();
        private static System.Reflection.FieldInfo? elapsed;
        private static DispatcherTimer? animationsTimer;
        private static System.Reflection.MethodInfo? fireTick;
        private static System.Collections.IDictionary? pendingBatches;

        public static void Take()
        {
            var type = typeof(AvaloniaObject).Assembly.GetType("Avalonia.Media.MediaContext");
            var context = type?.GetProperty("Instance", Flags)?.GetValue(null);
            var time = type?.GetField("_time", Flags);
            elapsed = typeof(System.Diagnostics.Stopwatch).GetField("_elapsed", Flags);
            animationsTimer = type?.GetField("_animationsTimer", Flags)?.GetValue(context) as DispatcherTimer;
            fireTick = typeof(DispatcherTimer).GetMethod("FireTick", Flags, Type.EmptyTypes);
            pendingBatches = type?.GetField("_pendingCompositionBatches", Flags)?.GetValue(context) as System.Collections.IDictionary;
            if (context == null || time?.GetValue(context) is not System.Diagnostics.Stopwatch current ||
                elapsed?.FieldType != typeof(long) || animationsTimer == null || fireTick == null || pendingBatches == null)
            {
                elapsed = null;
                pendingBatches = null;
                Console.Error.WriteLine("Note: Avalonia's animation clock could not be taken over; animations show whatever moment comes.");
                return;
            }
            // Carry on from the time so far, so nothing sees time run backwards
            Set(current.Elapsed);
            time.SetValue(context, Time);
        }

        public static void Frame(TimeSpan step)
        {
            if (elapsed == null)
                return;
            Set(Time.Elapsed + step);
            if (animationsTimer!.IsEnabled)
                fireTick!.Invoke(animationsTimer, null);
        }

        /// <summary>
        /// Waits (at most half a second) until the renderer has reported the frames it was given as done. A report
        /// may lead to a new frame at once, which the next tick draws: only the frames outstanding now are waited for.
        /// </summary>
        public static void Settle()
        {
            if (pendingBatches == null)
                return;
            var outstanding = pendingBatches.Values.Cast<object>().ToList();
            var until = Environment.TickCount64 + 500;
            while (outstanding.Count > 0 && Environment.TickCount64 < until)
            {
                Thread.Yield();
                Dispatcher.UIThread.RunJobs();
                var pending = pendingBatches.Values.Cast<object>().ToList();
                outstanding.RemoveAll(batch => !pending.Contains(batch));
            }
        }

        // A stopped Stopwatch reports _elapsed, counted in Stopwatch.Frequency ticks
        private static void Set(TimeSpan value) =>
            elapsed!.SetValue(Time, (long)(value.TotalSeconds * System.Diagnostics.Stopwatch.Frequency));
    }

    /// <summary>The value after <paramref name="name"/> in <paramref name="args"/>, if any.</summary>
    public static string? Option(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
