using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace EveAutoRat.Classes
{
  class ActionThreadLearn : ActionThread
  {
    private BlobCounter objectCounter = new BlobCounter();
    private Bitmap drawBuffer = null;
    private Boolean loadObjects = true;
    private Dictionary<int, Rectangle[]> objectList = null;
    private int currentThreshHoldIndex = 0;
    private Rectangle zeroRectangle = new Rectangle(0, 0, 0, 0);
    private Rectangle mouseOverRectangle;

    public ActionThreadLearn(EveAutoRatMainForm parentForm, IntPtr emuHWnd, IntPtr eventHWnd) : base(parentForm, emuHWnd, eventHWnd)
    {
      mouseOverRectangle = zeroRectangle;
    }

    protected override double Step(double totalTime)
    {
      Bitmap currentImg = threshHoldDictionary[threshHoldList[currentThreshHoldIndex]];
      if (loadObjects)
      {
        if (drawBuffer == null)
        {
          drawBuffer = new Bitmap(currentImg.Width, currentImg.Height);
        }
        objectList = new Dictionary<int, Rectangle[]>();
        foreach (int threshhold in threshHoldList)
        {
          Bitmap img = threshHoldDictionary[threshhold];
          objectCounter.ProcessImage(img);
          objectList[threshhold] = objectCounter.GetObjectsRectangles();
        }
        loadObjects = false;
        needUpdate = false;
      }
      using (Graphics g = Graphics.FromImage(drawBuffer))
      {
        g.DrawImage(currentImg, zp);
        Rectangle[] rList = objectList[threshHoldList[currentThreshHoldIndex]];
        foreach (Rectangle r in rList)
        {
          g.DrawRectangle(Pens.BlueViolet, r);
        }
        if (mouseOverRectangle.Width != 0)
        {
          g.DrawRectangle(Pens.Red, mouseOverRectangle);
        }
      }
      Stop();
      parentForm.Invoke(new Action(() =>
      {
        parentForm.BackgroundImage = drawBuffer.Clone(new Rectangle(0, 24, drawBuffer.Width, drawBuffer.Height-24), PixelFormat.Format24bppRgb);
        if (objectList != null)
        {
          List<Bitmap> bmpList = new List<Bitmap>();
          foreach (KeyValuePair<int, Rectangle[]> item in objectList)
          {
            if (item.Key != 64)
            {
              continue;
            }
            Directory.CreateDirectory("PixelObjects\\" + item.Key);

            Dictionary<Rectangle, List<Rectangle>> lineBoundsDictionary = GetBoundsDictionaryList(item.Value);
            List<string> wordList = new List<string>();
            currentImg = threshHoldDictionary[item.Key];
            foreach (List<Rectangle> rList in lineBoundsDictionary.Values)
            {
              string word = "";
              foreach (Rectangle r in rList)
              {
                Bitmap bmp = currentImg.Clone(r, currentImg.PixelFormat);
                bmpList.Add(bmp);
                String text = PixelObjectList.CheckBitmap(bmp, item.Key);
                if (text != null)
                {
                  word += text;
                }
                else
                {
//                  if (r.Width > 30 && r.Width < 80 && r.Height > 30 && r.Height < 80)
                  {
                    bmp.Save("PixelObjects\\" + item.Key + "\\" + r.X + "_" + r.Y + "_" + r.Width + "_" + r.Height + ".bmp");
                  }
                }
              }
              if (word.Length > 0)
              {
                wordList.Add(word);
              }
            }
          }
          // 1826, 900, 25, 25 Nosferatu
          // 1711, 900, 25, 25 Nosferatu
          // 1594, 900, 25, 25 Nosferatu
          // 1826, 900, 25, 25 Nosferatu
          // 1477, 900, 25, 25 Afterburner
          // 1711, 1034, 25, 25 Pulse Laser
          // 1594, 1034, 25, 25 Adaptive Armor Hardener
          // 1477, 1034, 25, 25 Armor Repairer
          // 1360, 1035, 32, 12 Heat Sink
          //Rectangle[] weaponBoundsList = new Rectangle[]
          //{
          //  new Rectangle(1826, 900, 25, 25),
          //  new Rectangle(1711, 900, 25, 25),
          //  new Rectangle(1594, 900, 25, 25),
          //  new Rectangle(1477, 900, 25, 25),
          //  new Rectangle(1711, 1034, 25, 25),
          //  new Rectangle(1594, 1034, 25, 25),
          //  new Rectangle(1477, 1034, 25, 25),
          //  new Rectangle(1352, 1035, 40, 12),
          //};
          //int n = 0;
          //foreach (Rectangle r in weaponBoundsList)
          //{
          //  Bitmap bmp = threshHoldDictionary[0].Clone(r, threshHoldDictionary[0].PixelFormat);
          //  bmp.Save("ObjBitmap\\" + (n++) + ".bmp");
          //}

        }
      }));
      return 0.0;
    }

    public override void MouseDown(MouseEventArgs e)
    {
      //if (e.Button == MouseButtons.Middle)
      //{
      //  mouseOverRectangle = zeroRectangle;
      //  currentThreshHoldIndex++;
      //  if (currentThreshHoldIndex >= threshHoldList.Length)
      //  {
      //    currentThreshHoldIndex = 0;
      //  }
      //}
    }

    public override void MouseMove(MouseEventArgs e)
    {
      //if (objectList != null)
      //{
      //  Point p = new Point(e.X, e.Y + 24);
      //  Rectangle[] rList = objectList[threshHoldList[currentThreshHoldIndex]];
      //  foreach (Rectangle r in rList)
      //  {
      //    if (r.Contains(p))
      //    {
      //      mouseOverRectangle = r;
      //      return;
      //    }
      //  }
      //  mouseOverRectangle = zeroRectangle;
      //}
    }

    public override void MouseUp(MouseEventArgs e)
    {
      //Console.WriteLine(e.X+" "+e.Y);
    }
  }
}
