using AForge.Imaging.Filters;
using System;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EveAutoRat.Classes
{
  public class ActionThread
  {
    protected EveAutoRatMainForm parentForm = null;
    protected IntPtr emuHWnd = IntPtr.Zero;
    protected IntPtr eventHWnd = IntPtr.Zero;
    protected Grayscale grayScaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
    public int[] threshHoldList = new int[] { 64, 80, 96, 112, 128 };
    public Dictionary<int, Bitmap> threshHoldDictionary = new Dictionary<int, Bitmap>();
    public Dictionary<Rectangle, List<Rectangle>> lineBoundsDictionary;
    protected Bitmap eveBitmap = null;
    protected Bitmap currentFrame = null;
    protected Boolean needUpdate = true;
    public bool running = false;

    protected Point zp = new Point(0, 0);

    public ActionThread(EveAutoRatMainForm parentForm, IntPtr emuHWnd, IntPtr eventHWnd)
    {
      this.parentForm = parentForm;
      this.emuHWnd = emuHWnd;
      this.eventHWnd = eventHWnd;
    }

    public Point GetClickPoint(Rectangle bounds)
    {
      Point center = bounds.Location;
      center.X += bounds.Width / 2;
      center.Y += (bounds.Height / 2); // 52 is the Title + Menu height
      return center;
    }

    public IntPtr GetEventHWnd()
    {
      return eventHWnd;
    }

    public void Start()
    {
      Thread actionThread = new Thread(RunLoop);
      actionThread.Start();
    }

    private void RunLoop()
    {
      if (!running)
      {
        running = true;
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        double lastTime = 0.0;
        double waitTime = 100.0;
        while (running)
        {
          TimeSpan ts = stopWatch.Elapsed;
          if (threshHoldDictionary.Count > 0)
          {
            StepEvery(ts.TotalMilliseconds);
          }
          if ((ts.TotalMilliseconds - lastTime) >= waitTime)
          {
            //Console.WriteLine("RunTime " + (ts.TotalMilliseconds - lastTime));
            lastTime = ts.TotalMilliseconds;

            if (needUpdate)
            {
              eveBitmap = Win32.GetScreenBitmap(emuHWnd);
              Win32.CopyScreenBitmap(emuHWnd, eveBitmap);
              currentFrame = grayScaleFilter.Apply(eveBitmap);
              threshHoldDictionary[0] = eveBitmap;
              threshHoldDictionary[1] = currentFrame;
              for (int i = 0; i < threshHoldList.Length; i++)
              {
                Threshold thFilter = new Threshold(threshHoldList[i]);
                Bitmap b = thFilter.Apply(currentFrame);
                threshHoldDictionary[threshHoldList[i]] = b.Clone(new Rectangle(0, 0, b.Width, b.Height), PixelFormat.Format32bppArgb);
              }
            }
            waitTime = Step(ts.TotalMilliseconds);
          }
        }
        foreach (KeyValuePair<int, Bitmap> item in threshHoldDictionary)
        {
          item.Value.Save("ObjBitmap\\o" + item.Key + ".bmp");
        }
        parentForm.Invoke(new Action(() =>
        {
          parentForm.BackgroundImage = null;
        }));
      }
    }

    public Dictionary<Rectangle, List<Rectangle>> GetBoundsDictionaryList(Rectangle[] rectArray)
    {
      List<Rectangle> sortedRectList = new List<Rectangle>(rectArray);
      sortedRectList.Sort((r1, r2) => {
        return r1.X - r2.X;
      });
      Dictionary<Rectangle, List<Rectangle>>  lineBoundsDictionary = new Dictionary<Rectangle, List<Rectangle>>();
      foreach (Rectangle rect in sortedRectList)
      {
        if (rect.Height < 80 && rect.Width < 80)
        {
          if (lineBoundsDictionary.Count == 0)
          {
            List<Rectangle> emptyList = new List<Rectangle>();
            emptyList.Add(rect);
            lineBoundsDictionary[rect] = emptyList;
          }
          else
          {
            Boolean addedRect = false;
            foreach (KeyValuePair<Rectangle, List<Rectangle>> kvr in lineBoundsDictionary)
            {
              Rectangle bb = kvr.Key;
              Rectangle r = new Rectangle(rect.X - 8, rect.Y - 2, rect.Width + 8, rect.Height + 4);
              if (bb.IntersectsWith(r))
              {
                lineBoundsDictionary.Remove(bb);
                Rectangle ur = Rectangle.Union(bb, rect);
                r = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                for (int i = 0; i < kvr.Value.Count; i++)
                {
                  Rectangle aRect = kvr.Value[i];
                  if (aRect.IntersectsWith(r))
                  {
                    kvr.Value[i] = Rectangle.Union(aRect, rect);
                    addedRect = true;
                    break;
                  }
                }
                if (!addedRect)
                {
                  kvr.Value.Add(rect);
                  addedRect = true;
                }
                lineBoundsDictionary[ur] = kvr.Value;
                break;
              }
            }
            if (!addedRect)
            {
              List<Rectangle> emptyList = new List<Rectangle>();
              emptyList.Add(rect);
              lineBoundsDictionary[rect] = emptyList;
            }
          }
        }
      }
      return lineBoundsDictionary;
    }

    protected virtual double Step(double totalTime)
    {
      Console.WriteLine("loop");
      Thread.Sleep(1000);
      return 0.0;
    }

    protected virtual void StepEvery(double totalTime)
    {
    }

    public void Stop()
    {
      running = false;
      Console.WriteLine("stop");
    }

    public void Draw(Graphics g)
    {
      Bitmap b = currentFrame;
      if (b != null)
      {
        Console.WriteLine("Draw");
        g.DrawImage(b, zp);
      }
    }

    public virtual void MouseDown(MouseEventArgs e)
    {
    }

    public virtual void MouseMove(MouseEventArgs e)
    {
    }

    public virtual void MouseUp(MouseEventArgs e)
    {
    }
  }
}
