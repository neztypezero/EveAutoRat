using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EveAutoRat
{
  public class Win32
  {
    enum TernaryRasterOperations : uint
    {
      /// <summary>dest = source</summary>
      SRCCOPY = 0x00CC0020,
      /// <summary>dest = source OR dest</summary>
      SRCPAINT = 0x00EE0086,
      /// <summary>dest = source AND dest</summary>
      SRCAND = 0x008800C6,
      /// <summary>dest = source XOR dest</summary>
      SRCINVERT = 0x00660046,
      /// <summary>dest = source AND (NOT dest)</summary>
      SRCERASE = 0x00440328,
      /// <summary>dest = (NOT source)</summary>
      NOTSRCCOPY = 0x00330008,
      /// <summary>dest = (NOT src) AND (NOT dest)</summary>
      NOTSRCERASE = 0x001100A6,
      /// <summary>dest = (source AND pattern)</summary>
      MERGECOPY = 0x00C000CA,
      /// <summary>dest = (NOT source) OR dest</summary>
      MERGEPAINT = 0x00BB0226,
      /// <summary>dest = pattern</summary>
      PATCOPY = 0x00F00021,
      /// <summary>dest = DPSnoo</summary>
      PATPAINT = 0x00FB0A09,
      /// <summary>dest = pattern XOR dest</summary>
      PATINVERT = 0x005A0049,
      /// <summary>dest = (NOT dest)</summary>
      DSTINVERT = 0x00550009,
      /// <summary>dest = BLACK</summary>
      BLACKNESS = 0x00000042,
      /// <summary>dest = WHITE</summary>
      WHITENESS = 0x00FF0062,
      /// <summary>
      /// Capture window as seen on screen.  This includes layered windows 
      /// such as WPF windows with AllowsTransparency="true"
      /// </summary>
      CAPTUREBLT = 0x40000000
    }

    [DllImport("gdi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

    [DllImport("gdi32.dll")]
    static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
      public int Left;    // x position of upper-left corner
      public int Top;     // y position of upper-left corner
      public int Right;     // x position of lower-right corner
      public int Bottom;    // y position of lower-right corner
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetDpiForWindow(IntPtr hWnd);

    public static float GetScaleFactorForWindow(IntPtr hWnd) {
      float dpi = GetDpiForWindow(hWnd);
      return dpi / 96.0f;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    /// <summary> Get the text for the window pointed to by hWnd </summary>
    public static string GetWindowText(IntPtr hWnd)
    {
      int size = GetWindowTextLength(hWnd);
      if (size > 0)
      {
        var builder = new StringBuilder(size + 1);
        GetWindowText(hWnd, builder, builder.Capacity);
        return builder.ToString();
      }

      return String.Empty;
    }

    public static void FindWindow(string winTitle, out IntPtr hWnd, out RECT winRect)
    {
      IntPtr hWndOut = IntPtr.Zero;
      RECT winRectOut = new RECT();

      EnumWindows(delegate (IntPtr wnd, IntPtr param)
      {
        string windowText = GetWindowText(wnd);
        if (windowText == winTitle)
        {
          RECT r;
          if (GetWindowRect(wnd, out r))
          {
            hWndOut = wnd;
            winRectOut = r;
            return false;
          }
        }
        return true;
      }, IntPtr.Zero);

      hWnd = hWndOut;
      winRect = winRectOut;
    }

    public static List<IntPtr> FindWindowList(string winTitle)
    {
      IntPtr hWndOut = IntPtr.Zero;
      List<IntPtr> hWindList = new List<IntPtr>();

      EnumWindows(delegate (IntPtr wnd, IntPtr param)
      {
        string windowText = GetWindowText(wnd);
        if (windowText == winTitle)
        {
          hWindList.Add(wnd);
        }
        return true;
      }, IntPtr.Zero);

      return hWindList;
    }

    private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

    public static List<IntPtr> GetAllChildHandles(IntPtr parentHWnd)
    {
      List<IntPtr> childHandles = new List<IntPtr>();

      GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
      IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

      try
      {
        EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
        EnumChildWindows(parentHWnd, childProc, pointerChildHandlesList);
      }
      finally
      {
        gcChildhandlesList.Free();
      }

      return childHandles;
    }

    private static bool EnumWindow(IntPtr hWnd, IntPtr lParam)
    {
      GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

      if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
      {
        return false;
      }

      List<IntPtr> childHandles = gcChildhandlesList.Target as List<IntPtr>;
      childHandles.Add(hWnd);

      return true;
    }

    enum WParams : int
    {
      MK_CONTROL = 0x0008, // The CTRL key is down.
      MK_LBUTTON = 0x0001, // The left mouse button is down.
      MK_MBUTTON = 0x0010, // The middle mouse button is down.
      MK_RBUTTON = 0x0002, // The right mouse button is down.
      MK_SHIFT = 0x0004, // The SHIFT key is down.
      MK_XBUTTON1 = 0x0020, // The first X button is down.
      MK_XBUTTON2 = 0x0040 // The second X button is down.
    }
    enum WMessages : int
    {
      WM_MOUSEMOVE = 0x0200, // mouse move
      WM_LBUTTONDOWN = 0x201, //Left mousebutton down
      WM_LBUTTONUP = 0x202, //Left mousebutton up
      WM_RBUTTONDOWN = 0x204, //Right mousebutton down
      WM_RBUTTONUP = 0x205, //Right mousebutton up
    }

    public static int MakeLParam(int LoWord, int HiWord)
    {
      return ((HiWord << 16) | (LoWord & 0xffff));
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    public static void SendMouseDown(IntPtr hWnd, int x, int y)
    {
      int LParam = MakeLParam(x, y);
      SendMessage(hWnd, (int)WMessages.WM_LBUTTONDOWN, 0, LParam);
    }

    public static void SendMouseUp(IntPtr hWnd, int x, int y)
    {
      int LParam = MakeLParam(x, y);
      SendMessage(hWnd, (int)WMessages.WM_LBUTTONUP, 0, LParam);
    }

    public static void SendMouseClick(IntPtr hWnd, int x, int y)
    {
      int LParam = MakeLParam(x, y-52);
      SendMessage(hWnd, (int)WMessages.WM_MOUSEMOVE, 0, LParam);
      SendMessage(hWnd, (int)WMessages.WM_LBUTTONDOWN, (int)WParams.MK_LBUTTON, LParam);
      SendMessage(hWnd, (int)WMessages.WM_LBUTTONUP, (int)WParams.MK_LBUTTON, LParam);
      SendMessage(hWnd, (int)WMessages.WM_MOUSEMOVE, 0, LParam);
    }

    public static Bitmap GetScreenBitmap(IntPtr hWnd)
    {
      RECT rc;
      GetWindowRect(hWnd, out rc);

      int width = rc.Right - rc.Left;
      int height = rc.Bottom - rc.Top;

      if (width > 0 && height > 0)
      {
        float scaleFactor = GetScaleFactorForWindow(hWnd);
        width = (int)(width * scaleFactor);
        height = (int)(height * scaleFactor);
        return new Bitmap(width, height, PixelFormat.Format24bppRgb);
      }
      return null;
    }

    public static void CopyScreenBitmap(IntPtr hWnd, Bitmap bmp)
    {
      Graphics graphics = Graphics.FromImage(bmp);

      IntPtr hWndDC = GetDC(hWnd);
      IntPtr gDC = graphics.GetHdc();

      BitBlt(gDC, 0, 0, bmp.Width, bmp.Height, hWndDC, 0, 0, TernaryRasterOperations.SRCCOPY);

      ReleaseDC(hWnd, hWndDC);
      graphics.ReleaseHdc(gDC);
      graphics.Dispose();
    }

    public static Bitmap GetRegion(Bitmap srcBitmap, Rectangle srcRegion)
    {
      Bitmap destBitmap = new Bitmap(srcRegion.Width, srcRegion.Height);
      Rectangle destRegion = new Rectangle(0, 0, srcRegion.Width, srcRegion.Height);
      using (Graphics grD = Graphics.FromImage(destBitmap))
      {
        grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
      }
      return destBitmap;
    }

    /// <summary>
    /// Creates an Image object containing a screen shot of a specific window
    /// </summary>
    /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
    /// <returns></returns>
    public static Image CaptureWindow(IntPtr handle)
    {
      // get te hDC of the target window
      IntPtr hdcSrc = GetWindowDC(handle);
      // get the size
      RECT windowRect = new RECT();
      GetWindowRect(handle, out windowRect);
      int width = windowRect.Right - windowRect.Left;
      int height = windowRect.Bottom - windowRect.Top;
      // create a device context we can copy to
      IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
      // create a bitmap we can copy it to,
      // using GetDeviceCaps to get the width/height

      float scaleFactor = GetScaleFactorForWindow(handle);

      width = (int)((float)width * scaleFactor);
      height = (int)((float)height * scaleFactor);

      IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
      // select the bitmap object
      IntPtr hOld = SelectObject(hdcDest, hBitmap);
      // bitblt over
      BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, TernaryRasterOperations.SRCCOPY);
      // restore selection
      SelectObject(hdcDest, hOld);
      // clean up 
      DeleteDC(hdcDest);
      ReleaseDC(handle, hdcSrc);
      // get a .NET image object for it

      Image img = Image.FromHbitmap(hBitmap);
      // free up the Bitmap object
      DeleteObject(hBitmap);
      return img;
    }

    /// <summary>
    /// Captures a screen shot of a specific window, and saves it to a file
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="filename"></param>
    /// <param name="format"></param>
    public static void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
    {
      Image img = CaptureWindow(handle);
      img.Save(filename, format);
    }
  }
}
