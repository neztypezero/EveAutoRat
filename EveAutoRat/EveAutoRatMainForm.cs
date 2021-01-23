using EveAutoRat.Classes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EveAutoRat
{
  public partial class EveAutoRatMainForm : Form
  {
    private EveAutoRatPlayer player;

    public EveAutoRatMainForm()
    {
      InitializeComponent();

      player = new EveAutoRatPlayer(this);
      player.startPlayer();
    }

    private void characters20x32ToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void characters32x32ToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void characters64x64ToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void readFrameToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void EveAutoRatMainForm_Paint(object sender, PaintEventArgs e)
    {
    }

    private void sectionRectangleToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void EveAutoRatMainForm_MouseDown(object sender, MouseEventArgs e)
    {
      player.MouseDown(e);
    }

    private void EveAutoRatMainForm_MouseMove(object sender, MouseEventArgs e)
    {
      player.MouseMove(e);
    }

    private void EveAutoRatMainForm_MouseUp(object sender, MouseEventArgs e)
    {
      player.MouseUp(e);
    }

    private void startMiningToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void startNewsRATToolStripMenuItem_Click(object sender, EventArgs e)
    {
      player.startRatProgram();
    }

    private void wordsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      player.startLearnProgram();
    }
  }
}
