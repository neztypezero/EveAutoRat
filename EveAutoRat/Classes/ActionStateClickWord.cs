using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  class ActionStateClickWord : ActionState
  {
    public int threshHold;
    public string word;
    public bool loop;
    public bool looping;

    public ActionStateClickWord(ActionThreadNewsRAT parent, int threshHold, string word, double delay) : base(parent, delay)
    {
      this.threshHold = threshHold;
      this.word = word;
    }

    public ActionStateClickWord(ActionThreadNewsRAT parent, int threshHold, string word, double delay, bool loop) : this(parent, threshHold, word, delay)
    {
      this.loop = loop;
      looping = false;
    }

    public override int GetThreshHold()
    {
      return threshHold;
    }
    public override ActionState Run(double totalTime)
    {
      Bitmap bmp = parent.threshHoldDictionary[threshHold];
      foreach (Rectangle r in parent.lineBoundsDictionary.Keys)
      {
        if (r.X > 15 && r.Y > 25 && r.Width > 15 && r.Height > 15)
        {
          Rectangle exRect = new Rectangle(r.X - 4, r.Y - 4, r.Width + 8, r.Height + 8);
          if ((exRect.X + exRect.Width) >= bmp.Width || (exRect.Y + exRect.Height) >= bmp.Height || exRect.X < 0 || exRect.Y < 0)
          {
            exRect = r;
          }
          using (Bitmap b = bmp.Clone(exRect, PixelFormat.Format24bppRgb))
          {
            invertFilter.ApplyInPlace(b);
            string foundWord = OCR.GetText(b, new Rectangle(0, 0, b.Width, b.Height)).Trim();
            if (word == foundWord)
            {
              Point center = parent.GetClickPoint(r);
              Win32.SendMouseClick(parent.GetEventHWnd(), center.X, center.Y);
              if (loop)
              {
                looping = true;
                return this;
              }
              return nextState;
            }
          }
        }

      }
      if (looping)
      {
        looping = false;
        return nextState;
      }
      return this;
    }
  }
}
