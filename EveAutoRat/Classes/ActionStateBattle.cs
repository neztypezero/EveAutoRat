using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Collections.Generic;

namespace EveAutoRat.Classes
{
  public class ActionStateBattle : ActionState
  {
    private double targetAllTime = 0;
    private bool battleOver = false;
    private double battleOverWaitTime = 0;
    private double currentTotalTime = 0;
    private double wordSearchTimeout = 0;

    private Font infoFont = new Font("Arial", 12);
    private Rectangle infoBounds = new Rectangle(10, 300, 320, 500);

    private string wordSearch = null;
    private Rectangle searchBounds = new Rectangle(0, 0, 0, 0);

    public ActionStateBattle(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override void Reset()
    {
      battleOverWaitTime = 0;
      battleOver = false;
      wordSearch = null;
      searchBounds = NullRect;
      wordSearchTimeout = 0;
    }

    public override void Draw(Graphics g)
    {
      base.Draw(g);
      Brush stateBg = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
      Brush textColor = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
      g.FillRectangle(stateBg, infoBounds);
      Point textPoint = new Point(infoBounds.X + 20, infoBounds.Y + 20);
      g.DrawString("" + currentTotalTime, infoFont, textColor, textPoint);
      textPoint.Y += 20;
      g.DrawString("" + targetAllTime, infoFont, textColor, textPoint);
      textPoint.Y += 20;
      g.DrawString("" + battleOver, infoFont, textColor, textPoint);
      textPoint.Y += 20;
      g.DrawString("" + wordSearch, infoFont, textColor, textPoint);
      textPoint.Y += 20;
      g.DrawString("" + searchBounds, infoFont, textColor, textPoint);
    }
    public override int GetThreshHold()
    {
      return 128;
    }

    public bool GetFirstActivatedTarget()
    {
      using (Bitmap iconColumnLoot = parent.GetThreshHoldBitmap(64).Clone(firstBattleIconBounds, PixelFormat.Format24bppRgb))
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
      Bitmap screenBmp = parent.GetThreshHoldBitmap(0);
      Bitmap bmp64 = parent.GetThreshHoldBitmap(64);
      Bitmap bmp128 = parent.GetThreshHoldBitmap(128);

      if (wordSearch != null)
      {
        FoundWord found = FindWord(wordSearch, searchBounds);
        if (found != null)
        {
          lastClick = parent.GetClickPoint(found.r);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          wordSearch = null;
          searchBounds = NullRect;
          nextDelay = 1000;
          return this;
        }
        else if (totalTime < wordSearchTimeout)
        {
          return this;
        }
        else
        {
          wordSearch = null;
          searchBounds = NullRect;
          wordSearchTimeout = 0;
        }
      }


      List<Rectangle> enemyBoundsList = GetEnemyBounds();
      List<Rectangle> smallEnemies = GetSmallEnemyBoundList(enemyBoundsList);
      List<Rectangle> enemyTargetedList = GetTargetEnemyList();

      currentTotalTime = totalTime;

      WeaponState pulseLaser = GetWeapon();
      if (enemyBoundsList.Count == 0 || (enemyTargetedList.Count == 0 && smallEnemies.Count > 0))
      {
        targetAllTime = totalTime + 5000;
      }

      if (totalTime > targetAllTime)
      {
        if ((smallEnemies.Count == 0 || (smallEnemies.Count == 1 && enemyTargetedList.Count > 0)))
        {
          if (FindIconSimilarity(bmp128, "target_all", targetAllBounds, 128) > 0.9f)
          {
            if (targetAllTime == 0)
            {
              targetAllTime = totalTime + 5000;
              return this;
            }
            targetAllTime = totalTime + 5000;
            lastClick = parent.GetClickPoint(targetAllBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
        }
        else
        {
          targetAllTime = 0;
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
      }
      if (smallEnemies.Count > 0 && wordSearch == null && totalTime > pulseLaser.clickTime && FindIconSimilarity(bmp128, "target_all", targetAllBounds, 128) > 0.9f)
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

            if (pulseLaser.CurrentState == WeaponStateFlag.Active)
            {
              wordSearch = "Lock";
            }
            else
            {
              wordSearch = "Focus";
            }
            wordSearchTimeout = totalTime + 2500;
            pulseLaser.clickTime = totalTime + 10000;
            searchBounds = popupBounds;
            return this;
          }
        }
      }
      float eyeClosed = FindIconSimilarity(bmp128, "eye", eyeClosedBounds, 128);
      if (eyeClosed > 0.90f)
      {
        lastClick = parent.GetClickPoint(eyeClosedBounds);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        nextDelay = 1000;
        return this;
      }
      float eyeOpen = FindIconSimilarity(bmp128, "eye", eyeOpenBounds, 128);
      if (eyeOpen > 0.9f)
      {
        string filterTitle = FindSingleWord(bmp64, filterTitleBounds);
        if (filterTitle != "Pirates")
        {
          lastClick = parent.GetClickPoint(filterTitleBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1000;
          wordSearch = "Pirates";
          wordSearchTimeout = totalTime + 2500;
          searchBounds = filterListBounds;
          return this;
        }
      }

      return this;
    }
  }
}
