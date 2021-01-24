using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  class ActionStateConfirmUntilEncounter : ActionState
  {
    private ActionThreadNewsRAT ratParent;
    private ActionStateBattle battleState;
    private string wordSearch = null;
    private Rectangle ZeroBounds = new Rectangle(0, 0, 0, 0);
    private Rectangle wordSearchBounds;

    public ActionStateConfirmUntilEncounter(ActionThreadNewsRAT parent, double delay, ActionStateBattle battleState) : base(parent, delay)
    {
      this.ratParent = parent;
      this.battleState = battleState;
      SetNextState(battleState);
      wordSearchBounds = ZeroBounds;
    }

    public override ActionState Run(double totalTime)
    {
      Rectangle[] enemyBoundsList = battleState.GetEnemyBounds();
      Bitmap bmp = parent.threshHoldDictionary[128];

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
        Point center = parent.GetClickPoint(confirmBounds);
        Win32.SendMouseClick(eventHWnd, center.X, center.Y);
        nextDelay = 2000;
        return this;
      }

      foreach (WeaponState weapon in ratParent.weaponDictionary.Values)
      {
        if (weapon.name != null && weapon.name.StartsWith("pulse-laser"))
        {
          if (weapon.IsActive(parent.threshHoldDictionary[0]) == WeaponStateFlag.InActive)
          {
            using (Bitmap eyeBmp = bmp.Clone(eyeBounds, PixelFormat.Format24bppRgb))
            {
              Rectangle poBounds = PixelObjectList.FindBitmap(eyeBmp, "eye", 128);
              if (poBounds.X > eyeBounds.Width / 2)
              {
                Point center = parent.GetClickPoint(new Rectangle(eyeBounds.X + poBounds.X, eyeBounds.Y + poBounds.Y, poBounds.Width, poBounds.Height));
                Win32.SendMouseClick(eventHWnd, center.X, center.Y);
                nextDelay = 1000;
                return this;
              }
            }

            string filterTitle = FindSingleWord(threshHoldDictionary[64], filterTitleBounds);
            if (filterTitle != "PiratesLoot")
            {
              Point center = parent.GetClickPoint(filterTitleBounds);
              Win32.SendMouseClick(eventHWnd, center.X, center.Y);
              nextDelay = 1000;
              wordSearch = "PiratesLoot";
              wordSearchBounds = filterListBounds;
              return this;
            }
          }
          break;
        }
      }

      if (enemyBoundsList != null && enemyBoundsList.Length > 0)
      {
        nextDelay = 12000;
        return battleState;
      }

      return this;
    }

    public override ActionState SetNextState(ActionState nextState)
    {
      if (battleState != null)
      {
        return battleState.SetNextState(nextState);
      }
      return base.SetNextState(nextState);
    }
  }
}
