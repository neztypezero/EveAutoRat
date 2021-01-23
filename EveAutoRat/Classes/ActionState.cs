using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public class ActionState
  {
    protected ActionThread parent;
    protected ActionState nextState = null;
    protected double delay;
    protected double nextDelay;
    protected Rectangle NullRect = new Rectangle(-1, -1, 0, 0);

    protected AForge.Imaging.BlobCounter objectCounter = new AForge.Imaging.BlobCounter();
    protected Dictionary<int, Bitmap> threshHoldDictionary;
    protected System.IntPtr eventHWnd;

    protected Rectangle confirmBounds = new Rectangle(1672, 856, 130, 35);
    protected Rectangle eyeBounds = new Rectangle(1440, 635, 480, 45);
    protected Rectangle filterTitleBounds = new Rectangle(1625, 75, 150, 35);
    protected Rectangle battleLockOnBounds = new Rectangle(1170, 707, 60, 60);
    protected Rectangle battleIconBounds = new Rectangle(1545, 135, 39, 561);
    protected Rectangle firstBattleIconBounds = new Rectangle(1545, 135, 39, 75);
    protected Rectangle lootAllBounds = new Rectangle(700, 950, 380, 70);
    protected Rectangle firstItemDistanceBounds = new Rectangle(1586, 164, 85, 26);
    protected Rectangle popupBounds = new Rectangle(1300, 150, 200, 950);
    protected Rectangle throttleBounds = new Rectangle(684, 986, 37, 31);
    protected Rectangle targetAllBounds = new Rectangle(1167, 707, 69, 62);
    protected Rectangle filterListBounds = new Rectangle(1650, 200, 250, 725);

    protected AForge.Imaging.Filters.Invert invertFilter = new AForge.Imaging.Filters.Invert();

    protected Point lastClick = new Point(-1, -1);

    public ActionState(ActionThread parent, double delay)
    {
      this.parent = parent;
      this.delay = delay;
      this.nextDelay = delay;
      this.eventHWnd = parent.GetEventHWnd();
      this.threshHoldDictionary = parent.threshHoldDictionary;
    }

    public Rectangle GetOCRBounds(Rectangle r, int w, int h)
    {
      Rectangle ocrBounds = new Rectangle(r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8);
      if (ocrBounds.X < 0)
      {
        ocrBounds.X = 0;
      }
      if (ocrBounds.Y < 0)
      {
        ocrBounds.Y = 0;
      }
      if (ocrBounds.X + ocrBounds.Width > w)
      {
        ocrBounds.Width = w - ocrBounds.X;
      }
      if (ocrBounds.Y + ocrBounds.Height > h)
      {
        ocrBounds.Height = h - ocrBounds.Y;
      }
      return ocrBounds;
    }

    public string FindSingleWord(Bitmap bmp, Rectangle searchBounds)
    {
      using (bmp = bmp.Clone(searchBounds, PixelFormat.Format24bppRgb))
      {
        invertFilter.ApplyInPlace(bmp);
        return OCR.GetText(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height)).Trim();
      }
    }

    public Rectangle FindWord(string word, Rectangle searchBounds)
    {
      int i = 0;
      foreach (int threshhold in parent.threshHoldList)
      {
        Bitmap bmpTh = threshHoldDictionary[threshhold];
        using (Bitmap bmp = bmpTh.Clone(searchBounds, bmpTh.PixelFormat))
        {
          objectCounter.ProcessImage(bmp);

          Rectangle[] rects = objectCounter.GetObjectsRectangles();
          Dictionary<Rectangle, List<Rectangle>> lineBoundsDictionary = parent.GetBoundsDictionaryList(rects);

          foreach (Rectangle r in lineBoundsDictionary.Keys)
          {
            if (r.Width > 20 && r.Height > 10 && r.X > 4 && r.Y > 4)
            {
              Rectangle ocrBounds = GetOCRBounds(r, bmp.Width, bmp.Height);
              using (Bitmap ocrBmp = bmp.Clone(ocrBounds, PixelFormat.Format24bppRgb))
              {
                invertFilter.ApplyInPlace(ocrBmp);
                string foundWord = OCR.GetText(ocrBmp, new Rectangle(0,0, ocrBmp.Width, ocrBmp.Height)).Trim();
                if (foundWord == word)
                {
                  return new Rectangle(r.X + searchBounds.X, r.Y + searchBounds.Y, r.Width, r.Height);
                }
              }
            }
          }
        }
      }
      return NullRect;
    }

    public Double FindSingleDouble(int threshhold, Rectangle searchBounds)
    {
      double lastResult = Double.NaN;
      Rectangle resultRectangle = NullRect;

      //Not Selected
      using (Bitmap bmp = parent.threshHoldDictionary[0].Clone(searchBounds, PixelFormat.Format24bppRgb))
      {
        Grayscale grayFilter = new Grayscale(0.5, 0.5, 0.5);
        Threshold thFilter = new Threshold(128);
        Bitmap gbmp = grayFilter.Apply(bmp);
        thFilter.ApplyInPlace(gbmp);
        invertFilter.ApplyInPlace(gbmp);
        string foundWord = OCR.GetNumber(gbmp, new Rectangle(0, 0, gbmp.Width, gbmp.Height)).Trim().Replace('g', '9');
        Double.TryParse(foundWord, out lastResult);
      }
      if (Double.IsNaN(lastResult))
      {
        using (Bitmap bmp = parent.threshHoldDictionary[0].Clone(searchBounds, PixelFormat.Format24bppRgb))
        {
          ColorFiltering whiteFilter = new ColorFiltering(new AForge.IntRange(100, 255), new AForge.IntRange(100, 255), new AForge.IntRange(100, 255));
          Grayscale grayFilter = new Grayscale(0.25, 0.25, 0.25);
          Threshold thFilter = new Threshold(64);
          Bitmap gbmp = whiteFilter.Apply(bmp);
          gbmp = grayFilter.Apply(gbmp);
          thFilter.ApplyInPlace(gbmp);
          invertFilter.ApplyInPlace(gbmp);
          string foundWord = OCR.GetNumber(gbmp, new Rectangle(0, 0, gbmp.Width, gbmp.Height)).Trim().Replace('g', '9');
          Double.TryParse(foundWord, out lastResult);
        }
      }
      return lastResult;
    }

    public Rectangle FindDouble(ref double value, Rectangle searchBounds)
    {
      double lastResult = Double.NaN;
      Rectangle resultRectangle = NullRect;
      foreach (Bitmap thbmp in threshHoldDictionary.Values)
      {
        using (Bitmap bmp = thbmp.Clone(searchBounds, PixelFormat.Format24bppRgb))
        {
          objectCounter.ProcessImage(bmp);
          Rectangle[] rects = objectCounter.GetObjectsRectangles();
          Dictionary<Rectangle, List<Rectangle>> lineBoundsDictionary = parent.GetBoundsDictionaryList(rects);

          foreach (Rectangle r in lineBoundsDictionary.Keys)
          {
            Rectangle ocrBounds = GetOCRBounds(r, searchBounds.Width, searchBounds.Height);
            using (Bitmap ocrBmp = bmp.Clone(ocrBounds, PixelFormat.Format24bppRgb))
            {
              invertFilter.ApplyInPlace(ocrBmp);
              string foundWord = OCR.GetNumber(ocrBmp, new Rectangle(0, 0, ocrBmp.Width, ocrBmp.Height)).Trim();
              ocrBmp.Save("ObjBitmap\\" +  "_" + foundWord + "_" + ocrBounds + ".bmp");
              Console.WriteLine(foundWord);
              double result;
              if (Double.TryParse(foundWord, out result))
              {
                if (Double.IsNaN(lastResult) || result > lastResult)
                {
                  lastResult = result;
                  resultRectangle = new Rectangle(r.X + searchBounds.X, r.Y + searchBounds.Y, r.Width, r.Height);
                }
              }
            }
          }
        }
      }
      value = lastResult;
      return resultRectangle;
    }

    public virtual ActionState Run(double totalTime)
    {
      return nextState;
    }

    public virtual double GetDelay()
    {
      if (nextDelay != delay)
      {
        double d = nextDelay;
        nextDelay = delay;
        return d;
      }
      return delay;
    }

    public virtual int GetThreshHold()
    {
      return 64;
    }

    public virtual ActionState SetNextState(ActionState nextState)
    {
      this.nextState = nextState;
      return nextState;
    }

    public virtual void DrawDebug(Graphics g)
    {
      return;
    }
  }
  public class ActionStateInfiniteLoop : ActionState
  {
    public ActionStateInfiniteLoop(ActionThread parent) : base(parent, 100)
    {
    }

    public override ActionState Run(double totalTime)
    {
      return this;
    }
  }
}
