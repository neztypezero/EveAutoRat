
namespace EveAutoRat
{
  partial class LearnPixelsForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.thumbnailListView = new System.Windows.Forms.ListView();
      this.SuspendLayout();
      // 
      // thumbnailListView
      // 
      this.thumbnailListView.AutoArrange = false;
      this.thumbnailListView.BackColor = System.Drawing.Color.Black;
      this.thumbnailListView.Dock = System.Windows.Forms.DockStyle.Left;
      this.thumbnailListView.HideSelection = false;
      this.thumbnailListView.Location = new System.Drawing.Point(0, 0);
      this.thumbnailListView.MultiSelect = false;
      this.thumbnailListView.Name = "thumbnailListView";
      this.thumbnailListView.Size = new System.Drawing.Size(827, 1350);
      this.thumbnailListView.TabIndex = 0;
      this.thumbnailListView.UseCompatibleStateImageBehavior = false;
      // 
      // LearnPixelsForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(2127, 1350);
      this.Controls.Add(this.thumbnailListView);
      this.Name = "LearnPixelsForm";
      this.Text = "2";
      this.Load += new System.EventHandler(this.LearnPixelsForm_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ListView thumbnailListView;
  }
}