using AForge.Imaging.Filters;
using System.Drawing;
using Tesseract;

namespace EveAutoRat.Classes
{
  class OCR
  {
    private static TesseractEngine textEngine = null;
    private static TesseractEngine numberEngine = null;
    private static TesseractEngine numberGEngine = null;
    private static Invert invertFilter = new Invert();

    private TesseractEngine ocrTextEngine = null;
    private TesseractEngine ocrNumericEngine = null;

    public static string GetText(Bitmap image, Rectangle region)
    {
      if (textEngine == null)
      {
        textEngine = new TesseractEngine(@"./tessdata", "shentox", EngineMode.Default);
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

    public static string GetTextInvert(Bitmap image, Rectangle region)
    {
      if (textEngine == null)
      {
        textEngine = new TesseractEngine(@"./tessdata", "shentox", EngineMode.Default);
        textEngine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.%()': ");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        using (Bitmap invBmp = image.Clone(region, image.PixelFormat))
        {
          invertFilter.ApplyInPlace(invBmp);
          Rect r = new Rect(0, 0, region.Width, region.Height);
          using (Page page = textEngine.Process(invBmp, r, PageSegMode.SingleLine))
          {
            return page.GetText().Trim();
          }
        }
      }
      return "";
    }

    public static string GetNumber(Bitmap image, Rectangle region)
    {
      if (numberEngine == null)
      {
        numberEngine = new TesseractEngine(@"./tessdata", "shentox", EngineMode.Default);
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
        numberGEngine = new TesseractEngine(@"./tessdata", "shentox", EngineMode.Default);
        numberGEngine.SetVariable("tessedit_char_whitelist", "lbBDgG0123456789-.");
      }
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = numberGEngine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText().Trim().Replace('g', '9').Replace("G", "").Replace("l", "1").Replace("B", "0").Replace("b", "0");
        }
      }
      return "";
    }

    public OCR()
    {
      ocrTextEngine = new TesseractEngine(@"./tessdata", "shentox", EngineMode.Default);
      ocrTextEngine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-.%()': ");

      ocrNumericEngine = new TesseractEngine(@"./tessdata", "shentox", EngineMode.Default);
      ocrNumericEngine.SetVariable("tessedit_char_whitelist", "0123456789-.%");
    }

    public string GetString(Bitmap image, Rectangle region)
    {
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = ocrTextEngine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText().Trim();
        }
      }
      return "";
    }

    public string GetNumeric(Bitmap image, Rectangle region)
    {
      if (region.X > -1 && region.Y > -1 && (region.Width + region.X) <= image.Width && (region.Height + region.Y) <= image.Height)
      {
        Rect r = new Rect(region.X, region.Y, region.Width, region.Height);
        using (Page page = ocrNumericEngine.Process(image, r, PageSegMode.SingleLine))
        {
          return page.GetText().Trim();
        }
      }
      return "";
    }
  }
}
