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
    public bool isTargeted;
    public EnemyTypes type;

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
    }

    public void UpgradeType(Rectangle r)
    {
      type++;
      bounds = Rectangle.Union(bounds, r);
    }
  }

  class PixelStateEnemies : PixelState
  {
    private List<EnemyInfo> enemyList = new List<EnemyInfo>();
    private Threshold filter128 = new Threshold(112);

    public PixelStateEnemies(ActionThreadNewsRAT parent) : base(parent)
    {
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
              eList[n - 1].UpgradeType(r);
            }
          }
          else if (r.Height > 9)
          {
            eList.Add(new EnemyInfo(r));
          }
        }
      }
      int i = 0;
      foreach (EnemyInfo e in eList)
      {
        i++;
        Rectangle r = e.bounds;
        Rectangle iconBounds = new Rectangle(0, r.Y - 5, battleIconBounds.Width, r.Height + 10);
        if (FindIconSimilarityCount(bmp112, "enemy_targeted", iconBounds, 112, 0.9f) > 0)
        {
          e.isTargeted = true;
        }
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
        lock(this)
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
