using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat
{
  public class LookupBits
  {
    public byte[] pixels;
    public int width;
    public int height;
    public int stride;
    public int x;
    public int y;
    public Rectangle r;
    public int clickOffsetX;
    public int clickOffsetY;
    public int multiClick;
    public bool isTwoColor;
    public string name;

    public LookupBits(int clickOffsetX, int clickOffsetY, int multiClick)
    {
      pixels = null;
      width = -1;
      height = -1;
      stride = -1;
      x = -1;
      y = -1;
      r = new Rectangle(-1, -1, -1, -1);
      this.clickOffsetX = clickOffsetX;
      this.clickOffsetY = clickOffsetY;
      this.multiClick = multiClick;
      isTwoColor = false;
    }

    public LookupBits(string filename, string name) : this(filename, 0, 0, 0) 
    {
      this.name = name;
    }

    public LookupBits(string filename, int clickOffsetX, int clickOffsetY, int multiClick) : this(clickOffsetX, clickOffsetY, multiClick)
    {
      Bitmap bitmap = new Bitmap(filename);
      loadBitMap(bitmap);
      bitmap.Dispose();
    }

    public LookupBits(Bitmap bitmap, int clickOffsetX, int clickOffsetY, int multiClick) : this(clickOffsetX, clickOffsetY, multiClick)
    {
      loadBitMap(bitmap);
    }

    public void loadBitMap(Bitmap bitmap)
    {
      BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

      this.width = bitmap.Width;
      this.height = bitmap.Height;
      this.stride = bitmapData.Stride;
      this.x = -1;
      this.y = -1;

      this.pixels = new byte[Math.Abs(bitmapData.Stride) * bitmapData.Height];
      System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, this.pixels, 0, this.pixels.Length);

      bitmap.UnlockBits(bitmapData);
    }

    public void reloadBitMap(Bitmap bitmap)
    {
      BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

      this.x = -1;
      this.y = -1;

      System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, this.pixels, 0, this.pixels.Length);

      bitmap.UnlockBits(bitmapData);
    }

    public LookupBits Clone()
    {
      LookupBits bits = new LookupBits(this.clickOffsetX, this.clickOffsetY, this.multiClick);
      bits.pixels = (byte[])this.pixels.Clone();
      bits.width = this.width;
      bits.height = this.height;
      bits.stride = this.stride;
      bits.x = this.x;
      bits.y = this.y;
      bits.r = this.r;

      return bits;
    }

    public bool FindCloseBitmapInBitmap(LookupBits bitsB, byte thr)
    {
      byte[] b = bitsB.pixels;
      int bW = bitsB.width;
      int bH = bitsB.height;
      int bStride = bitsB.stride;

      byte[] a = this.pixels;
      int aW = this.width;
      int aH = this.height;
      int aStride = this.stride;

      int scanLength = aW * 3;

      int bMaxX = bW - aW;
      int bMaxY = bH - aH;

      for (int y = 0; y <= bMaxY; y++)
      {
        int i = (y * bStride);
        for (int x = 0; x <= bMaxX; x++)
        {
          if ((a[0] >= b[i] - thr) && (a[0] <= b[i] + thr) && (a[1] >= b[i + 1] - thr) && (a[1] <= b[i + 1] + thr) && (a[2] >= b[i + 2] - thr) && (a[2] <= b[i + 2] + thr))
          {
            bool bad = false;

            for (int j = 0; j < aH; j++)
            {
              int aIndex = j * aStride;
              int bIndex = i + (j * bStride);

              for (int pIndex = 0; pIndex < scanLength; pIndex += 3)
              {
                if (!(
                  (a[aIndex + pIndex] >= b[bIndex + pIndex] - thr) && 
                  (a[aIndex + pIndex] <= b[bIndex + pIndex] + thr) && 
                  (a[aIndex + pIndex + 1] >= b[bIndex + pIndex + 1] - thr) && 
                  (a[aIndex + pIndex + 1] <= b[bIndex + pIndex + 1] + thr) && 
                  (a[aIndex + pIndex + 2] >= b[bIndex + pIndex + 2] - thr) && 
                  (a[aIndex + pIndex + 2] <= b[bIndex + pIndex + 2] + thr)))
                {
                  bad = true;
                  break;
                }
              }
              if (bad)
              {
                break;
              }
            }
            if (!bad)
            {
              this.x = x;
              this.y = y;
              this.r.X = x;
              this.r.Y = y;
              this.r.Width = aW;
              this.r.Height = aH;

              this.x = this.r.X + (this.r.Width / 2);
              this.y = this.r.Y + (this.r.Height / 2);
              return true;
            }
          }
          i += 3;
        }
      }
      this.r.X = -1;
      this.r.Y = -1;
      this.r.Width = -1;
      this.r.Height = -1;
      this.x = -1;
      this.y = -1;
      return false;
    }


    public void FindBitmapInBitmap(LookupBits bitsB, double threshHold)
    {
      FindBitmapInBitmapInRect(bitsB, threshHold, new Rectangle(0, 0, bitsB.width, bitsB.height));
    }

    public double FindBitmapInBitmapInRect(LookupBits bitsB, double threshHold, Rectangle r)
    {
      byte[] b = bitsB.pixels;
      int bW = bitsB.width;
      int bH = bitsB.height;
      int bStride = bitsB.stride;

      byte[] a = this.pixels;
      int aW = this.width;
      int aH = this.height;
      int aStride = this.stride;

      int scanLength = aW * 3;

      int bMaxX = bW - aW;
      int bMaxY = bH - aH;

      if (bMaxX > (r.X + r.Width))
      {
        bMaxX = (r.X + r.Width) - aW;
      }
      if (bMaxY > (r.Y + r.Height))
      {
        bMaxY = (r.Y + r.Height) - aH;
      }

      for (int y = r.Y; y <= bMaxY; y++)
      {
        int i = (y * bStride) + (r.X * 3);
        for (int x = r.X; x <= bMaxX; x++)
        {
          if (a[0] == b[i] && a[1] == b[i + 1] && a[2] == b[i + 2])
          {
            int badCount = 0;

            for (int j = 0; j < aH; j++)
            {
              int aIndex = j * aStride;
              int bIndex = i + (j * bStride);

              if (this.isTwoColor && bitsB.isTwoColor)
              {
                for (int pIndex = 0; pIndex < scanLength; pIndex += 3)
                {
                  if (a[aIndex + pIndex] != b[bIndex + pIndex])
                  {
                    badCount++;
                  }
                }
              }
              else
              {
                for (int pIndex = 0; pIndex < scanLength; pIndex += 3)
                {
                  if (a[aIndex + pIndex + 0] != b[bIndex + pIndex + 0] ||
                    a[aIndex + pIndex + 1] != b[bIndex + pIndex + 1] ||
                    a[aIndex + pIndex + 2] != b[bIndex + pIndex + 2])
                  {
                    badCount++;
                  }
                }
              }

            }
            double badPercentage = (double)badCount / ((double)(aW * aH));
            if (badPercentage <= threshHold)
            {
              this.x = x;
              this.y = y;
              this.r.X = x;
              this.r.Y = y;
              this.r.Width = aW;
              this.r.Height = aH;

              this.x = this.r.X + (this.r.Width / 2);
              this.y = this.r.Y + (this.r.Height / 2);
              return badPercentage;
            }
          }
          i += 3;
        }
      }
      this.r.X = -1;
      this.r.Y = -1;
      this.r.Width = -1;
      this.r.Height = -1;
      this.x = -1;
      this.y = -1;
      return 1.0;
    }

    public void TwoColorizeBitMapData(double keepThreshold)
    {
      byte[] pixels = this.pixels;

      for (int y = 0; y < this.height; y++)
      {
        int i = y * this.stride;
        int end = i + (this.width * 3);
        for (; i < end; i += 3)
        {
          double gray = (double)(pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3.0;
          if (gray > keepThreshold)
          {
            pixels[i] = 0;
            pixels[i + 1] = 0;
            pixels[i + 2] = 0;
          }
          else
          {
            pixels[i] = 255;
            pixels[i + 1] = 255;
            pixels[i + 2] = 255;
          }
        }
      }
      this.isTwoColor = true;
    }

    public void TwoColorizeRBGBitMapData(int rt, int gt, int bt)
    {
      byte[] pixels = this.pixels;

      for (int y = 0; y < this.height; y++)
      {
        int i = y * this.stride;
        int end = i + (this.width * 3);
        for (; i < end; i += 3)
        {
          int b = pixels[i];
          int g = pixels[i + 1];
          int r = pixels[i + 2];
          double gray = (double)(pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3.0;
          if (b > 95 && g > 95 && r < 50)
          {
            pixels[i] = 0;
            pixels[i + 1] = 255;
            pixels[i + 2] = 0;
          }
          else
          {
            pixels[i] = 255;
            pixels[i + 1] = 255;
            pixels[i + 2] = 255;
          }
        }
      }
      this.isTwoColor = true;
    }
  }
}
