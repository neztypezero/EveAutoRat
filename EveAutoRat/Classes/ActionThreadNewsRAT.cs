using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  class ActionThreadNewsRAT : ActionThread
  {
    private BlobCounter objectCounter = new BlobCounter();

    private ActionState currentState;

    public Rectangle battleWeaponBounds = new Rectangle(1190, 850, 720, 260);
    public Dictionary<string, WeaponState> weaponDictionary = new Dictionary<string, WeaponState>();
    public bool allWeaponsLoaded = false;

    Font drawFont;

    public ActionThreadNewsRAT(EveAutoRatMainForm parentForm, IntPtr emuHWnd, IntPtr eventHWnd) : base(parentForm, emuHWnd, eventHWnd)
    {
      drawFont = new Font("Arial", 9);

      currentState = new ActionStateNOP(this, 100);
      currentState
        //.SetNextState(new ActionStateClickPoint(this, 150, 100, 1500))
        //.SetNextState(new ActionStateClickWord(this, 64, "Encounters", 3000))
        //.SetNextState(new ActionStateClickWord(this, 128, "News", 5000))
        //.SetNextState(new ActionStateClickWord(this, 128, "Accept", 1500))
        //.SetNextState(new ActionStateClickWordAt(this, 128, "Accept", 1500, 2500))
        //.SetNextState(new ActionStateClickWord(this, 64, "News", 2000))
        //.SetNextState(new ActionStateClickWord(this, 64, "Communication", 2000))
        //.SetNextState(new ActionStateClickWord(this, 128, "Begin", 200, true))
        .SetNextState(new ActionStateConfirmUntilEncounter(this, 128, new ActionStateBattle(this, 100)))
        //.SetNextState(new ActionStateBattle(this, 100))
        .SetNextState(currentState)
      ;

      List<PixelObject> poList = PixelObjectList.GetPixelObjectList(0);
      WeaponStateNull nullState = new WeaponStateNull();
      foreach (PixelObject po in poList)
      {
        weaponDictionary[po.text] = nullState;
      }
    }

    public void LoadWeapons(Bitmap screenBmp)
    {
      List<PixelObject> poList = PixelObjectList.GetPixelObjectList(0);
      using (Bitmap weaponAreaBitmap = screenBmp.Clone(battleWeaponBounds, PixelFormat.Format24bppRgb))
      {
        int loadedCount = 0;
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
              loadedCount++;
            }
            else
            {
              break;
            }
          }
        }
        if (loadedCount == weaponDictionary.Count)
        {
          allWeaponsLoaded = true;
        }
      }
    }

    protected override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      if (allWeaponsLoaded)
      {
        foreach (WeaponState weapon in weaponDictionary.Values)
        {
          weapon.SetWeaponState(screenBmp, totalTime);
        }
      }
      else
      {
        LoadWeapons(screenBmp);
      }
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
      objectCounter.ProcessImage(currentThreshHoldBmp);
      Rectangle[] rects = objectCounter.GetObjectsRectangles();
      lineBoundsDictionary = GetBoundsDictionaryList(rects);

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

    public override void Draw(Bitmap b)
    {
      using (Graphics g = Graphics.FromImage(b))
      {
        if (lineBoundsDictionary != null)
        {
          lock(lineBoundsDictionary)
          {
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
          }
        }
        if (weaponDictionary != null)
        {
          foreach (WeaponState weapon in weaponDictionary.Values)
          {
            WeaponStateFlag weaponFlag = weapon.CurrentState;
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
        //currentState.DrawDebug(g);
      }
    }
  }
}
