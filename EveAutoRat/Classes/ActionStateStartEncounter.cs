using System.Drawing;

namespace EveAutoRat.Classes
{
  public class ActionStateStartEncounter : ActionState
  {
    private bool dialogOpened = false;

    public ActionStateStartEncounter(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public override ActionState Run(double totalTime)
    {
      Rectangle found;
      Bitmap bmp0 = threshHoldDictionary[0];
      Bitmap bmp128 = threshHoldDictionary[128];

      float encounterTile = FindIconSimilarity(bmp0, "encounter_tile", encounterTileBounds, 0);
      if (encounterTile > 0.94f)
      {
        lastClick = parent.GetClickPoint(encounterTileBounds);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        return this;
      }
      float encounterNews = FindIconSimilarity(bmp0, "encounter_news", encounterNewsBounds, 0);
      if (encounterNews > 0.94f)
      {
        float encounterBattleIconJournal = FindIconSimilarity(bmp0, "encounter_battle_icon_journal", encounterBattleIconJournalBounds, 0);
        if (encounterBattleIconJournal > 0.90f)
        {
          lastClick = parent.GetClickPoint(encounterBattleIconJournalBounds);
        }
        else
        {
          lastClick = parent.GetClickPoint(encounterNewsBounds);
        }
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        return this;
      }
      float encounterRefreshIcon = FindIconSimilarity(bmp0, "encounter_refresh", encounterRefreshBounds, 0);
      if (encounterRefreshIcon > 0.94f)
      {
        lastClick = parent.GetClickPoint(encounterRefreshBounds);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        nextDelay = 4000;
        return this;
      }
      float encounterBattleIcon = FindIconSimilarity(bmp0, "encounter_battle_icon", encounterBattleIconBounds, 0);
      if (encounterBattleIcon > 0.94f)
      {
        lastClick = parent.GetClickPoint(encounterBattleIconBounds);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        return this;
      }
      found = FindWord("Accept", encounterAcceptBounds);
      if (found.X > -1)
      {
        lastClick = parent.GetClickPoint(found);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        return this;
      }
      if (FindSingleWord(bmp128, encounterBeginBounds) == "Begin")
      {
        lastClick = parent.GetClickPoint(encounterBeginBounds);
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
        return this;
      }
      if (FindSingleWord(bmp128, confirmBounds) == "Confirm")
      {
        return NextState;
      }
      if (!dialogOpened)
      {
        dialogOpened = true;
        lastClick = encounterOpenDialogPoint;
        Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
      }

      return this;
    }

    public override void Reset()
    {
      dialogOpened = false;
    }
  }
}