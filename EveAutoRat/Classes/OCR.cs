using System.Drawing;
using Tesseract;

namespace EveAutoRat.Classes
{
  class OCR
  {
    private static TesseractEngine textEngine = null;
    private static TesseractEngine numberEngine = null;
    private static TesseractEngine numberGEngine = null;

    public static string GetText(Bitmap image, Rectangle region)
    {
      if (textEngine == null)
      {
        textEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        textEngine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.%()': ");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = textEngine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText().Trim();
        }
      }
      return "";
    }

    public static string GetNumber(Bitmap image, Rectangle region)
    {
      if (numberEngine == null)
      {
        numberEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        numberEngine.SetVariable("tessedit_char_whitelist", "0123456789-.");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = numberEngine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText().Trim();
        }
      }
      return "";
    }

    public static string GetNumberG(Bitmap image, Rectangle region)
    {
      if (numberEngine == null)
      {
        numberGEngine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
        numberGEngine.SetVariable("tessedit_char_whitelist", "gG0123456789-.");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = numberGEngine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText().Trim().Replace('g', '9').Replace("G", "");
        }
      }
      return "";
    }
  }
}
