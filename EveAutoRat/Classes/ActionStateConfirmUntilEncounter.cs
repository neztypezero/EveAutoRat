using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public class ActionStateConfirmUntilEncounter : ActionState
  {
    private string wordSearch = null;
    private Rectangle ZeroBounds = new Rectangle(0, 0, 0, 0);
    private Rectangle wordSearchBounds;

    public ActionStateConfirmUntilEncounter(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
      wordSearchBounds = ZeroBounds;
    }

    public override void Reset()
    {
      wordSearch = null;
      wordSearchBounds = ZeroBounds;
    }

    public override ActionState Run(double totalTime)
    {
      Rectangle[] enemyBoundsList = GetEnemyBounds();
      Bitmap bmp128 = parent.threshHoldDictionary[128];

      if (wordSearch != null)
      {
        Rectangle found = FindWord(wordSearch, wordSearchBounds);
        if (found.X != -1)
        {
          lastClick = parent.GetClickPoint(found);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          nextDelay = 1500;
          wordSearch = null;
          wordSearchBounds = ZeroBounds;
          return this;
        }
      }

      string confirmText = FindSingleWord(threshHoldDictionary[128], confirmBounds);
      if (confirmText == "Confirm")
      {
        lastClick = parent.GetClickPoint(confirmBounds);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        nextDelay = 1000;
        return this;
      }

      foreach (WeaponState weapon in parent.WeaponsState.Values)
      {
        if (weapon.name != null && weapon.name.StartsWith("pulse-laser"))
        {
          if (weapon.CurrentState == WeaponStateFlag.InActive)
          {
            float eyeClosed = FindIconSimilarity(bmp128, "eye", eyeClosedBounds, 128);
            if (eyeClosed > 0.90f)
            {
              lastClick = parent.GetClickPoint(eyeClosedBounds);
              Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
              nextDelay = 1000;
              return this;
            }
            float eyeOpen = FindIconSimilarity(bmp128, "eye", eyeOpenBounds, 128);
            if (eyeOpen > 0.9f)
            {
              string filterTitle = FindSingleWord(threshHoldDictionary[64], filterTitleBounds);
              if (filterTitle != "Pirates")
              {
                lastClick = parent.GetClickPoint(filterTitleBounds);
                Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
                nextDelay = 1000;
                wordSearch = "Pirates";
                wordSearchBounds = filterListBounds;
                return this;
              }
            }
          }
          break;
        }
      }

      if (enemyBoundsList != null && enemyBoundsList.Length > 0)
      {
        if (FindIconSimilarity(bmp128, "target_all", targetAllBounds, 128) > 0.9f)
        {
          return NextState;
        }
      }

      return this;
    }
  }
}
