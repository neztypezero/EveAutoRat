using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace EveAutoRat.Classes
{
  class ActionStateLoot : ActionState
  {
    private string wordSearch = null;
    private Rectangle NullBounds = new Rectangle(0, 0, -1, -1);
    private Rectangle wordSearchBounds;
    private int looting = 0;
    private double lootingTime = 0;
    private bool menuPressed = false;

    public ActionStateLoot(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public Rectangle GetFirstLootBounds()
    {
      using (Bitmap iconColumnLoot = threshHoldDictionary[64].Clone(firstBattleIconBounds, PixelFormat.Format24bppRgb))
      {
        objectCounter.ProcessImage(iconColumnLoot);
      }
      Rectangle[] lootRectList = objectCounter.GetObjectsRectangles();
      foreach (Rectangle r in lootRectList)
      {
        if (r.Width > 12)
        {
          return new Rectangle(r.X + firstBattleIconBounds.X, r.Y + firstBattleIconBounds.Y, r.Width, r.Height);
        }
      }
      return NullRect;
    }

    public override void Reset()
    {
      looting = 0;
      wordSearch = null;
      wordSearchBounds = NullBounds;
      menuPressed = false;
      lootingTime = 0;
    }

    public override ActionState Run(double totalTime)
    {
      Bitmap bmp64 = threshHoldDictionary[64];

      if (wordSearch != null)
      {
        Rectangle found = FindWord(wordSearch, wordSearchBounds);
        if (found.X != -1)
        {
          if (looting == 1)
          {
            lootingTime = totalTime + 25000;
            looting = 2;
            wordSearch = "Loot";
            wordSearchBounds = lootAllBounds;
          }
          else if (looting == 2)
          {
            looting = 0;
            wordSearch = null;
            wordSearchBounds = NullBounds;
            nextDelay = 3000;
            Thread.Sleep(500);
          }
          else
          {
            looting = 0;
            wordSearch = null;
            wordSearchBounds = NullBounds;
          }
          lastClick = parent.GetClickPoint(found);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          return this;
        }
        if (wordSearch == "Loot")
        {
          if (looting != 0 && totalTime > lootingTime)
          {
            looting = 0;
            wordSearch = null;
          }
          return this;
        }
      }

      if (!menuPressed)
      {
        menuPressed = true;
        string filterTitle = FindSingleWord(bmp64, filterTitleBounds);
        if (filterTitle != "Loot")
        {
          lastClick = parent.GetClickPoint(filterTitleBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          wordSearch = "Loot";
          wordSearchBounds = filterListBounds;
          return this;
        }
      }
      if (looting == 0)
      {
        Rectangle lootBounds = GetFirstLootBounds();
        if (lootBounds.X > -1)
        {
          string noSearchResults = FindSingleWord(bmp64, noSearchResultsBounds);
          if (noSearchResults.Contains("NO") && noSearchResults.Contains("SEARCH") && noSearchResults.Contains("RESULTS"))
          {
            return NextState;
          }
          lastClick = parent.GetClickPoint(lootBounds);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          wordSearch = "Loot";
          wordSearchBounds = popupBounds;
          looting = 1;
          lootingTime = totalTime + 2500;
          return this;
        }
        else
        {
          return NextState;
        }
      }
      return this;
    }
  }
}
