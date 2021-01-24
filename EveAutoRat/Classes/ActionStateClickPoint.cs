using System.Drawing;

namespace EveAutoRat.Classes
{
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
}
