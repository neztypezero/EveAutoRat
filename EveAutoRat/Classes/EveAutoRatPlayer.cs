using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using AForge.Imaging.Filters;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace EveAutoRat.Classes
{
  class EveAutoRatPlayer
  {
    private IntPtr emuHWnd = IntPtr.Zero;
    private IntPtr eventHWnd = IntPtr.Zero;
    private ActionThread currentAction = null;
    private EveAutoRatMainForm parentForm = null;

    public EveAutoRatPlayer(EveAutoRatMainForm parentForm)
    {
      this.parentForm = parentForm;
    }

    public void startPlayer()
    {
      List<IntPtr> hWndList = Win32.FindWindowList("LDPlayer");
      if (hWndList.Count == 1)
      {
        emuHWnd = hWndList[0];
        List<IntPtr> childHWnd = Win32.GetAllChildHandles(emuHWnd);
        eventHWnd = childHWnd[0];
      }
    }

    public void startRatProgram()
    {
      if (currentAction == null)
      {
        currentAction = new ActionThreadNewsRAT(parentForm, emuHWnd, eventHWnd);
        currentAction.Start();
      }
      else
      {
        currentAction.Stop();
        currentAction = null;
      }
    }

    public void startLearnProgram()
    {
      if (currentAction == null)
      {
        currentAction = new ActionThreadLearn(parentForm, emuHWnd, eventHWnd);
        currentAction.Start();
      }
      else
      {
        currentAction.Stop();
        currentAction = null;
      }
    }

    public void MouseDown(MouseEventArgs e)
    {
      if (currentAction != null)
      {
        currentAction.MouseDown(e);
      }
    }

    public void MouseMove(MouseEventArgs e)
    {
      if (currentAction != null)
      {
        currentAction.MouseMove(e);
      }
    }

    public void MouseUp(MouseEventArgs e)
    {
      if (currentAction != null)
      {
        currentAction.MouseUp(e);
      }
    }
  }
}