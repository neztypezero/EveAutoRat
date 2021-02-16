using System;
using System.Drawing;

namespace EveAutoRat.Classes
{
  public enum UnloadCargoStateFlag : int
  {
    Unknown = -1,
    DestinationMenu = 0,
    DestinationAuto = 1,
    DestinationFlying = 2,
    DestinationSelectShip = 3,
    Unloading = 4,
    CloseDialog = 5,
  }

  public class ActionStateUnloadCargo : ActionState
  {
    private UnloadCargoStateFlag currentDestinationState = UnloadCargoStateFlag.Unknown;

    public ActionStateUnloadCargo(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override ActionState Run(double totalTime)
    {
      Bitmap bmp0 = threshHoldDictionary[0];
      Bitmap bmp64 = threshHoldDictionary[64];
      Bitmap bmp80 = threshHoldDictionary[80];
      Bitmap bmp96 = threshHoldDictionary[96];
      Bitmap bmp128 = threshHoldDictionary[128];

      if (parent.CurrentHoldAmount > 0.85 || (parent.InsideState == InsideFlag.Inside && parent.CurrentHoldAmount > 0.10) || currentDestinationState != UnloadCargoStateFlag.Unknown)
      {
        if (currentDestinationState == UnloadCargoStateFlag.Unknown)
        {
          
          float destinationClosed = FindIconSimilarity(bmp80, "destination", destinationClosedBounds, 80);
          if (destinationClosed > 0.9f)
          {
            lastClick = parent.GetClickPoint(destinationClosedBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
          float destinationOpen = FindIconSimilarity(bmp80, "destination", destinationOpenBounds, 80);
          if (destinationOpen > 0.9f)
          {
            if (parent.InsideState == InsideFlag.Inside)
            {
              string stationName = FindSingleWord(bmp64, stationNameBounds);
              string jumpToName = FindSingleWord(bmp64, jumpToBounds);
              int n = GetStringSimilarity(stationName, jumpToName);
              if (n < 2 && parent.CurrentHoldAmount > 0.75)
              {
                currentDestinationState = UnloadCargoStateFlag.DestinationFlying;
                return this;
              } 
              else
              {
                return NextState;
              }
            }
            float destinationPin = FindIconSimilarity(bmp80, "destination_pin", destinationPinBounds, 80);
            if (destinationPin > 0.9f)
            {
              currentDestinationState = UnloadCargoStateFlag.DestinationMenu;
              lastClick = parent.GetClickPoint(destinationPinBounds);
              Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
              nextDelay = 2000;
              return this;
            }
            else
            {
              float destinationSet = FindIconSimilarity(bmp0, "destination_set", destinationSetBounds, 0);
              if (destinationSet > 0.9f)
              {
                lastClick = parent.GetClickPoint(destinationOpenBounds);
                Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
              }
              else
              {
                lastClick = parent.GetClickPoint(destinationPinBounds);
                lastClick.Y -= 100;
                Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
                return this;
              }
            }
          }
        }
        else if (currentDestinationState == UnloadCargoStateFlag.DestinationMenu)
        {
          currentDestinationState = UnloadCargoStateFlag.DestinationAuto;
          Rectangle found = FindWord("Destination", destinationSetAsBounds);
          if (found.X > -1)
          {
            lastClick = parent.GetClickPoint(found);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
        }
        else if (currentDestinationState == UnloadCargoStateFlag.DestinationAuto)
        {
          float destinationSet = FindIconSimilarity(bmp0, "destination_set", destinationSetBounds, 0);
          if (destinationSet > 0.9f)
          {
            float destinationAuto = FindIconSimilarity(bmp96, "destination_auto", destinationAutoBounds, 96);
            if (destinationAuto > 0.9f)
            {
              currentDestinationState = UnloadCargoStateFlag.DestinationFlying;
              lastClick = parent.GetClickPoint(destinationAutoBounds);
              Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
              return this;
            }
          }
        } 
        else if (currentDestinationState == UnloadCargoStateFlag.DestinationFlying)
        {
          if (parent.InsideState == InsideFlag.Inside)
          {
            currentDestinationState = UnloadCargoStateFlag.DestinationSelectShip;
            lastClick = parent.GetClickPoint(PixelStateCargo.cargoHoldBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y - 15);
            return this;
          }
          else
          {
            if (FindSingleWord(bmp128, confirmBounds) == "Confirm")
            {
              lastClick = parent.GetClickPoint(confirmBounds);
              Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
              nextDelay = 10000;
              return this;
            }
          }
        }
        else if (currentDestinationState == UnloadCargoStateFlag.DestinationSelectShip)
        {
          currentDestinationState = UnloadCargoStateFlag.Unloading;
          lastClick = parent.GetClickPoint(activeShipBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          return this;
        }
        else if (currentDestinationState == UnloadCargoStateFlag.Unloading)
        {
          float unloadAll = FindIconSimilarity(bmp0, "unload_all", unloadSelectAllBounds, 0);
          if (unloadAll > 0.94)
          {
            lastClick = parent.GetClickPoint(unloadSelectAllBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
          float moveTo = FindIconSimilarity(bmp0, "unload_move", unloadMoveToBounds, 0);
          if (moveTo > 0.94)
          {
            lastClick = parent.GetClickPoint(unloadMoveToBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
          float itemHanger = FindIconSimilarity(bmp0, "unload_item_hanger", unloadMoveToItemHangerBounds, 0);
          if (itemHanger > 0.94)
          {
            currentDestinationState = UnloadCargoStateFlag.CloseDialog;
            lastClick = parent.GetClickPoint(unloadMoveToItemHangerBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return this;
          }
        }
        else if (currentDestinationState == UnloadCargoStateFlag.CloseDialog)
        {
          float closeDialog = FindIconSimilarity(bmp0, "close_dialog", closeDialogBounds, 0);
          if (closeDialog > 0.95)
          {
            lastClick = parent.GetClickPoint(closeDialogBounds);
            Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
            return NextState;
          }
        }
      }
      else
      {
        return NextState;
      }
      return this;
    }

    public override void Reset()
    {
      currentDestinationState = UnloadCargoStateFlag.Unknown;
    }
  }
}