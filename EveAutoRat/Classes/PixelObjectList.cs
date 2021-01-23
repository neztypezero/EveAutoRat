using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace EveAutoRat.Classes
{
  class PixelObjectList
  {
    private static String path = "PixelObjects\\";
    private static Dictionary<int, List<PixelObject>> objectList = null;
    public static String CheckBitmap(Bitmap bmp, int threshHold)
    {
      List<PixelObject> poList = GetPixelObjectList(threshHold);
      foreach (PixelObject po in poList)
      {
        if (CompareBitmap(bmp, po.bmp))
        {
          return po.text;
        }
      }
      return null;
    }

    public static Rectangle FindBitmap(Bitmap bmp, string iconName, int threshHold)
    {
      ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.95f);
      List<PixelObject> poList = GetPixelObjectList(threshHold);

      foreach (PixelObject po in poList)
      {
        if (po.text.StartsWith(iconName))
        {
          TemplateMatch[] matchings = tm.ProcessImage(bmp, po.bmp);

          if (matchings.Length > 0)
          {
            return matchings[0].Rectangle;
          }
        }
      }
      return new Rectangle(-1, -1, 0, 0);
    }

    public static List<PixelObject> GetPixelObjectList(int threshHold)
    {
      if (objectList == null)
      {
        LoadObjectList();
      }
      return objectList[threshHold];
    }

    private static void LoadObjectList()
    {
      objectList = new Dictionary<int, List<PixelObject>>();
      if (Directory.Exists(path))
      {
        String[] threshHoldFolders = Directory.GetDirectories(path);
        foreach (String folder in threshHoldFolders)
        {
          String[] threshHoldString = folder.Split('\\');
          int threshHold = Int32.Parse(threshHoldString[threshHoldString.Length - 1]);
          List<PixelObject> poList = new List<PixelObject>();
          objectList[threshHold] = poList;
          String[] imageList = Directory.GetFiles(folder);
          foreach (String imagePath in imageList)
          {
            Bitmap bmp = new Bitmap(imagePath);
            String[] imagePathSplit = imagePath.Split('\\');
            String imageFile = imagePathSplit[imagePathSplit.Length - 1];
            if (imageFile.StartsWith("icon"))
            {
              int fu = imageFile.IndexOf('_');
              int lu = imageFile.LastIndexOf('_');
              String text = imageFile.Substring(fu + 1, lu - fu - 1);
              PixelObject po = new PixelObject(bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format24bppRgb), text);
              poList.Add(po);
            } 
            else if (imageFile.StartsWith("weapon"))
            {
              int fu = imageFile.IndexOf('_');
              int lu = imageFile.LastIndexOf('.');
              String text = imageFile.Substring(fu + 1, lu - fu - 1);
              PixelObject po = new PixelObject(bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format24bppRgb), text);
              poList.Add(po);
            }
          }
        }
      }
    }

    private static bool CompareBitmap(Bitmap bmp1, Bitmap bmp2)
    {
      if (bmp1 == null || bmp2 == null)
        return false;
      if (object.Equals(bmp1, bmp2))
        return true;
      if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
        return false;

      ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.95f);
      TemplateMatch[] matchings = tm.ProcessImage(bmp1, bmp2);
      return (matchings.Length == 1);
    }

  }

  public class PixelObject
  {
    public Bitmap bmp;
    public String text;

    public PixelObject(Bitmap bmp, String text)
    {
      this.bmp = bmp;
      this.text = text;
    }
  }
}
