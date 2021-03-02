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
    private double lootingTimeout = 0;
    private bool menuPressed = false;

    public ActionStateLoot(ActionThreadNewsRAT parent, double delay) : base(parent, delay)
    {
    }

    public Rectangle GetFirstLootBounds()
    {
      using (Bitmap iconColumnLoot = parent.GetThreshHoldBitmap(112).Clone(battleIconBounds, PixelFormat.Format24bppRgb))
      {
        objectCounter.ProcessImage(iconColumnLoot);
        iconColumnLoot.Save("ObjBitmap\\loot.bmp");
      }
      Rectangle[] lootRectList = objectCounter.GetObjectsRectangles();
      foreach (Rectangle r in lootRectList)
      {
        if (r.Width > 7 && r.Height > 7)
        {
          return new Rectangle(r.X + battleIconBounds.X, r.Y + battleIconBounds.Y, r.Width, r.Height);
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
      lootingTimeout = 0;
    }

    public override ActionState Run(double totalTime)
    {
      Bitmap bmp64 = parent.GetThreshHoldBitmap(64);

      if (wordSearch != null)
      {
        FoundWord found = FindWord(wordSearch, wordSearchBounds);
        if (found != null)
        {
          if (looting == 1)
          {
            lootingTimeout = totalTime + 25000;
            looting = 2;
            wordSearch = "Loot";
            wordSearchBounds = lootAllBounds;
          }
          else if (looting == 2)
          {
            looting = 0;
            wordSearch = null;
            wordSearchBounds = NullBounds;
            nextDelay = 1000;
          }
          else
          {
            looting = 0;
            wordSearch = null;
            wordSearchBounds = NullBounds;
          }
          lastClick = parent.GetClickPoint(found.r);
          Thread.Sleep(500);
          Win32.SendMouseClick(eventHWnd, lastClick.X, lastClick.Y);
          return this;
        }
        if (wordSearch == "Loot")
        {
          if (looting != 0 && totalTime > lootingTimeout)
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
          lootingTimeout = totalTime + 2500;
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
