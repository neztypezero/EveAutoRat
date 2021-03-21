using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;

namespace EveAutoRat.Classes
{
  public enum EnemyTypes : int
  {
    Unknown = 0,
    Frigate = 1,
    Destroyer = 2,
    Cruiser = 3,
    BattleCruiser = 4,
    BattleShip = 5,

  }

  public class EnemyInfo
  {
    public Rectangle bounds;
    public Rectangle checkBounds;
    public bool isTargeted;
    public EnemyTypes type;
    public Dictionary<int, Rectangle> distanceBounds;
    public Dictionary<int, Bitmap> distanceBmp;
    public int distance;

    public EnemyInfo(Rectangle bounds)
    {
      this.bounds = bounds;
      this.isTargeted = false;
      if (this.bounds.Height > 10 && this.bounds.Height < 13)
      {
        this.type = EnemyTypes.Frigate;
      }
      else if (this.bounds.Height > 17 && this.bounds.Height < 21)
      {
        this.type = EnemyTypes.Cruiser;
      }
      else if (this.bounds.Height > 24)
      {
        this.type = EnemyTypes.BattleShip;
      }
      else
      {
        this.type = EnemyTypes.Unknown;
      }
      checkBounds = new Rectangle(bounds.X, bounds.Y, 5000, 2);
      this.distanceBounds = new Dictionary<int, Rectangle>();
      this.distanceBmp = new Dictionary<int, Bitmap>();
    }

    public void UpgradeType(Rectangle r)
    {
      type++;
      bounds = Rectangle.Union(bounds, r);
    }

    public bool AddDistanceRectangle(Rectangle r, int threshold)
    {
      if (r.Width < 30 && r.Height < 30 & r.Width > 9 && r.Height > 9)
      {
        if (checkBounds.IntersectsWith(r))
        {
          if (!this.distanceBounds.ContainsKey(threshold))
          {
            this.distanceBounds[threshold] = r;
          }
          else
          {
            this.distanceBounds[threshold] = Rectangle.Union(this.distanceBounds[threshold], r);
          }
          return true;
        }
      }
      return false;
    }

    public void SetDistanceBmp(Bitmap bmp, int threshold, int xOff, int yOff)
    {
      if (this.distanceBounds.ContainsKey(threshold))
      {
        Rectangle r = this.distanceBounds[threshold];
        r.X -= xOff;
        r.Y -= yOff;
        if (r.X > -1 && r.Y > -1 && (r.X + r.Width) < bmp.Width && (r.Y + r.Height) < bmp.Height)
        {
          this.distanceBmp[threshold] = bmp.Clone(r, bmp.PixelFormat);
        }
      }
    }
  }

  class PixelStateEnemies : PixelState
  {
    private List<EnemyInfo> enemyList = new List<EnemyInfo>();
    private Threshold filter64 = new Threshold(64);
    private Threshold filter80 = new Threshold(80);
    private Threshold filter96 = new Threshold(96);
    private Threshold filter112 = new Threshold(112);
    private Threshold filter128 = new Threshold(128);

    private Threshold[] thresholdList;
    //private OCR ocr = new OCR();

    public PixelStateEnemies(ActionThreadNewsRAT parent) : base(parent)
    {
      thresholdList = new Threshold[] {
        filter64,
        filter80,
        filter96,
        filter112,
        filter128,
      };
    }

    public override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      Bitmap iconColumnEnemies = screenBmp.Clone(battleIconBounds, PixelFormat.Format24bppRgb);
      Bitmap bmp112 = null;
      using (Bitmap gray = parent.grayScaleFilter.Apply(iconColumnEnemies))
      {
        bmp112 = filter128.Apply(gray);
      }
      iconColumnEnemies = battleIconRedFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconGrayFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconThresholdFilter.Apply(iconColumnEnemies);

      objectCounter.ProcessImage(iconColumnEnemies);
      iconColumnEnemies.Dispose();
      Rectangle[] rArray = objectCounter.GetObjectsRectangles();

      List<EnemyInfo> eList = new List<EnemyInfo>();
      foreach (Rectangle r in rArray)
      {
        if (r.Width > 20)
        {
          if (r.Height < 3)
          {
            int n = eList.Count;
            if (n > 0)
            {
              eList[n - 1].UpgradeType(new Rectangle(r.X + battleIconBounds.X, r.Y + battleIconBounds.Y, r.Width, r.Height));
            }
          }
          else if (r.Height > 9)
          {
            eList.Add(new EnemyInfo(new Rectangle(r.X + battleIconBounds.X, r.Y + battleIconBounds.Y, r.Width, r.Height)));
          }
        }
      }

      //Bitmap distanceBitmap = screenBmp.Clone(enemyDistanceBounds, PixelFormat.Format24bppRgb);
      //using (Bitmap gray = parent.grayScaleFilter.Apply(distanceBitmap))
      //{
      //  foreach (Threshold filter in thresholdList)
      //  {
      //    Bitmap filteredBmp = filter.Apply(gray);
      //    int threshold = filter.ThresholdValue;

      //    objectCounter.ProcessImage(filteredBmp);
      //    Rectangle[] distanceObjects = objectCounter.GetObjectsRectangles();
      //    foreach (Rectangle rr in distanceObjects)
      //    {
      //      Rectangle r = new Rectangle(rr.X + enemyDistanceBounds.X - 5, rr.Y + enemyDistanceBounds.Y - 5, rr.Width + 10, rr.Height + 10);
      //      foreach (EnemyInfo e in eList)
      //      {
      //        if (e.AddDistanceRectangle(r, threshold))
      //        {
      //          break;
      //        }
      //      }
      //    }
      //    foreach (EnemyInfo e in eList)
      //    {
      //      e.SetDistanceBmp(filteredBmp, threshold, enemyDistanceBounds.X, enemyDistanceBounds.Y);
      //    }
      //  }
      //}
      foreach (EnemyInfo e in eList)
      {
        Rectangle r = e.bounds;
        Rectangle iconBounds = new Rectangle(0, r.Y - battleIconBounds.Y - 5, battleIconBounds.Width, r.Height + 10);
        if (FindIconSimilarityCount(bmp112, "enemy_targeted", iconBounds, 112, 0.9f) > 0)
        {
          e.isTargeted = true;
        }
      }
      if (eList.Count < enemyList.Count && eList.Count - enemyList.Count < -1)
      {
        return;
      }
      lock (this)
      {
        enemyList = eList;
      }
    }

    public EnemyInfo[] EnemyList
    {
      get
      {
        lock (this)
        {
          return enemyList.ToArray();
        }
      }
    }

    public int EnemyTargetedCount
    {
      get
      {
        return 0;
      }
    }
  }
}
