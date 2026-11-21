using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickLauncher
{
    // The exe's own icon, as large as it has: Explorer's 256 px "jumbo" image, falling back to
    // the 48 px one when the exe only has a small icon (jumbo would show it tiny in a corner).
    // Shell image lists are STA objects: call on the UI thread.
    static class ShellIcon
    {
        public static ImageSource For(string path)
        {
            if (StartMenu.IsShellTarget(path)) return ForShellItem(path, 96);
            try
            {
                var info = new SHFILEINFO();
                if (SHGetFileInfo(path, 0, ref info, (uint)Marshal.SizeOf(info), SHGFI_SYSICONINDEX) == IntPtr.Zero) return null;
                var jumbo = Get(SHIL_JUMBO, info.iIcon);
                if (jumbo != null && ContentWidth(jumbo) > 64) return jumbo;
                return Get(SHIL_EXTRALARGE, info.iIcon) ?? jumbo;
            }
            catch { return null; }
        }

        static BitmapSource Get(int list, int index)
        {
            var iid = IID_IImageList;
            if (SHGetImageList(list, ref iid, out IntPtr himl) != 0 || himl == IntPtr.Zero) return null;
            IntPtr hicon = ImageList_GetIcon(himl, index, ILD_TRANSPARENT);
            if (hicon == IntPtr.Zero) return null;
            try
            {
                var bmp = Imaging.CreateBitmapSourceFromHIcon(hicon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                bmp.Freeze();
                return bmp;
            }
            finally { DestroyIcon(hicon); }
        }

        // Width of the non-transparent part.
        static int ContentWidth(BitmapSource b)
        {
            var src = b.Format == PixelFormats.Bgra32 ? b : new FormatConvertedBitmap(b, PixelFormats.Bgra32, null, 0);
            int w = src.PixelWidth, h = src.PixelHeight, stride = w * 4;
            var px = new byte[stride * h];
            src.CopyPixels(px, stride, 0);
            int min = w, max = -1;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (px[y * stride + x * 4 + 3] > 16) { if (x < min) min = x; if (x > max) max = x; }
            return max < 0 ? 0 : max - min + 1;
        }

        // Store apps and other shell-only entries ("shell:AppsFolder\…") have no file to read an
        // icon from: ask the shell item for its image. The HBITMAP is a 32-bit premultiplied DIB
        // section; Imaging.CreateBitmapSourceFromHBitmap would drop its alpha (black corners),
        // so its pixels are copied by hand.
        static ImageSource ForShellItem(string parsingName, int size)
        {
            IntPtr hbm = IntPtr.Zero;
            try
            {
                SHCreateItemFromParsingName(parsingName, IntPtr.Zero, typeof(IShellItemImageFactory).GUID, out IShellItemImageFactory f);
                try
                {
                    if (f.GetImage(new SIZE { cx = size, cy = size }, SIIGBF_ICONONLY | SIIGBF_BIGGERSIZEOK, out hbm) != 0 || hbm == IntPtr.Zero)
                        return null;
                }
                finally { Marshal.ReleaseComObject(f); }

                var ds = new DIBSECTION();
                if (GetObject(hbm, Marshal.SizeOf(typeof(DIBSECTION)), ref ds) == 0 || ds.dsBm.bmBits == IntPtr.Zero || ds.dsBm.bmBitsPixel != 32)
                {
                    var plain = Imaging.CreateBitmapSourceFromHBitmap(hbm, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    plain.Freeze();
                    return plain;
                }
                int w = ds.dsBm.bmWidth, h = ds.dsBm.bmHeight, stride = ds.dsBm.bmWidthBytes;
                var px = new byte[stride * h];
                Marshal.Copy(ds.dsBm.bmBits, px, 0, px.Length);
                if (ds.dsBmih.biHeight > 0)   // bottom-up DIB: flip rows
                {
                    var flipped = new byte[px.Length];
                    for (int y = 0; y < h; y++) Buffer.BlockCopy(px, y * stride, flipped, (h - 1 - y) * stride, stride);
                    px = flipped;
                }
                var bmp = BitmapSource.Create(w, h, 96, 96, PixelFormats.Pbgra32, null, px, stride);
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
            finally { if (hbm != IntPtr.Zero) DeleteObject(hbm); }
        }

        [ComImport, Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        interface IShellItemImageFactory { [PreserveSig] int GetImage(SIZE size, int flags, out IntPtr phbm); }
        [StructLayout(LayoutKind.Sequential)] struct SIZE { public int cx, cy; }
        const int SIIGBF_BIGGERSIZEOK = 0x1, SIIGBF_ICONONLY = 0x4;

        [StructLayout(LayoutKind.Sequential)]
        struct BITMAP { public int bmType, bmWidth, bmHeight, bmWidthBytes; public ushort bmPlanes, bmBitsPixel; public IntPtr bmBits; }
        [StructLayout(LayoutKind.Sequential)]
        struct BITMAPINFOHEADER { public uint biSize; public int biWidth, biHeight; public ushort biPlanes, biBitCount; public uint biCompression, biSizeImage; public int biXPelsPerMeter, biYPelsPerMeter; public uint biClrUsed, biClrImportant; }
        [StructLayout(LayoutKind.Sequential)]
        struct DIBSECTION { public BITMAP dsBm; public BITMAPINFOHEADER dsBmih; public uint dsBitfields0, dsBitfields1, dsBitfields2; public IntPtr dshSection; public uint dsOffset; }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        static extern void SHCreateItemFromParsingName(string path, IntPtr pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory ppv);
        [DllImport("gdi32.dll")] static extern int GetObject(IntPtr h, int size, ref DIBSECTION ds);
        [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr h);

        const uint SHGFI_SYSICONINDEX = 0x4000;
        const int SHIL_EXTRALARGE = 2, SHIL_JUMBO = 4, ILD_TRANSPARENT = 1;
        static readonly Guid IID_IImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct SHFILEINFO
        {
            public IntPtr hIcon; public int iIcon; public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SHGetFileInfo(string path, uint attr, ref SHFILEINFO info, uint size, uint flags);
        [DllImport("shell32.dll", EntryPoint = "#727")]
        static extern int SHGetImageList(int list, ref Guid riid, out IntPtr ppv);
        [DllImport("comctl32.dll")]
        static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr h);
    }
}
