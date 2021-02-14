using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;

namespace EveAutoRat.Classes
{
  public class ActionStateBattle : ActionState
  {
    private double lootingTime = 0;
    private double targetAllTime = 0;
    private bool battleOver = false;
    private double battleOverWaitTime = 0;
    private bool smallShips = true;

    private int looting = 0;

    private string wordSearch = null;
    private Rectangle searchBounds = new Rectangle(0, 0, 0, 0);

    public ActionStateBattle(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override void Reset()
    {
      battleOverWaitTime = 0;
      battleOver = false;
      looting = 0;
      lootingTime = 0;
      wordSearch = null;
      searchBounds = NullRect;
      smallShips = true;
    }

    public override int GetThreshHold()
    {
      return 128;
    }

    public bool GetFirstActivatedTarget()
    {
      using (Bitmap iconColumnLoot = threshHoldDictionary[64].Clone(firstBattleIconBounds, PixelFormat.Format24bppRgb))
      {
        objectCounter.ProcessImage(iconColumnLoot);
      }
      Rectangle[] lootRectList = objectCounter.GetObjectsRectangles();
      int count = 0;
      foreach (Rectangle r in lootRectList)
      {
        if (r.Width > 3 && r.Width < 10 && r.Height > 3 && r.Height < 16)
        {
          count++;
        }
      }
      return count == 2;
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

    public override ActionState Run(double totalTime)
    {
      Bitmap screenBmp = threshHoldDictionary[0];
      Bitmap bmp64 = threshHoldDictionary[64];
      Bitmap bmp128 = threshHoldDictionary[128];

      Rectangle[] enemyBoundsList = GetEnemyBounds();

      int enemyTargetedCount = GetTargetEnemyCount();

      WeaponState pulseLaser = GetWeapon();
      if (enemyTargetedCount == 0 && enemyBoundsList.Length > 0 && totalTime > pulseLaser.clickTime)
      {
        Rectangle er = GetSmallestEnemyBounds(enemyBoundsList);
        er.X += battleIconBounds.X;
        er.Y += battleIconBounds.Y;
        lastClick = parent.GetClickPoint(er);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        Thread.Sleep(25);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        Thread.Sleep(250);

        if (pulseLaser.CurrentState == WeaponStateFlag.InActive)
        {
          lastClick = parent.GetClickPoint(pulseLaser.bounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          pulseLaser.clickTime = totalTime + 10000;
        }
        targetAllTime = totalTime + 5000;
        nextDelay = 3000;
        return this;
      }

      if (totalTime > targetAllTime)
      {
        if (FindIconSimilarity(bmp128, "target_all", targetAllBounds, 128) > 0.9f)
        {
          targetAllTime = totalTime + 5000;
          lastClick = parent.GetClickPoint(targetAllBounds);
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
          wordSearch = null;
          searchBounds = NullRect;
          nextDelay = 2000;
          if (looting == 2)
          {
            looting = 0;
          }
          return this;
        }
        else
        {
          if (looting > 0 && totalTime > lootingTime)
          {
            looting = 0;
            lootingTime = 0;
            wordSearch = null;
            searchBounds = NullRect;
          }
          else
          {
            return this;
          }
        }
      }
      if (battleOver)
      {
        if (pulseLaser != null)
        {
          foreach (WeaponState weapon in parent.WeaponsState.Values)
          {
            if (weapon.CurrentState == WeaponStateFlag.Active)
            {
              if (totalTime > weapon.clickTime && !weapon.name.StartsWith("afterburner"))
              {
                lastClick = parent.GetClickPoint(weapon.bounds);
                Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
                weapon.clickTime = totalTime + 10000;
                return this;
              }
            }
          }
        }
      }

      Point enemyPoint = FindFirstEnemy(enemyBoundsList);
      if (enemyPoint.X == -1)
      {
        Rectangle battleOverRect = FindIcon(dialogArrowBounds, "dialog_arrow", 128);
        if (battleOverRect.X > -1)
        {
          battleOver = true;
          lastClick = parent.GetClickPoint(battleOverRect);
          lastClick.X += dialogArrowBounds.X;
          lastClick.Y += dialogArrowBounds.Y;
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 400;
          battleOverWaitTime = totalTime + 2500;
          return this;
        }
        if (battleOver)
        {
          Rectangle battleOverRect2 = FindIcon(dialogSelfArrowBounds, "dialog_arrow", 128);
          if (battleOverRect2.X > -1)
          {
            battleOver = true;
            lastClick = parent.GetClickPoint(battleOverRect2);
            lastClick.X += dialogArrowBounds.X;
            lastClick.Y += dialogArrowBounds.Y;
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            battleOverWaitTime = totalTime + 2500;
            nextDelay = 400;
            return this;
          }

          if (totalTime > battleOverWaitTime)
          {
            return NextState;
          }
        }
      }
      else
      {
        battleOverWaitTime = 0;
        battleOver = false;
      }

      if (pulseLaser != null)
      {
        if (pulseLaser.CurrentState == WeaponStateFlag.Active)
        {
          foreach (WeaponState weapon in parent.WeaponsState.Values)
          {
            if (weapon.CurrentState == WeaponStateFlag.InActive)
            {
              if (weapon != pulseLaser && totalTime > weapon.clickTime)
              {
                lastClick = parent.GetClickPoint(weapon.bounds);
                Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
                weapon.clickTime = totalTime + 5000;
                return this;
              }
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
    }
  }
}
