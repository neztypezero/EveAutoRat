using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EveAutoRat
{
  public partial class LearnPixelsForm : Form
  {
    public LearnPixelsForm(Bitmap[] bmpList)
    {
      InitializeComponent();
      LoadBitmapList(bmpList);
    }

    private void LearnPixelsForm_Load(object sender, EventArgs e)
    {
      thumbnailListView.Columns.Add("Spacecraft", 150);
      thumbnailListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.HeaderSize);
    }

    public void LoadBitmapList(Bitmap[] bmpList)
    {
      ImageList il = new ImageList();
      il.ImageSize = new Size(100, 100);
      foreach(Bitmap bmp in bmpList)
      {
        il.Images.Add(bmp);
      }
      thumbnailListView.LargeImageList = il;
      for (int i=0;i<bmpList.Length;i++)
      {
        thumbnailListView.Items.Add("?"+i, i);
      }
    }
  }
}
