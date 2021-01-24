using System.Drawing;

namespace EveAutoRat.Classes
{
  class ActionStateClickWordAt : ActionState
  {
    public int threshHold;
    public string word;
    public double timeout = -1;
    public double startingTime = -1;

    public ActionStateClickWordAt(ActionThreadNewsRAT parent, int threshHold, string word, double delay, double timeout) : base(parent, delay)
    {
      this.threshHold = threshHold;
      this.word = word;
      this.timeout = timeout;
    }

    public override int GetThreshHold()
    {
      return threshHold;
    }

    public override ActionState Run(double totalTime)
    {
      if (startingTime < 0)
      {
        startingTime = totalTime;
      }
      if (totalTime > startingTime + timeout)
      {
        return nextState;
      }
      string foundWord = FindSingleWord(threshHoldDictionary[128], confirmBounds);
      if (foundWord == word)
      {
        Point center = parent.GetClickPoint(confirmBounds);
        Win32.SendMouseClick(eventHWnd, center.X, center.Y);
        nextDelay = 2000;
        return this;
      }

      return this;
    }
  }
}
