using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public enum InsideFlag : int
  {
    Unknown = -1,
    Outside = 0,
    Inside = 1,
  }
  class PixelStateInStation : PixelState
  {
    private InsideFlag currentState;

    public PixelStateInStation(ActionThreadNewsRAT parent) : base(parent)
    {
      currentState = InsideFlag.Unknown;
    }

    public override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      float inStation = FindIconSimilarity(screenBmp, "in_station_area", inStationCheckBounds, 0);
      if (inStation >= 0.99f)
      {
        currentState = InsideFlag.Inside;
        return;
      }
      else
      {
        float eyeClosed = FindIconSimilarity(screenBmp, "eye_closed", eyeClosedZeroBounds, 0);
        float eyeOpen = FindIconSimilarity(screenBmp, "eye_open", eyeOpenZeroBounds, 0);

        if (eyeClosed > 0.90f)
        {
          currentState = InsideFlag.Outside;
          return;
        }
        if (eyeOpen > 0.82f)
        {
          currentState = InsideFlag.Outside;
          return;
        }
      }
      currentState = InsideFlag.Unknown;
    }

    public InsideFlag CurrentState
    {
      get
      {
        return currentState;
      }
    }
  }
}
