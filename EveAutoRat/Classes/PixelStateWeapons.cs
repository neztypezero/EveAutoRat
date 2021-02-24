using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveAutoRat.Classes
{
  class PixelStateWeapons : PixelState
  {
    public Dictionary<string, WeaponState> weaponDictionary = new Dictionary<string, WeaponState>();
    private Dictionary<Rectangle, int> currentWeaponsBounds = new Dictionary<Rectangle, int>();

    public PixelStateWeapons(ActionThreadNewsRAT parent) : base(parent)
    {
      foreach (Rectangle rc in weaponBoundsList)
      {
        currentWeaponsBounds[rc] = 0;
      }
    }
    public void LoadWeapons(Bitmap screenBmp)
    {
      if (currentWeaponsBounds.Count > 0 && parent.InsideState == InsideFlag.Outside)
      {
        List<PixelObject> poList = PixelObjectList.GetPixelObjectList(0);

        Rectangle[] boundsList = currentWeaponsBounds.Keys.ToArray();

        foreach (Rectangle rc in boundsList)
        {
          using (Bitmap weaponAreaBitmap = screenBmp.Clone(rc, PixelFormat.Format24bppRgb))
          {
            foreach (PixelObject po in poList)
            {
              if (po.type == "weapon" && !weaponDictionary.ContainsKey(po.text + rc))
              {
                ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.94f);
                TemplateMatch[] matchings = tm.ProcessImage(weaponAreaBitmap, po.bmp);
                if (matchings.Length == 1)
                {
                  Rectangle r = matchings[0].Rectangle;
                  weaponDictionary[po.text + rc] = new WeaponState(rc, r, po);
                  currentWeaponsBounds.Remove(rc);
                }
              }
            }
          }
        }
        Rectangle[] keys = currentWeaponsBounds.Keys.ToArray();

        foreach (Rectangle rc in keys)
        {
          currentWeaponsBounds[rc]++;
          if (currentWeaponsBounds[rc] > 10)
          {
            currentWeaponsBounds.Remove(rc);
          }
        }
      }
    }

    public override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      LoadWeapons(screenBmp);
      foreach (WeaponState weapon in weaponDictionary.Values)
      {
        weapon.SetWeaponState(screenBmp, totalTime);
      }
    }

    public Dictionary<string, WeaponState> WeaponsState
    {
      get
      {
        return weaponDictionary;
      }
    }

    public override void Draw(Graphics g)
    {
        foreach (WeaponState weapon in weaponDictionary.Values)
        {
          WeaponStateFlag weaponFlag = weapon.CurrentState;
          if (weapon.diffFrame != null)
          {
            g.DrawImage(weapon.diffFrame, weapon.bounds.X, weapon.bounds.Y);
            objectCounter.ProcessImage(weapon.diffFrame);
            Rectangle[] rects = objectCounter.GetObjectsRectangles();
            Point cp = new Point(weapon.bounds.X + weapon.bounds.Width / 2, weapon.bounds.Y + weapon.bounds.Height / 2);
            foreach (Rectangle r in rects)
            {
              double radius = (double)weapon.bounds.Width / 2.0 - 3.0;
              double innerRadius = radius - 4.0;
              int x = weapon.bounds.X + r.X;
              int y = weapon.bounds.Y + r.Y;
              Point p1 = new Point(x, y);
              Point p2 = new Point(x + r.Width, y);
              Point p3 = new Point(x + r.Width, y + r.Height);
              Point p4 = new Point(x, y + r.Height);

              double d1 = Math.Sqrt((p1.X - cp.X) * (p1.X - cp.X) + (p1.Y - cp.Y) * (p1.Y - cp.Y));
              double d2 = Math.Sqrt((p2.X - cp.X) * (p2.X - cp.X) + (p2.Y - cp.Y) * (p2.Y - cp.Y));
              double d3 = Math.Sqrt((p3.X - cp.X) * (p3.X - cp.X) + (p3.Y - cp.Y) * (p3.Y - cp.Y));
              double d4 = Math.Sqrt((p4.X - cp.X) * (p4.X - cp.X) + (p4.Y - cp.Y) * (p4.Y - cp.Y));

              if (d1 < radius && d2 < radius && d3 < radius && d4 < radius)
              {
                if (d1 > innerRadius || d2 > innerRadius || d3 > innerRadius || d4 > innerRadius)
                {
                  g.DrawRectangle(Pens.Red, new Rectangle(x, y, r.Width, r.Height));
                }
              }
            }
          }
          if (weaponFlag == WeaponStateFlag.Active)
          {
            g.DrawEllipse(Pens.Red, weapon.bounds);
          }
          else if (weaponFlag == WeaponStateFlag.InActive)
          {
            g.DrawEllipse(Pens.Orange, weapon.bounds);
          }
          else
          {
            g.DrawEllipse(Pens.Yellow, weapon.bounds);
          }
        }
    }
  }
}
