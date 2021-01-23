using System.Drawing;
using Tesseract;

namespace EveAutoRat.Classes
{
  class OCR
  {
    private static TesseractEngine engine = null;

    public static string GetText(Bitmap image, Rectangle region)
    {
      if (engine == null)
      {
        engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        engine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.%()'");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = engine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText();
        }
      }
      return "";
    }

    public static string GetNumber(Bitmap image, Rectangle region)
    {
      if (engine == null)
      {
        engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        engine.SetVariable("tessedit_char_whitelist", "g0123456789-.");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = engine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText();
        }
      }
      return "";
    }
  }
}
