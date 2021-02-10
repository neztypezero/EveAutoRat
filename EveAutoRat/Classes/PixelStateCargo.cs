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
  public class PixelStateCargo : PixelState
  {
    protected double holdAmount = -1;

    public PixelStateCargo(ActionThreadNewsRAT parent) : base(parent)
    {
    }

    public override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      using (Bitmap cargoBmp = screenBmp.Clone(cargoHoldBounds, PixelFormat.Format24bppRgb))
      {
        objectCounter.ProcessImage(cargoBmp);
        Rectangle[] rects = objectCounter.GetObjectsRectangles();
        if (rects.Length == 1)
        {
          holdAmount = (double)rects[0].Width/(double)cargoHoldBounds.Width;
        }
        else
        {
          holdAmount = 0;
        }
      }
    }

    public double CurrentHoldAmount
    {
      get
      {
        return holdAmount;
      }
    }
  }
}
