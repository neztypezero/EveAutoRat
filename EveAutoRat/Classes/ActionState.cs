using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public class ActionState : PixelState
  {
    private ActionState nextState = null;
    protected double delay;
    protected double nextDelay;

    protected Point lastClick = new Point(-1, -1);

    public ActionState(ActionThreadNewsRAT parent, double delay) : base(parent)
    {
      this.delay = delay;
      this.nextDelay = delay;
    }

    public Rectangle[] GetEnemyBounds()
    {
      Bitmap screenBmp = threshHoldDictionary[0];
      Bitmap iconColumnEnemies = screenBmp.Clone(battleIconBounds, PixelFormat.Format24bppRgb);
      iconColumnEnemies = battleIconRedFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconGrayFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconThresholdFilter.Apply(iconColumnEnemies);

      objectCounter.ProcessImage(iconColumnEnemies);
      iconColumnEnemies.Dispose();
      Rectangle[] rList = objectCounter.GetObjectsRectangles();

      return rList;
    }

    public Rectangle GetSmallestEnemyBounds(Rectangle[] enemyList)
    {
      foreach (Rectangle r in enemyList)
      {
        if (r.Height > 9 && r.Height < 13)
        {
          return r;
        }
      }
      return enemyList[0];
    }

    public int GetTargetEnemyCount()
    {
      Bitmap bmp112 = threshHoldDictionary[112];
      using (Bitmap iconColumnEnemies = bmp112.Clone(battleIconBounds, PixelFormat.Format24bppRgb))
      {
        Rectangle r = new Rectangle(0,0,iconColumnEnemies.Width,iconColumnEnemies.Height);
        return FindIconSimilarityCount(iconColumnEnemies, "enemy_targeted", r, 112, 0.9f);
      }
    }

    public WeaponState GetWeapon()
    {
      Dictionary<string, WeaponState> weaponsState = parent.WeaponsState;
      foreach (WeaponState weapon in weaponsState.Values)
      {
        if (weapon.name.StartsWith("pulse-laser"))
        {
          return weapon;
        }
      }
      return null;
    }

    public virtual ActionState NextState
    {
      get
      {
        Reset();
        nextState.Reset();
        return nextState;
      }
    }

    public virtual void Reset()
    {
    }

    public virtual ActionState Run(double totalTime)
    {
      return NextState;
    }

    public virtual double GetDelay()
    {
      if (nextDelay != delay)
      {
        double d = nextDelay;
        nextDelay = delay;
        return d;
      }
      return delay;
    }

    public virtual int GetThreshHold()
    {
      return 64;
    }

    public virtual ActionState SetNextState(ActionState nextState)
    {
      this.nextState = nextState;
      return NextState;
    }

    public virtual void DrawDebug(Graphics g)
    {
      return;
    }
  }
}
