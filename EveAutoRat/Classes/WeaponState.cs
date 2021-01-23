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

    public WeaponState(Bitmap bmp, Rectangle bounds, string name)
    {
      this.bmp = bmp;
      this.bounds = bounds;
      this.name = name;
    }

    public virtual WeaponStateFlag IsActive(Bitmap screenBitmap)
    {
      Bitmap section = screenBitmap.Clone(bounds, screenBitmap.PixelFormat);
      ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.0f);
      TemplateMatch[] matchings = tm.ProcessImage(section, bmp);
      if (matchings.Length == 1)
      {
        float similarity = matchings[0].Similarity;
        if (similarity >= 0.999f)
        {
          return WeaponStateFlag.InActive;
        }
        if (similarity >= 0.89f)
        {
          return WeaponStateFlag.Active;
        }
      }
      return WeaponStateFlag.Unknown;
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

    public override WeaponStateFlag IsActive(Bitmap screenBitmap)
    {
      return WeaponStateFlag.Unknown;
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
