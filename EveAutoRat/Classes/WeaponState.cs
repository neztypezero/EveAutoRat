using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public enum WeaponStateFlag : int
  {
    Unknown = -1,
    InActive = 0,
    Active = 1
  }

  public class WeaponState
  {
    public Rectangle bounds;
    public Rectangle pixelBounds;
    public string name;
    public double clickTime = 0;
    public double inactiveTime = 0;
    public double activeTime = 0;

    private BlobCounter objectCounter = new BlobCounter();

    private WeaponStateFlag lastState = WeaponStateFlag.Unknown;
    private WeaponStateFlag currentState = WeaponStateFlag.Unknown;
    private double changeTime = 0;
    private double stateDelay = 300;

    ColorFiltering outlineColorFilter = new ColorFiltering(new AForge.IntRange(180, 200), new AForge.IntRange(200, 255), new AForge.IntRange(200, 255));
    public Bitmap previousFrame = null;
    public Bitmap currentFrame = null;
    public Bitmap diffFrame = null;
    public Bitmap pixels = null;
    public int tryCount = 0;

    public WeaponState(Rectangle bounds, Rectangle pixelBounds, PixelObject po)
    {
      this.bounds = bounds;
      this.pixelBounds = pixelBounds;
      this.pixelBounds.X += bounds.X;
      this.pixelBounds.Y += bounds.Y;
      if (po != null)
      {
        this.name = po.text;
        this.pixels = po.bmp;
      }
      else
      {
        this.name = "NULL";
      }
    }

    public virtual void SetWeaponState(Bitmap screenBitmap, double time)
    {
      if (previousFrame == null)
      {
        currentFrame = screenBitmap.Clone(bounds, screenBitmap.PixelFormat);
        outlineColorFilter.ApplyInPlace(currentFrame);
        previousFrame = currentFrame;
        return;
      }
      previousFrame = currentFrame;
      currentFrame = screenBitmap.Clone(bounds, screenBitmap.PixelFormat);
      outlineColorFilter.ApplyInPlace(currentFrame);

      Difference filter = new Difference(previousFrame);
      diffFrame = filter.Apply(currentFrame);

      objectCounter.ProcessImage(diffFrame);
      Rectangle[] rects = objectCounter.GetObjectsRectangles();

      WeaponStateFlag newState = WeaponStateFlag.Unknown;

      using(Bitmap bmp = screenBitmap.Clone(pixelBounds, PixelFormat.Format24bppRgb))
      {
        newState = WeaponStateFlag.InActive;
        Point cp = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        foreach (Rectangle r in rects)
        {
          double radius = (double)bounds.Width / 2.0 - 3.0;
          double innerRadius = radius - 3.0;
          int x = bounds.X + r.X;
          int y = bounds.Y + r.Y;
          Point p1 = new Point(x, y);
          Point p2 = new Point(x + r.Width, y);
          Point p3 = new Point(x + r.Width, y + r.Height);
          Point p4 = new Point(x, y + r.Height);

          double d1 = Math.Sqrt((p1.X - cp.X) * (p1.X - cp.X) + (p1.Y - cp.Y) * (p1.Y - cp.Y));
          double d2 = Math.Sqrt((p2.X - cp.X) * (p2.X - cp.X) + (p2.Y - cp.Y) * (p2.Y - cp.Y));
          double d3 = Math.Sqrt((p3.X - cp.X) * (p3.X - cp.X) + (p3.Y - cp.Y) * (p3.Y - cp.Y));
          double d4 = Math.Sqrt((p4.X - cp.X) * (p4.X - cp.X) + (p4.Y - cp.Y) * (p4.Y - cp.Y));

          if (d1 < radius && d2 < radius && d3 < radius && d4 < radius)
          {
            if (d1 > innerRadius || d2 > innerRadius || d3 > innerRadius || d4 > innerRadius)
            {
              newState = WeaponStateFlag.Active;
              break;
            }
          }
        }
        if (newState == WeaponStateFlag.InActive)
        {
          ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.89f);
          TemplateMatch[] matchings = tm.ProcessImage(bmp, pixels);
          if (matchings.Length == 0)
          {
            newState = WeaponStateFlag.Unknown;
          }
        }
      }
      if (currentState != newState)
      {
        changeTime = time + stateDelay;
        currentState = newState;
      }
      if (time > changeTime)
      {
        if (currentState != lastState)
        {
          lastState = currentState;
          
          if (currentState == WeaponStateFlag.Active)
          {
            activeTime = time;
          }
          else
          {
            activeTime = 0;
          }
          if (currentState == WeaponStateFlag.InActive)
          {
            inactiveTime = time;
          }
          else
          {
            inactiveTime = 0;
          }
        }
      }
    }

    public virtual WeaponStateFlag CurrentState
    {
      get
      {
        return lastState;
      }
    }

    public virtual bool IsNull
    {
      get {
        return false;
      }
    }
  }

  class WeaponStateNull : WeaponState
  {
    public WeaponStateNull() : base(new Rectangle(0, 0, 0, 0), new Rectangle(0, 0, 0, 0), null)
    {
    }

    public override void SetWeaponState(Bitmap screenBitmap, double time) {
      return;
    }

    public override WeaponStateFlag CurrentState
    {
      get
      {
        return WeaponStateFlag.Unknown;
      }
    }

    public override bool IsNull
    {
      get
      {
        return true;
      }
    }
  }
}
