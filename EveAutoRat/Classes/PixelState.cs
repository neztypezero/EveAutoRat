using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public class PixelState
  {
    protected static Rectangle startEveEchoesBounds = new Rectangle(838, 866, 226, 35);

    protected static Point encounterOpenDialogPoint = new Point(150, 100);
    protected static Rectangle encounterTileBounds = new Rectangle(333, 766, 71, 56);
    protected static Rectangle encounterNewsBounds = new Rectangle(64, 419, 52, 32);
    protected static Rectangle encounterBattleIconBounds = new Rectangle(368, 369, 44, 45);
    protected static Rectangle encounterBattleIconJournalBounds = new Rectangle(837, 611, 58, 58);
    protected static Rectangle encounterAcceptBounds = new Rectangle(1370, 940, 120, 50);
    protected static Rectangle encounterBeginBounds = new Rectangle(1260, 940, 400, 50);
    protected static Rectangle encounterRefreshBounds = new Rectangle(1086, 249, 44, 45);

    protected static Rectangle eyeClosedZeroBounds = new Rectangle(1837, 646, 21, 20);
    protected static Rectangle eyeOpenZeroBounds = new Rectangle(1460, 644, 21, 20);

    protected static Rectangle confirmBounds = new Rectangle(1672, 856, 130, 35);
    protected static Rectangle eyeClosedBounds = new Rectangle(1827, 644, 41, 25);
    protected static Rectangle eyeOpenBounds = new Rectangle(1449, 644, 41, 25);
    protected static Rectangle eyeBounds = new Rectangle(1440, 635, 480, 45);
    protected static Rectangle filterTitleBounds = new Rectangle(1625, 75, 150, 35);
    protected static Rectangle battleLockOnBounds = new Rectangle(1170, 707, 60, 60);
    protected static Rectangle battleIconBounds = new Rectangle(1545, 135, 39, 561);
    protected static Rectangle firstBattleIconBounds = new Rectangle(1545, 135, 42, 75);
    protected static Rectangle lootAllBounds = new Rectangle(700, 950, 380, 70);
    protected static Rectangle firstItemDistanceBounds = new Rectangle(1586, 164, 85, 26);
    protected static Rectangle popupBounds = new Rectangle(1300, 150, 200, 950);
    protected static Rectangle throttleBounds = new Rectangle(684, 986, 37, 31);
    protected static Rectangle targetAllBounds = new Rectangle(1167, 707, 69, 62);
    protected static Rectangle filterListBounds = new Rectangle(1650, 200, 250, 725);
    protected static Rectangle dialogArrowBounds = new Rectangle(810, 400, 50, 215);
    protected static Rectangle dialogSelfArrowBounds = new Rectangle(1640, 335, 45, 220);
    protected static Rectangle destinationClosedBounds = new Rectangle(23, 288, 25, 36);
    protected static Rectangle destinationOpenBounds = new Rectangle(405, 288, 25, 36);
    protected static Rectangle destinationSetBounds = new Rectangle(96, 290, 29, 28);
    protected static Rectangle destinationPinBounds = new Rectangle(21, 493, 17, 23);
    protected static Rectangle destinationSetAsBounds = new Rectangle(480, 480, 300, 55);
    protected static Rectangle destinationAutoBounds = new Rectangle(25, 294, 25, 25);
    protected static Rectangle undockBounds = new Rectangle(1740, 380, 160, 55);
    protected static Rectangle activeShipBounds = new Rectangle(31, 669, 33, 33);
    protected static Rectangle unloadSelectAllBounds = new Rectangle(1455, 989, 44, 45);
    protected static Rectangle unloadMoveToBounds = new Rectangle(57, 246, 55, 55);
    protected static Rectangle unloadMoveToItemHangerBounds = new Rectangle(315, 265, 65, 65);
    protected static Rectangle closeDialogBounds = new Rectangle(1836, 94, 32, 33);
    protected static Rectangle closeWreckageBounds = new Rectangle(1153, 159, 32, 32);
    protected static Rectangle inStationCheckBounds = new Rectangle(1700, 55, 100, 2);
    protected static Rectangle cargoHoldBounds = new Rectangle(6, 255, 140, 1);
    protected static Rectangle stationNameBounds = new Rectangle(1590, 260, 320, 36);
    protected static Rectangle jumpToBounds = new Rectangle(55, 470, 280, 36);
    protected static Rectangle noSearchResultsBounds = new Rectangle(1580, 625, 300, 60);
    protected static Rectangle topEnemyBounds = new Rectangle(800, 50, 750, 160);

    protected static Rectangle[] weaponBoundsList = new Rectangle[] {
      new Rectangle(1202, 859, 110, 110),
      new Rectangle(1319, 859, 110, 110),
      new Rectangle(1435, 859, 110, 110),
      new Rectangle(1550, 859, 110, 110),
      new Rectangle(1668, 859, 110, 110),
      new Rectangle(1784, 859, 110, 110),
      new Rectangle(1202, 991, 110, 110),
      new Rectangle(1319, 991, 110, 110),
      new Rectangle(1435, 991, 110, 110),
      new Rectangle(1550, 991, 110, 110),
      new Rectangle(1668, 991, 110, 110),
      new Rectangle(1784, 991, 110, 110),
    };

    protected ActionThreadNewsRAT parent;
    protected Rectangle NullRect = new Rectangle(-1, -1, 0, 0);

    protected AForge.Imaging.BlobCounter objectCounter = new AForge.Imaging.BlobCounter();
    protected Dictionary<int, Bitmap> threshHoldDictionary;
    protected System.IntPtr eventHWnd;

    protected static Grayscale battleIconGrayFilter = new Grayscale(1.0, 0.0, 0.0);
    protected static ColorFiltering battleIconRedFilter = new ColorFiltering(new AForge.IntRange(150, 255), new AForge.IntRange(22, 38), new AForge.IntRange(40, 60));
    protected static Threshold battleIconThresholdFilter = new Threshold(50);
    protected Invert invertFilter = new Invert();

    public PixelState(ActionThreadNewsRAT parent)
    {
      this.parent = parent;
      this.eventHWnd = parent.GetEventHWnd();
      this.threshHoldDictionary = parent.threshHoldDictionary;
    }

    public virtual void StepEvery(Bitmap screenBmp, double totalTime)
    {
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
                string foundWord = OCR.GetText(ocrBmp, new Rectangle(0, 0, ocrBmp.Width, ocrBmp.Height)).Trim();
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
        string foundWord = OCR.GetNumber(gbmp, new Rectangle(0, 0, gbmp.Width, gbmp.Height)).Trim();
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
          string foundWord = OCR.GetNumber(gbmp, new Rectangle(0, 0, gbmp.Width, gbmp.Height)).Trim();
          Double.TryParse(foundWord, out lastResult);
        }
      }
      return lastResult;
    }

    public Rectangle FindDouble(ref double value, Rectangle searchBounds)
    {
      Dictionary<double, List<Rectangle>> valueCounter = new Dictionary<double, List<Rectangle>>();
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
              //ocrBmp.Save("ObjBitmap\\" +  "_" + foundWord + "_" + ocrBounds + ".bmp");
              //Console.WriteLine(foundWord);
              double result;
              if (Double.TryParse(foundWord, out result))
              {
                Rectangle bounds = new Rectangle(r.X + searchBounds.X, r.Y + searchBounds.Y, r.Width, r.Height);
                if (!valueCounter.ContainsKey(result))
                {
                  valueCounter[result] = new List<Rectangle>();
                }
                valueCounter[result].Add(bounds);
              }
            }
          }
        }
      }
      if (valueCounter.Count > 0)
      {
        double currentValue = -1;
        int currentCount = 0;
        foreach (KeyValuePair<double, List<Rectangle>> r in valueCounter)
        {
          if (r.Value.Count > currentCount)
          {
            currentCount = r.Value.Count;
            currentValue = r.Key;
          }
          else if (r.Value.Count == currentCount && r.Key > currentValue)
          {
            currentValue = r.Key;
          }
        }
        value = currentValue;
        return valueCounter[value][0];
      }
      else
      {
        value = Double.NaN;
        return NullRect;
      }
    }

    public Rectangle FindIcon(Rectangle searchBounds, string iconName, int threshHold)
    {
      using (Bitmap bmp = parent.threshHoldDictionary[threshHold].Clone(searchBounds, PixelFormat.Format24bppRgb))
      {
        return PixelObjectList.FindBitmap(bmp, iconName, threshHold);
      }
    }

    public float FindIconSimilarity(Bitmap src, string iconName, Rectangle bounds, int threshHold)
    {
      Dictionary<string, PixelObject> poDict = PixelObjectList.GetPixelObjectDictionary(threshHold);
      if (poDict.ContainsKey(iconName))
      {
        using (Bitmap bmp = src.Clone(bounds, PixelFormat.Format24bppRgb))
        {
          ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.0f);
          TemplateMatch[] matchings = tm.ProcessImage(bmp, poDict[iconName].bmp);
          if (matchings.Length > 0)
          {
            return matchings[0].Similarity;
          }
        }
      }
      return -1;
    }

    public int FindIconSimilarityCount(Bitmap src, string iconName, Rectangle bounds, int threshHold, float similarity)
    {
      List<float> valueList = new List<float>();
      Dictionary<string, PixelObject> poDict = PixelObjectList.GetPixelObjectDictionary(threshHold);
      if (poDict.ContainsKey(iconName))
      {
        using (Bitmap bmp = src.Clone(bounds, PixelFormat.Format24bppRgb))
        {
          ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(similarity);
          TemplateMatch[] matchings = tm.ProcessImage(bmp, poDict[iconName].bmp);
          return matchings.Length;
        }
      }
      return 0;
    }

    public int GetStringSimilarity(string s, string t)
    {
      if (string.IsNullOrEmpty(s))
      {
        throw new ArgumentNullException(s, "String Cannot Be Null Or Empty");
      }

      if (string.IsNullOrEmpty(t))
      {
        throw new ArgumentNullException(t, "String Cannot Be Null Or Empty");
      }

      int n = s.Length; // length of s
      int m = t.Length; // length of t

      if (n == 0)
      {
        return m;
      }

      if (m == 0)
      {
        return n;
      }

      int[] p = new int[n + 1]; //'previous' cost array, horizontally
      int[] d = new int[n + 1]; // cost array, horizontally

      // indexes into strings s and t
      int i; // iterates through s
      int j; // iterates through t

      for (i = 0; i <= n; i++)
      {
        p[i] = i;
      }

      for (j = 1; j <= m; j++)
      {
        char tJ = t[j - 1]; // jth character of t
        d[0] = j;

        for (i = 1; i <= n; i++)
        {
          int cost = s[i - 1] == tJ ? 0 : 1; // cost
                                             // minimum of cell to the left+1, to the top+1, diagonally left and up +cost                
          d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
        }

        // copy current distance counts to 'previous row' distance counts
        int[] dPlaceholder = p; //placeholder to assist in swapping p and d
        p = d;
        d = dPlaceholder;
      }

      // our last action in the above loop was to switch d and p, so p now 
      // actually has the most recent cost counts
      return p[n];
    }

    public virtual void Draw(Graphics g)
    {

    }
  }
}
