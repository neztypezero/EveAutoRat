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
    private double smallClickTime = 0;
    private double wordSearchTime = 0;

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

    public Point FindFirstEnemy(List<Rectangle> battleIconRectList)
    {
      if (battleIconRectList.Count > 0)
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

      List<Rectangle> enemyBoundsList = GetEnemyBounds();
      List<Rectangle> smallEnemies = GetSmallEnemyBoundList(enemyBoundsList);
      List<Rectangle> enemyTargetedList = GetTargetEnemyList();

      WeaponState pulseLaser = GetWeapon();
      if (enemyTargetedList.Count == 0)
      {
        targetAllTime = totalTime + 5000;
      }
      if ((enemyTargetedList.Count == 0 || pulseLaser.CurrentState == WeaponStateFlag.InActive) && enemyBoundsList.Count > 0 && totalTime > pulseLaser.clickTime)
      {
        Rectangle er;
        if (smallEnemies.Count > 0)
        {
          er = smallEnemies[0];
        }
        else
        {
          er = enemyBoundsList[0];
        }
        er.X += battleIconBounds.X;
        er.Y += battleIconBounds.Y;
        lastClick = parent.GetClickPoint(er);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);

        wordSearch = "Focus";
        wordSearchTime = totalTime + 5000;
        searchBounds = popupBounds;
        nextDelay = 1500;
        pulseLaser.clickTime = totalTime + 10000;
        targetAllTime = totalTime + 5000;
      }
      else if  (pulseLaser.CurrentState == WeaponStateFlag.Active && smallEnemies.Count > 1 && wordSearch == null)
      {
        foreach (Rectangle sr in smallEnemies)
        {
          bool isTargeted = false;
          foreach (Rectangle r in enemyTargetedList)
          {
            if (sr.IntersectsWith(r))
            {
              isTargeted = true;
              break;
            }
          }
          if (!isTargeted)
          {
            Rectangle er = sr;
            er.X += battleIconBounds.X;
            er.Y += battleIconBounds.Y;
            lastClick = parent.GetClickPoint(er);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);

            wordSearch = "Lock";
            wordSearchTime = totalTime + 5000;
            searchBounds = popupBounds;
            nextDelay = 1500;

            return this;
          }
        }
      }

      if (totalTime > targetAllTime && smallEnemies.Count <= 1)
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
          else if (totalTime > wordSearchTime)
          {
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
        if (battleOverRect.X == -1)
        {
          battleOverRect = FindIcon(dialogSelfArrowBounds, "dialog_arrow", 80);
          if (battleOverRect.X != -1)
          { 
            battleOverRect.X += dialogSelfArrowBounds.X;
            battleOverRect.Y += dialogSelfArrowBounds.Y;
          }
        }
        else
        {
          battleOverRect.X += dialogArrowBounds.X;
          battleOverRect.Y += dialogArrowBounds.Y;
        }
        if (battleOverRect.X > -1)
        {
          battleOver = true;
          lastClick = parent.GetClickPoint(battleOverRect);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 400;
          battleOverWaitTime = totalTime + 2500;
          return this;
        }
        if (battleOver)
        {
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
        //List<Rectangle> smallEnemies = GetSmallEnemyBoundList(enemyBoundsList);
        //if (smallEnemies.Count > 1 && totalTime > smallClickTime)
        //{
        //  lastClick = parent.GetClickPoint(smallEnemies[0]);
        //  lastClick.X += battleIconBounds.X;
        //  lastClick.Y += battleIconBounds.Y;
        //  Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        //  smallClickTime = totalTime + 20000;
        //}
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
