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
    private int wave = 1;
    private int lastEnemyCount = 99;

    private string iconSearch;
    private Rectangle iconSearchBounds = new Rectangle(0, 0, 0, 0);

    private string wordSearch = null;
    private Rectangle searchBounds = new Rectangle(0, 0, 0, 0);

    private EnemyInfo[] currentEnemies = new EnemyInfo[] {};

    public ActionStateBattle(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override void Reset()
    {
      battleOverWaitTime = 0;
      battleOver = false;
      iconSearch = null;
      wordSearch = null;
      searchBounds = NullRect;
      wordSearchTimeout = 0;
      wave = 1;
      lastEnemyCount = 99;
    }

    public override void Draw(Graphics g)
    {
      base.Draw(g);

      WeaponState pulseLaser = GetWeapon();

      Brush stateBg = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
      Brush textColor = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
      g.FillRectangle(stateBg, infoBounds);
      Point textPoint = new Point(infoBounds.X + 20, infoBounds.Y + 20);

      g.DrawString("Current Wave: " + wave, infoFont, textColor, textPoint);
      textPoint.Y += 20;
      g.DrawString("Last Enemy Count: " + lastEnemyCount, infoFont, textColor, textPoint);

      foreach (WeaponState weapon in parent.WeaponsState.Values)
      {
        textPoint.Y += 20;
        g.DrawString(weapon.name + " " + weapon.CurrentState, infoFont, textColor, textPoint);
      }
      textPoint.Y += 20;
      foreach (EnemyInfo enemy in currentEnemies)
      {
        textPoint.Y += 20;
        g.DrawString(enemy.type + ": " + enemy.isTargeted + " " + enemy.distance, infoFont, textColor, textPoint);
      }
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

    public Point FindFirstEnemy(EnemyInfo[] enemyList)
    {
      if (enemyList.Length > 0)
      {
        Point center = parent.GetClickPoint(enemyList[0].bounds);
        center.X += battleIconBounds.X;
        center.Y += battleIconBounds.Y;

        return center;
      }
      return new Point(-1, -1);
    }

    public void CalculateCurrentEnemiesDistances()
    {
      double d = Double.NaN;
      foreach (EnemyInfo e in currentEnemies)
      {
        Dictionary<int, int> nList = new Dictionary<int, int>();
        foreach (int threshold in e.distanceBmp.Keys)
        {
          Bitmap bmp = e.distanceBmp[threshold];
          Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);
          if (Double.TryParse(OCR.GetTextInvert(bmp, r), out d))
          {
            int i = (int)d;
            if (nList.ContainsKey(i))
            {
              nList[i]++;
            }
            else
            {
              nList[i] = 1;
            }
          }
        }
        if (nList.Count > 0)
        {
          int distance = 0;
          int count = 0;
          foreach (int i in nList.Keys)
          {
            if (nList[i] > count)
            {
              distance = i;
              count = nList[i];
            }
          }
          e.distance = distance;
        }
      }
    }

    public override ActionState Run(double totalTime)
    {
      Bitmap bmp64 = parent.GetThreshHoldBitmap(64);
      Bitmap bmp128 = parent.GetThreshHoldBitmap(128);

      currentEnemies = GetEnemyList();
      int currentEnemyCount = currentEnemies.Length;
      if (lastEnemyCount <= 1 && currentEnemyCount > 1)
      {
        wave++;
        targetAllTime = totalTime + 5000;
      }
      lastEnemyCount = currentEnemyCount;
      //CalculateCurrentEnemiesDistances();

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
      if (iconSearch != null)
      {
        List<Rectangle> iconsFound = FindIconSimilarityList(bmp128, iconSearch, battleActionIconBounds, 128, 0.9f);
        if (iconsFound.Count == 1)
        {
          lastClick = parent.GetClickPoint(iconsFound[0]);
          lastClick.X += battleActionIconBounds.X;
          lastClick.Y += battleActionIconBounds.Y;
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          iconSearch = null;
          nextDelay = 1000;
        }
        if (totalTime > wordSearchTimeout)
        {
          iconSearch = null;
        }
        else
        {
          return this;
        }
      }

      EnemyInfo[] smallEnemies = GetSmallEnemyList(currentEnemies);

      currentTotalTime = totalTime;

      WeaponState pulseLaser = GetWeapon();

      if (currentEnemies.Length == 0 || smallEnemies.Length > 1)
      {
        targetAllTime = totalTime + 5000;
      }

      Point enemyPoint = FindFirstEnemy(currentEnemies);
      if (FindIconSimilarity(bmp128, "target_all", targetAllBounds, 128) > 0.9f)
      {
        if (smallEnemies.Length > 0 && wordSearch == null && iconSearch == null && totalTime > pulseLaser.clickTime)
        {
          foreach (EnemyInfo enemy in smallEnemies)
          {
            if (!enemy.isTargeted)
            {
              Rectangle er = enemy.bounds;
              lastClick = parent.GetClickPoint(er);
              Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);

              if (pulseLaser.CurrentState == WeaponStateFlag.Active)
              {
                iconSearch = "lock";
              }
              else
              {
                iconSearch = "focus_fire";
              }
              nextDelay = 500;
              pulseLaser.clickTime = totalTime + 10000;
              wordSearchTimeout = totalTime + 5000;
              targetAllTime = totalTime + 10000;
              
              return this;
            }
          }
        }
        if (totalTime > targetAllTime)
        {
          if ((smallEnemies.Length == 0 || (smallEnemies.Length == 1 && smallEnemies[0].isTargeted)))
          {
            targetAllTime = totalTime + 5000;
            lastClick = parent.GetClickPoint(targetAllBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
          else
          {
            targetAllTime = 0;
          }
        }
      } 
      else if (enemyPoint.X > -1 && totalTime > targetAllTime  && pulseLaser.inactiveTime != 0 && totalTime > (pulseLaser.inactiveTime + 5000))
      {
        lastClick = enemyPoint;
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        iconSearch = "focus_fire";
        wordSearchTimeout = totalTime + 2500;
        return this;
      }
      if (smallEnemies.Length > 1)
      {
        foreach (EnemyInfo e in currentEnemies)
        {
          if (e.isTargeted && e.type >= EnemyTypes.Cruiser)
          {
            Rectangle er = e.bounds;
            lastClick = parent.GetClickPoint(er);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);

            iconSearch = "lock";
            nextDelay = 500;
            wordSearchTimeout = totalTime + 5000;
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
