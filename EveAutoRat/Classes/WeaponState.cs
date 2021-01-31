using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace EveAutoRat.Classes
{
  public enum WeaponStateFlag : int
  {
    Unknown = -1,
    InActive = 0,
    Active = 1
  }

  class WeaponState
  {
    public Bitmap bmp;
    public Rectangle bounds;
    public string name;
    public double clickTime = 0;

    private WeaponStateFlag lastState = WeaponStateFlag.Unknown;
    private WeaponStateFlag currentState = WeaponStateFlag.Unknown;
    private double changeTime = 0;

    public WeaponState(Bitmap bmp, Rectangle bounds, string name)
    {
      this.bmp = bmp;
      this.bounds = bounds;
      this.name = name;
    }

    public void SetWeaponState(Bitmap screenBitmap, double time)
    {
      Bitmap section = screenBitmap.Clone(bounds, screenBitmap.PixelFormat);
      ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.0f);
      TemplateMatch[] matchings = tm.ProcessImage(section, bmp);

      WeaponStateFlag newState = WeaponStateFlag.Unknown;
      if (matchings.Length == 1)
      {
        float similarity = matchings[0].Similarity;
        if (similarity >= 0.999f)
        {
          newState = WeaponStateFlag.InActive;
        }
        else if (similarity >= 0.89f)
        {
          newState = WeaponStateFlag.Active;
        }
      }
      if (currentState != newState)
      {
        changeTime = time + 500;
        currentState = newState;
      }
      if (time > changeTime)
      {
        if (currentState != lastState)
        {
          lastState = currentState;
        }
      }
    }//1596, 1033, 25, 25

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
    public WeaponStateNull():base(null, new Rectangle(0,0,0,0), null)
    {

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
