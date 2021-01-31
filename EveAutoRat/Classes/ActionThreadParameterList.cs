using System.Drawing;

namespace EveAutoRat.Classes
{
  public class ActionThreadParameterList
  {
    public double time;
    public Bitmap screenBitmap;
    public double nextCallDelay;

    public ActionThreadParameterList() {
      time = 0;
      screenBitmap = null;
      nextCallDelay = 0;
    }
  }

}
