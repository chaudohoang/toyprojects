using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlideIt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            ArrayList alAllButtons;
            ArrayList ilSmallImages;
            Bitmap MainBitmap;
            Point EmptyPoint = new Point();
           
            int resolution = Int32.Parse(txtX.Text) * Int32.Parse(txtY.Text);
            Form pic = new Form();
            pic.AutoSize = true;
            pic.Dock = DockStyle.Fill;
            Panel Image = new Panel();
            Image.Location = new Point(10, 10);
            Image.Size = new Size(640, 640);
            Panel gamemenu = new Panel();
            gamemenu.Dock = DockStyle.Fill;
            gamemenu.Location = new Point(10, 10+Image.Height+10);
            gamemenu.Size = new Size(640, 10);
            Button btnrestart = new Button();
            btnrestart.Text = "Restart";
            Button btnhint = new Button();
            btnhint.Text = "Hint";
            gamemenu.Controls.Add(btnrestart);
            gamemenu.Controls.Add(btnhint);
            Point curPos = new Point(10, 10);
            int totButtons = gamemenu.Controls.OfType<Button>().Count();
            foreach (Button but in gamemenu.Controls.OfType<Button>())
            {
                but.Width = gamemenu.Width / totButtons;
                but.Location = curPos;
                curPos = new Point(curPos.X + but.Width, 10);
            }
            pic.Controls.Add(Image);
            pic.Controls.Add(gamemenu);
            pic.Show();
            using (OpenFileDialog img = new OpenFileDialog())
            {
                if (img.ShowDialog() == DialogResult.OK)
                {
                    MainBitmap = new Bitmap(320, 320);
                    MainBitmap = (Bitmap)Bitmap.FromFile(img.FileName);
                    Image.BackgroundImage = (Image)(MainBitmap);
                }

            }

        }


    }
}
