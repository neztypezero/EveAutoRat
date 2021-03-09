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
    public EnemyInfo[] GetEnemyList()
    {
      return parent.EnemyList;
    }

    public EnemyInfo[] GetSmallEnemyList(EnemyInfo[] enemyList)
    {
      List<EnemyInfo> eList = new List<EnemyInfo>();
      foreach (EnemyInfo e in enemyList)
      {
        if (e.type <= EnemyTypes.Destroyer)
        {
          eList.Add(e);
        }
      }
      return eList.ToArray();
    }

    public List<Rectangle> GetLargeEnemyBoundList(List<Rectangle> enemyList)
    {
      List<Rectangle> rList = new List<Rectangle>();
      foreach (Rectangle r in enemyList)
      {
        if (r.Height > 50)
        {
          rList.Add(r);
        }
      }
      return rList;
    }

    public int GetTargetEnemyCount(EnemyInfo[] enemyList)
    {
      int count = 0;
      foreach (EnemyInfo e in enemyList)
      {
        if (e.isTargeted)
        {
          count++;
        }
      }
      return count;
    }

    public List<Rectangle> GetTargetEnemyList()
    {
      Bitmap bmp112 = parent.GetThreshHoldBitmap(112);
      using (Bitmap iconColumnEnemies = bmp112.Clone(battleIconBounds, PixelFormat.Format24bppRgb))
      {
        //objectCounter.ProcessImage(iconColumnEnemies);
        //Rectangle[] rArray = objectCounter.GetObjectsRectangles();
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
