using AForge.Imaging.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  class ActionStateBattle : ActionState
  {
    private ActionThreadNewsRAT ratParent;

    private Grayscale battleIconGrayFilter = new Grayscale(1.0, 0.0, 0.0);
    private ColorFiltering battleIconRedFilter = new ColorFiltering(new AForge.IntRange(150, 255), new AForge.IntRange(22, 38), new AForge.IntRange(40, 60));
    private ColorFiltering whiteFilter = new ColorFiltering(new AForge.IntRange(200, 255), new AForge.IntRange(200, 255), new AForge.IntRange(200, 255));
    private Threshold battleIconThresholdFilter = new Threshold(50);

    private double lootingTime = 0;
    private double approachingTime = 0;
    private double targetAllTime = 0;

    private int looting = 0;

    private string wordSearch = null;
    private Rectangle searchBounds = new Rectangle(0, 0, 0, 0);

    public ActionStateBattle(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
      ratParent = parent;
    }

    public override int GetThreshHold()
    {
      return 128;
    }

    public Rectangle[] GetEnemyBounds()
    {
      Bitmap screenBmp = threshHoldDictionary[0];
      Bitmap iconColumnEnemies = screenBmp.Clone(battleIconBounds, PixelFormat.Format24bppRgb);
      iconColumnEnemies = battleIconRedFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconGrayFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconThresholdFilter.Apply(iconColumnEnemies);

      objectCounter.ProcessImage(iconColumnEnemies);
      return objectCounter.GetObjectsRectangles();
    }

    public Rectangle GetFirstLootBounds()
    {
      using (Bitmap iconColumnLoot = threshHoldDictionary[64].Clone(firstBattleIconBounds, PixelFormat.Format24bppRgb))
      {
        objectCounter.ProcessImage(iconColumnLoot);
      }
      Rectangle[] lootRectList = objectCounter.GetObjectsRectangles();
      foreach (Rectangle r in lootRectList)
      {
        if (r.Width > 12)
        {
          return new Rectangle(r.X + firstBattleIconBounds.X, r.Y + firstBattleIconBounds.Y, r.Width, r.Height);
        }
      }
      return NullRect;
    }

    public Point FindFirstEnemy(Rectangle[] battleIconRectList)
    {
      if (battleIconRectList.Length > 0)
      {
        Point center = parent.GetClickPoint(battleIconRectList[0]);
        center.X += battleIconBounds.X;
        center.Y += battleIconBounds.Y;

        return center;
      }
      return new Point(-1, -1);
    }

    public Rectangle FindTargetAll(Bitmap src)
    {
      using (Bitmap bmp = src.Clone(targetAllBounds, PixelFormat.Format24bppRgb))
      {
        objectCounter.ProcessImage(bmp);
      }
      Rectangle[] rects = objectCounter.GetObjectsRectangles();
      foreach (Rectangle r in rects)
      {
        if (r.Width > 30 && r.Height > 30)
        {
          return new Rectangle(r.X + targetAllBounds.X, r.Y + targetAllBounds.Y, r.Width, r.Height);
        }
      }
      return NullRect;
    }

    public override ActionState Run(double totalTime)
    {
      Bitmap screenBmp = threshHoldDictionary[0];
      Bitmap bmp64 = threshHoldDictionary[64];
      Bitmap bmp128 = threshHoldDictionary[128];

      if (totalTime > targetAllTime)
      {
        Rectangle targetAll = FindTargetAll(bmp128);
        if (targetAll.X > -1)
        {
          targetAllTime = totalTime + 5000;
          lastClick = parent.GetClickPoint(targetAll);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          return this;
        }
      }

      if (wordSearch != null)
      {
        Rectangle found = FindWord(wordSearch, searchBounds);
        if (found.X != -1)
        {
          lastClick = parent.GetClickPoint(found);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          if (looting == 2)
          {
            looting = 0;
            nextDelay = 3000;
          }
          else
          {
            nextDelay = 1500;
          }
          wordSearch = null;
          searchBounds = NullRect;
          return this;
        }
      }
      if (looting != 0)
      {
        if (wordSearch == null)
        {
          looting = 2;
          wordSearch = "Loot";
          searchBounds = lootAllBounds;
          lootingTime = totalTime + 5000;
        }
        return this;
      }

      Rectangle[] battleIconRectList = GetEnemyBounds();

      double distance = FindSingleDouble(96, firstItemDistanceBounds);
      Point enemyPoint = FindFirstEnemy(battleIconRectList);

      if (totalTime > lootingTime && (enemyPoint.X == -1 || distance < 10))
      {
        Rectangle lootBounds = GetFirstLootBounds();
        if (lootBounds.Width > 0)
        {
          Point center = parent.GetClickPoint(lootBounds);
          Win32.SendMouseClick(eventHWnd, center.X, center.Y);
          lastClick = center;

          nextDelay = 1500;
          wordSearch = "Loot";
          searchBounds = popupBounds;
          looting = 1;

          return this;
        }
      }
      WeaponState pulseLaser = null;
      if (!Double.IsNaN(distance) && distance < 20)
      {
        foreach (WeaponState weapon in ratParent.weaponDictionary.Values)
        {
          if (weapon.name != null && weapon.name.StartsWith("pulse-laser"))
          {
            pulseLaser = weapon;
            break;
          }
        }

        if (pulseLaser != null)
        {
          if (pulseLaser.IsActive(screenBmp) == WeaponStateFlag.InActive)
          {
            if (enemyPoint.X > -1)
            {
              Win32.SendMouseClick(eventHWnd, enemyPoint.X, enemyPoint.Y);
              lastClick = enemyPoint;

              nextDelay = 1500;
              wordSearch = "Focus";
              searchBounds = popupBounds;

              return this;
            }
          }
          else if (enemyPoint.X > -1 && pulseLaser.IsActive(screenBmp) == WeaponStateFlag.Active)
          {
            foreach (WeaponState weapon in ratParent.weaponDictionary.Values)
            {
              if (weapon.name.StartsWith("heat-sink") || weapon.name.StartsWith("nosferatu"))
              {
                if (weapon.IsActive(screenBmp) == WeaponStateFlag.InActive)
                {
                  Point center = parent.GetClickPoint(weapon.bounds);
                  Win32.SendMouseClick(eventHWnd, center.X, center.Y);
                  lastClick = center;
                  nextDelay = 1500;
                  return this;
                }
              }
            }
          }
        }
      }
      if (totalTime > approachingTime)
      {
        Point center = FindFirstEnemy(battleIconRectList);
        if (center.X > -1)
        {
          Win32.SendMouseClick(eventHWnd, center.X, center.Y);
          lastClick = center;

          if (Double.IsNaN(distance) || distance > 30)
          {
            wordSearch = "Approach";
          }
          else
          {
            wordSearch = "Orbit";
          }
          searchBounds = popupBounds;
          approachingTime = totalTime + 30000;

          return this;
        }
      }

      if (enemyPoint.X > -1)
      {
        foreach (WeaponState weapon in ratParent.weaponDictionary.Values)
        {
          if (weapon.name != null && (weapon.name.StartsWith("armor-repairer") || weapon.name.StartsWith("adaptive-armor-hardener")))
          {
            if (weapon.IsActive(screenBmp) == WeaponStateFlag.InActive)
            {
              Point center = parent.GetClickPoint(weapon.bounds);
              Win32.SendMouseClick(eventHWnd, center.X, center.Y);
              lastClick = center;
              nextDelay = 1500;
              return this;
            }
          }
        }
      }

      return this;
    }

    public override void DrawDebug(Graphics g)
    {
      Brush debugBackgroundColor = new SolidBrush(Color.FromArgb(128, 250, 0, 0));
      Brush weaponColor = new SolidBrush(Color.FromArgb(100, 50, 250, 250));
      Brush lastClickColor = new SolidBrush(Color.FromArgb(200, 250, 50, 250));
      Brush nextClickColor = new SolidBrush(Color.FromArgb(200, 250, 250, 50));
      g.FillRectangle(debugBackgroundColor, searchBounds);
      g.FillEllipse(nextClickColor, new Rectangle(lastClick.X - 15, lastClick.Y - 15, 30, 30));

      //g.FillRectangle(debugBackgroundColor, lastFoundDoubleBounds);
    }
  }
}
