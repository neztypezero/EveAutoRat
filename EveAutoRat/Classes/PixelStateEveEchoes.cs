using System;
using System.Drawing;

namespace EveAutoRat.Classes
{
  class PixelStateEveEchoes : PixelState
  {
    private bool iconFound = false;

    public PixelStateEveEchoes(ActionThreadNewsRAT parent) : base(parent)
    {
    }

    public override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      Rectangle r = FindIcon(eveEchoesIconBounds, "eve_app", 0);
      if (r.X > -1)
      {
        iconFound = true;
        return;
      }
      r = FindIcon(eveEchoesIconBounds, "eve_app_overlayed", 0);
      if (r.X > -1)
      {
        iconFound = true;
        return;
      }
      iconFound = false;
    }

    public bool IconFound
    {
      get
      {
        return iconFound;
      }
    }
  }
}
