using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EveAutoRat.Classes.ActionState
{
  class ActionStateNOP : ActionState
  {
    public ActionStateNOP(ActionThread parent, double delay) : base(parent, delay)
    {
    }
  }
}
