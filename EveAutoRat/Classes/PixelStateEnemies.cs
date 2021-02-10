using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  class PixelStateEnemies : PixelState
  {
    public PixelStateEnemies(ActionThreadNewsRAT parent) : base(parent)
    {
    }

    public override void StepEvery(Bitmap screenBmp, double totalTime)
    {
    }

    public int EnemyCount
    {
      get
      {
        return 0;
      }
    }

    public int EnemyTargetedCount
    {
      get
      {
        return 0;
      }
    }
  }
}
