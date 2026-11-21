using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace QuickLauncher
{
    // Tile icons, loaded off the UI thread. Reading ~270 shell icons took ~2 s, and on the UI
    // thread that froze the panel at start and on every tab switch.
    //
    // Each icon is also shrunk ONCE to the exact pixel size the tile shows (64 DIP × screen
    // scale). Kept at 256 px, every repaint (scroll, hover) re-scaled 270 big bitmaps with
    // high-quality filtering.
    //
    // Shell icon APIs need an STA thread, so this is one STA worker with a queue.
    sealed class IconLoader
    {
        readonly Dispatcher ui;
        readonly int px;
        readonly BlockingCollection<string> queue = new BlockingCollection<string>();
        readonly Dictionary<string, ImageSource> cache = new Dictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<string, List<Action<ImageSource>>> waiting = new Dictionary<string, List<Action<ImageSource>>>(StringComparer.OrdinalIgnoreCase);

        public IconLoader(Dispatcher ui, double dipSize)
        {
            this.ui = ui;
            px = (int)Math.Round(dipSize * ScreenScale());
            var t = new Thread(Work) { IsBackground = true, Name = "Icons" };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        // UI thread only. done runs on the UI thread, at once if the icon is cached.
        public void Get(string path, Action<ImageSource> done)
        {
            if (cache.TryGetValue(path, out var img)) { done(img); return; }
            if (waiting.TryGetValue(path, out var list)) { list.Add(done); return; }
            waiting[path] = new List<Action<ImageSource>> { done };
            queue.Add(path);
        }

        void Work()
        {
            foreach (string path in queue.GetConsumingEnumerable())
            {
                ImageSource img = null;
                try { img = Shrink(ShellIcon.For(path) as BitmapSource, px); } catch { }
                ui.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    cache[path] = img;
                    if (waiting.TryGetValue(path, out var list)) { waiting.Remove(path); foreach (var d in list) d(img); }
                }));
            }
        }

        // High-quality downscale to size × size, done once; a smaller icon is kept as it is.
        static BitmapSource Shrink(BitmapSource src, int size)
        {
            if (src == null || (src.PixelWidth <= size && src.PixelHeight <= size)) return src;
            var dv = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.HighQuality);
            using (var dc = dv.RenderOpen()) dc.DrawImage(src, new Rect(0, 0, size, size));
            var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            rtb.Freeze();
            return rtb;
        }

        // WPF apps are system-DPI aware: the primary screen's scale (1.0 at 100 %, 1.5 at 150 %).
        static double ScreenScale()
        {
            try { using (var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) return g.DpiX / 96.0; }
            catch { return 1.0; }
        }
    }
}
