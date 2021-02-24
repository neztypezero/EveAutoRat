using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
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
      //IntPtr ldMultiPlayerHWnd = GetHWndByName("LDMultiPlayer");
      //if (ldMultiPlayerHWnd != IntPtr.Zero)
      //{
      //  emuHWnd = ldMultiPlayerHWnd;
      //  eventHWnd = ldMultiPlayerHWnd;

      //  using (Bitmap screenBmp = Win32.GetScreenBitmap(ldMultiPlayerHWnd))
      //  {
      //    Grayscale grayScaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
      //    using (Bitmap currentFrame = grayScaleFilter.Apply(screenBmp))
      //    {
      //      Threshold thFilter = new Threshold(64);
      //      using (Bitmap b = thFilter.Apply(currentFrame))
      //      {
      //        AForge.Imaging.BlobCounter objectCounter = new AForge.Imaging.BlobCounter();
      //        objectCounter.ProcessImage(b);
      //        Rectangle[] rList = objectCounter.GetObjectsRectangles();
      //        foreach(Rectangle r in rList)
      //        {

      //        }
      //      }
      //    }


      //  }

      List<IntPtr> hWndList = Win32.FindWindowList("EveAutoRat - Addison Zen");
      if (hWndList.Count == 1)
      {
        emuHWnd = hWndList[0];
        List<IntPtr> childHWnd = Win32.GetAllChildHandles(emuHWnd);
        eventHWnd = childHWnd[0];
      }
    }

    public IntPtr GetHWndByName(string windowName)
    {
      List<IntPtr> hWndList = Win32.FindWindowList(windowName);
      if (hWndList.Count == 1)
      {
        return hWndList[0];
      }
      return IntPtr.Zero;
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
        StopCurrentAction();
      }
    }

    public void StopCurrentAction()
    {
      if (currentAction != null)
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