using System;
using System.Drawing;

namespace EveAutoRat.Classes
{
  public class ActionStateStartUp : ActionState
  {
    bool running = false;

    public ActionStateStartUp(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override void Reset()
    {
      running = false;
    }

    public override ActionState Run(double totalTime)
    {
      if (!running)
      {
        if (parent.EveEchosIconFound)
        {
          lastClick = parent.GetClickPoint(eveEchoesIconBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1500;

          running = true;
          return this;
        }
        else
        {
          return NextState;
        }
      }
      else
      {
        Bitmap bmp64 = parent.GetThreshHoldBitmap(64);
        Bitmap bmp112 = parent.GetThreshHoldBitmap(112);
        Bitmap bmp128 = parent.GetThreshHoldBitmap(128);

        if (parent.EveEchosIconFound)
        {
          lastClick = parent.GetClickPoint(eveEchoesIconBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1500;
        }
        else if (FindIconSimilarity(bmp128, "eve_start", startEveEchoesBounds, 128) > 0.95)
        {
          lastClick = parent.GetClickPoint(startEveEchoesBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1500;
        }
        else if (FindIconSimilarity(bmp112, "close_dialog_button", gameAnnouncementBounds, 112) > 0.95)
        {
          lastClick = parent.GetClickPoint(gameAnnouncementBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1500;
        }
        else
        {
          string playerName = FindSingleWord(bmp64, playerNameBounds);
          if (playerName.Contains("Addison") && playerName.Contains("Zen"))
          {
            lastClick = parent.GetClickPoint(playerNameBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y+50);
            running = false;
            return NextState;
          }
        }
        return this;
      }
    }
  }
}
