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

    public List<Rectangle> GetEnemyBounds()
    {
      Bitmap screenBmp = parent.GetThreshHoldBitmap(0);
      Bitmap iconColumnEnemies = screenBmp.Clone(battleIconBounds, PixelFormat.Format24bppRgb);
      iconColumnEnemies = battleIconRedFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconGrayFilter.Apply(iconColumnEnemies);
      iconColumnEnemies = battleIconThresholdFilter.Apply(iconColumnEnemies);

      objectCounter.ProcessImage(iconColumnEnemies);
      iconColumnEnemies.Dispose();
      Rectangle[] rArray = objectCounter.GetObjectsRectangles();

      List<Rectangle> rList = new List<Rectangle>();
      foreach (Rectangle r in rArray)
      {
        if (r.Height < 3)
        {
          int n = rList.Count;
          if (n > 0)
          {
            rList[n - 1].Intersect(r);
          }
        } 
        else if (r.Height > 9)
        {
          rList.Add(r);
        }
      }

      return rList;
    }

    public List<Rectangle> GetSmallEnemyBoundList(List<Rectangle> enemyList)
    {
      List<Rectangle> rList = new List<Rectangle>();
      foreach (Rectangle r in enemyList)
      {
        if (r.Height > 9 && r.Height < 13)
        {
          rList.Add(r);
        }
      }
      return rList;
    }

    public int GetTargetEnemyCount()
    {
      Bitmap bmp112 = parent.GetThreshHoldBitmap(112);
      using (Bitmap iconColumnEnemies = bmp112.Clone(battleIconBounds, PixelFormat.Format24bppRgb))
      {
        Rectangle r = new Rectangle(0, 0, iconColumnEnemies.Width, iconColumnEnemies.Height);
        return FindIconSimilarityCount(iconColumnEnemies, "enemy_targeted", r, 112, 0.9f);
      }
    }

    public List<Rectangle> GetTargetEnemyList()
    {
      Bitmap bmp112 = parent.GetThreshHoldBitmap(112);
      using (Bitmap iconColumnEnemies = bmp112.Clone(battleIconBounds, PixelFormat.Format24bppRgb))
      {
        Rectangle r = new Rectangle(0, 0, iconColumnEnemies.Width, iconColumnEnemies.Height);
        return FindIconSimilarityList(iconColumnEnemies, "enemy_targeted", r, 112, 0.9f);
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

    public override void Draw(Graphics g)
    {
      Brush lastClickColor = new SolidBrush(Color.FromArgb(150, 250, 50, 200));
      g.FillEllipse(lastClickColor, new Rectangle(lastClick.X - 15, lastClick.Y - 15, 30, 30));
    }
  }
}
