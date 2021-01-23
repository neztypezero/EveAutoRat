using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  struct WordBounds
  {
    public WordBounds(string w, Rectangle b)
    {
      word = w;
      bounds = b;
    }

    public string word;
    public Rectangle bounds;
  }

  class ActionStateClickPoint : ActionState
  {
    public int x, y;
    public ActionStateClickPoint(ActionThreadNewsRAT parent, int x, int y, double delay) : base(parent, delay)
    {
      this.x = x;
      this.y = y;
    }

    public override ActionState Run(double totalTime)
    {
      Point center = parent.GetClickPoint(new Rectangle(x, y, 1, 1));
      Win32.SendMouseClick(parent.GetEventHWnd(), center.X, center.Y);
      return nextState;
    }
  }

  class ActionStateClickWordAt : ActionState
  {
    public int threshHold;
    public string word;
    public double timeout = -1;
    public double startingTime = -1;

    public ActionStateClickWordAt(ActionThreadNewsRAT parent, int threshHold, string word, double delay, double timeout) : base(parent, delay)
    {
      this.threshHold = threshHold;
      this.word = word;
      this.timeout = timeout;
    }

    public override int GetThreshHold()
    {
      return threshHold;
    }

    public override ActionState Run(double totalTime)
    {
      if (startingTime < 0)
      {
        startingTime = totalTime;
      }
      if (totalTime > startingTime + timeout)
      {
        return nextState;
      }
      string confirmText = FindSingleWord(threshHoldDictionary[128], confirmBounds);
      if (confirmText == "Confirm")
      {
        Point center = parent.GetClickPoint(confirmBounds);
        Win32.SendMouseClick(eventHWnd, center.X, center.Y);
        nextDelay = 2000;
        return this;
      }

      return this;
    }
  }

  class ActionStateClickWord : ActionState
  {
    public int threshHold;
    public string word;
    public bool loop;
    public bool looping;

    public ActionStateClickWord(ActionThreadNewsRAT parent, int threshHold, string word, double delay) : base(parent, delay)
    {
      this.threshHold = threshHold;
      this.word = word;
    }

    public ActionStateClickWord(ActionThreadNewsRAT parent, int threshHold, string word, double delay, bool loop) : this(parent, threshHold, word, delay)
    {
      this.loop = loop;
      looping = false;
    }

    public override int GetThreshHold()
    {
      return threshHold;
    }
    public override ActionState Run(double totalTime)
    {
      Bitmap bmp = parent.threshHoldDictionary[threshHold];
      foreach (Rectangle r in parent.lineBoundsDictionary.Keys)
      {
        if (r.X > 15 && r.Y > 25 && r.Width > 15 && r.Height > 15)
        {
          Rectangle exRect = new Rectangle(r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8);
          if ((exRect.X + exRect.Width) >= bmp.Width || (exRect.Y + exRect.Height) >= bmp.Height || exRect.X < 0 || exRect.Y < 0) {
            exRect = r;
          }
          using (Bitmap b = bmp.Clone(exRect, PixelFormat.Format24bppRgb))
          {
            invertFilter.ApplyInPlace(b);
            string foundWord = OCR.GetText(b, new Rectangle(0, 0, b.Width, b.Height)).Trim();
            if (word == foundWord)
            {
              Point center = parent.GetClickPoint(r);
              Win32.SendMouseClick(parent.GetEventHWnd(), center.X, center.Y);
              if (loop)
              {
                looping = true;
                return this;
              }
              return nextState;
            }
          }
        }

      }
      if (looping)
      {
        looping = false;
        return nextState;
      }
      return this;
    }
  }
  class ActionStateConfirmUntilEncounter : ActionState
  {
    private ActionThreadNewsRAT ratParent;
    private ActionStateBattle battleState;
    private string wordSearch = null;
    private Rectangle ZeroBounds = new Rectangle(0, 0, 0, 0);
    private Rectangle wordSearchBounds;

    public ActionStateConfirmUntilEncounter(ActionThreadNewsRAT parent, double delay, ActionStateBattle battleState) : base(parent, delay)
    {
      this.ratParent = parent;
      this.battleState = battleState;
      SetNextState(battleState);
      wordSearchBounds = ZeroBounds;
    }

    public override ActionState Run(double totalTime)
    {
      Rectangle[] enemyBoundsList = battleState.GetEnemyBounds();
      Bitmap bmp = parent.threshHoldDictionary[128];
      
      if (wordSearch != null)
      {
        Rectangle found = FindWord(wordSearch, wordSearchBounds);
        if (found.X != -1)
        {
          lastClick = parent.GetClickPoint(found);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1500;
          wordSearch = null;
          wordSearchBounds = ZeroBounds;
          return this;
        }
      }

      string confirmText = FindSingleWord(threshHoldDictionary[128], confirmBounds);
      if (confirmText == "Confirm")
      {
        Point center = parent.GetClickPoint(confirmBounds);
        Win32.SendMouseClick(eventHWnd, center.X, center.Y);
        nextDelay = 2000;
        return this;
      }

      foreach (WeaponState weapon in ratParent.weaponDictionary.Values)
      {
        if (weapon.name !=null && weapon.name.StartsWith("pulse-laser"))
        {
          if (weapon.IsActive(parent.threshHoldDictionary[0]) == WeaponStateFlag.InActive)
          {
            using (Bitmap eyeBmp = bmp.Clone(eyeBounds, PixelFormat.Format24bppRgb))
            {
              Rectangle poBounds = PixelObjectList.FindBitmap(eyeBmp, "eye", 128);
              if (poBounds.X > eyeBounds.Width / 2)
              {
                Point center = parent.GetClickPoint(new Rectangle(eyeBounds.X + poBounds.X, eyeBounds.Y + poBounds.Y, poBounds.Width, poBounds.Height));
                Win32.SendMouseClick(eventHWnd, center.X, center.Y);
                nextDelay = 1000;
                return this;
              }
            }

            string filterTitle = FindSingleWord(threshHoldDictionary[64], filterTitleBounds);
            if (filterTitle != "PiratesLoot")
            {
              Point center = parent.GetClickPoint(filterTitleBounds);
              Win32.SendMouseClick(eventHWnd, center.X, center.Y);
              nextDelay = 1000;
              wordSearch = "PiratesLoot";
              wordSearchBounds = filterListBounds;
              return this;
            }
          }
          break;
        }
      }

      if (enemyBoundsList != null && enemyBoundsList.Length > 0)
      {
        nextDelay = 12000;
        return battleState;
      }

      return this;
    }

    public override ActionState SetNextState(ActionState nextState)
    {
      if (battleState != null)
      {
        return battleState.SetNextState(nextState);
      }
      return base.SetNextState(nextState);
    }
  }

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
          return new Rectangle(r.X+targetAllBounds.X,r.Y+targetAllBounds.Y,r.Width,r.Height);
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
      Brush weaponColor =    new SolidBrush(Color.FromArgb(100, 50, 250, 250));
      Brush lastClickColor = new SolidBrush(Color.FromArgb(200, 250, 50, 250));
      Brush nextClickColor = new SolidBrush(Color.FromArgb(200, 250, 250, 50));
      g.FillRectangle(debugBackgroundColor, searchBounds);
      g.FillEllipse(nextClickColor, new Rectangle(lastClick.X - 15, lastClick.Y - 15, 30, 30));

      //g.FillRectangle(debugBackgroundColor, lastFoundDoubleBounds);
    }
  }

  class ActionThreadNewsRAT : ActionThread
  {
    private BlobCounter objectCounter = new BlobCounter();
    private Bitmap drawBuffer = null;

    private ActionState currentState;

    public Rectangle battleWeaponBounds = new Rectangle(1190, 850, 720, 260);
    public Dictionary<string, WeaponState> weaponDictionary = new Dictionary<string, WeaponState>();

    Font drawFont;

    public ActionThreadNewsRAT(EveAutoRatMainForm parentForm, IntPtr emuHWnd, IntPtr eventHWnd) : base(parentForm, emuHWnd, eventHWnd)
    {
      drawFont = new Font("Arial", 9);

      currentState = new ActionState(this, 100);
      currentState
        //.SetNextState(new ActionStateClickPoint(this, 150, 100, 1500))
        //.SetNextState(new ActionStateClickWord(this, 64, "Encounters", 3000))
        //.SetNextState(new ActionStateClickWord(this, 128, "News", 5000))
        //.SetNextState(new ActionStateClickWord(this, 128, "Accept", 1500))
        //.SetNextState(new ActionStateClickWordAt(this, 128, "Accept", 1500, 2500))
        //.SetNextState(new ActionStateClickWord(this, 64, "News", 2000))
        //.SetNextState(new ActionStateClickWord(this, 64, "Communication", 2000))
        //.SetNextState(new ActionStateClickWord(this, 128, "Begin", 200, true))
        //.SetNextState(new ActionStateConfirmUntilEncounter(this, 128, new ActionStateBattle(this, 100)))
        .SetNextState(new ActionStateBattle(this, 100))
        .SetNextState(new ActionStateInfiniteLoop(this))
      ;

      List<PixelObject> poList = PixelObjectList.GetPixelObjectList(0);
      WeaponStateNull nullState = new WeaponStateNull();
      foreach (PixelObject po in poList)
      {
        weaponDictionary[po.text] = nullState;
      }
    }

    public void LoadWeapons()
    {
      List<PixelObject> poList = PixelObjectList.GetPixelObjectList(0);
      using (Bitmap weaponAreaBitmap = threshHoldDictionary[0].Clone(battleWeaponBounds, PixelFormat.Format24bppRgb))
      {
        foreach (PixelObject po in poList)
        {
          if (weaponDictionary[po.text].IsNull)
          {
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.999f);
            TemplateMatch[] matchings = tm.ProcessImage(weaponAreaBitmap, po.bmp);

            if (matchings.Length > 0)
            {
              Rectangle r = matchings[0].Rectangle;
              weaponDictionary[po.text] = new WeaponState(po.bmp, new Rectangle(r.X + battleWeaponBounds.X, r.Y + battleWeaponBounds.Y, r.Width, r.Height), po.text);
            }
            else
            {
              break;
            }
          }
        }
      }
    }

    protected override void StepEvery(double totalTime)
    {
      LoadWeapons();
    }

    protected override double Step(double totalTime)
    {
      Bitmap currentThreshHoldBmp;
      int currentThreshHold = 64;
      if (currentState != null)
      {
        currentThreshHold = currentState.GetThreshHold();
      }
      currentThreshHoldBmp = threshHoldDictionary[currentThreshHold];
      if (drawBuffer == null)
      {
        drawBuffer = new Bitmap(currentThreshHoldBmp.Width, currentThreshHoldBmp.Height);
      }
      objectCounter.ProcessImage(currentThreshHoldBmp);
      Rectangle[] rects = objectCounter.GetObjectsRectangles();
      lineBoundsDictionary = GetBoundsDictionaryList(rects);

      using (Graphics g = Graphics.FromImage(drawBuffer))
      {
        g.DrawImage(threshHoldDictionary[128], zp);
        //g.FillRectangle(Brushes.Black, new Rectangle(0, 0, b64.Width, b64.Height));
        foreach (KeyValuePair<Rectangle, List<Rectangle>> item in lineBoundsDictionary)
        {
          int count = 0;
          Pen p = Pens.Red;// linePen[item.Key];
          foreach (Rectangle r in item.Value)
          {
            count++;
            g.DrawRectangle(p, r);
            g.DrawString(count.ToString(), drawFont, Brushes.Green, r.Location);
          }
        }
        if (weaponDictionary != null)
        {
          foreach (WeaponState weapon in weaponDictionary.Values)
          {
            WeaponStateFlag weaponFlag = weapon.IsActive(threshHoldDictionary[0]);
            if (weaponFlag == WeaponStateFlag.Active)
            {
              g.FillRectangle(Brushes.Red, weapon.bounds);
            }
            else if (weaponFlag == WeaponStateFlag.InActive)
            {
              g.FillRectangle(Brushes.Orange, weapon.bounds);
            }
            else
            {
              g.FillRectangle(Brushes.Yellow, weapon.bounds);
            }
          }
        }
        currentState.DrawDebug(g);
      }
      parentForm.Invoke(new Action(() =>
      {
        parentForm.BackgroundImage = drawBuffer.Clone(new Rectangle(0, 28, drawBuffer.Width, drawBuffer.Height-28), PixelFormat.Format24bppRgb);
      }));
      if (currentState != null)
      {
        currentState = currentState.Run(totalTime);
        if (currentState != null)
        {
          return currentState.GetDelay();
        }
      }
      return 100.0;
    }
  }
}
