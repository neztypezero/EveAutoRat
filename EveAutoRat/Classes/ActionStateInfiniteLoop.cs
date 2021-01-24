namespace EveAutoRat.Classes
{
  class ActionStateInfiniteLoop : ActionState
  {
    public ActionStateInfiniteLoop(ActionThread parent, double delay) : base(parent, delay)
    {
    }

    public override ActionState Run(double totalTime)
    {
      return this;
    }
  }
}
