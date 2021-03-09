using System.Drawing;

namespace EveAutoRat.Classes
{
  class ActionStateNOP : ActionState
  {
    public ActionStateNOP(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
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
      foreach (EnemyInfo enemy in parent.EnemyList)
      {
        textPoint.Y += 20;
        g.DrawString(enemy.type + " " + enemy.isTargeted, infoFont, textColor, textPoint);
      }
    }
  }
}
