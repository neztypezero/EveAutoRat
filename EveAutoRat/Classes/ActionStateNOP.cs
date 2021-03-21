using System;
using System.Collections.Generic;
using System.Drawing;

namespace EveAutoRat.Classes
{
  class ActionStateNOP : ActionState
  {
    private EnemyInfo[] currentEnemies = null;

    public ActionStateNOP(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override ActionState Run(double totalTime)
    {
      currentEnemies = GetEnemyList();

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
      return NextState;
    }

    public override void Draw(Graphics g)
    {
      base.Draw(g);

      WeaponState pulseLaser = GetWeapon();

      Brush stateBg = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
      Brush textColor = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
      g.FillRectangle(stateBg, infoBounds);
      Point textPoint = new Point(infoBounds.X + 20, infoBounds.Y + 20);

      foreach (WeaponState weapon in parent.WeaponsState.Values)
      {
        textPoint.Y += 20;
        g.DrawString(weapon.name + " " + weapon.CurrentState, infoFont, textColor, textPoint);
      }
      textPoint.Y += 20;
      if (currentEnemies != null)
      {
        foreach (EnemyInfo enemy in currentEnemies)
        {
          textPoint.Y += 20;
          g.DrawString(enemy.type + ": " + enemy.isTargeted + " " + enemy.distance, infoFont, textColor, textPoint);
          foreach (Rectangle r in enemy.distanceBounds.Values)
          {
            g.DrawRectangle(Pens.Red, r);
          }
        }
      }
    }
  }
}
