using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
public class Globals
{



   private static bool _expired;
   public static bool Expired
   {
       get
       {
           // Reads are usually simple
           return _expired;
       }
       set
       {
           // You can add logic here for race conditions,
           // or other measurements
           _expired = value;
       }
   }
   // Perhaps extend this to have Read-Modify-Write static methods
   // for data integrity during concurrency? Situational.

}
*/

namespace OASIS_Room_Editor
{
    public partial class EditorMain : Form
    {
        public EditorMain()
        {
            InitializeComponent();
            // Make HiresPictureBox handle the MouseWheel event
            this.HiresPictureBox.MouseWheel += HiresPictureBox_MouseWheel;
        }

        protected ActiveAttributes AttributeMap=new ActiveAttributes();
        protected float ZoomLevel = 2;
        protected bool ShowGrid = true;

        private void Form1_Load(object sender, EventArgs e)
        {

            // For testing set alternating ink colors
            for (int j = 0; j < PictureDefinitions.MaxLines; j++)
            {
                AttributeMap.SetInk(j % 8, 0, j);
                AttributeMap.SetPaper((j+1) % 8, 1, j);
            }

            // Create an empty bitmap for the size of the largest room
            var bmp = new Bitmap(768, 136);
            using (var g = Graphics.FromImage(bmp))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.FillRectangle(new SolidBrush(Color.Black), 0, 0, bmp.Width, bmp.Height);
                for(int i=0;i<128;i++)
                    for(int j=0;j<136;j++)
                    {
                        for (int k = 0; k < 6; k++)
                            bmp.SetPixel(i*6 + k, j, PictureDefinitions.ListColors[AttributeMap.CurrentPaper[i, j]]);
                    }


            }
            HiresPictureBox.Image = bmp;
            HiresPictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;
        }

        private void HiresPictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta;

            if (e.Delta > 0 && ZoomLevel<32)
            {
                HiresPictureBox.Scale(new SizeF(2f, 2f));
                ZoomLevel *= 2;
                HiresPictureBox.Location = new Point(0, 0);
                HiresPictureBox.Invalidate();
            }
            if (e.Delta < 0 && ZoomLevel > 2)
            {
                HiresPictureBox.Scale(new SizeF(0.5f, 0.5f));
                ZoomLevel /= 2;
                HiresPictureBox.Location = new Point(0, 0);
                HiresPictureBox.Invalidate();
            }
         }

        private void HiresPictureBox_Click(object sender, EventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {
                var bmp = HiresPictureBox.Image;
                var locx = (int)(mouseEventArgs.X / ZoomLevel);
                var locy = (int)(mouseEventArgs.Y / ZoomLevel);
                var scan =  locx/ 6;
                var line = locy;

                int ink, paper;

                // Avoid drawing over attributes
                if (AttributeMap.isInkAttribute[scan, line] || AttributeMap.isPaperAttribute[scan, line])
                {
                    return;
                }

                // Get the colors for this scan
                ink = AttributeMap.CurrentInk[scan, line];
                paper = AttributeMap.CurrentPaper[scan, line];

                // If inverse bit is on, calculate the inverse color
                if (AttributeMap.isInverse[scan, line])
                {
                    ink = (ink ^ 0xff) & (0x07);
                    paper = (paper ^ 0xff) & (0x07);
                }

                Color brushColor;

                if (mouseEventArgs.Button == MouseButtons.Right)
                {
                    // Draw in paper color
                    brushColor = PictureDefinitions.ListColors[paper];
                    // And set bit to 0 in bitmap
                }
                else
                {
                    // Draw in ink color
                    brushColor = PictureDefinitions.ListColors[ink];
                    // And set bit to 1 in bitmap
                }

                // Do the actual drawing
                using (Brush b = new SolidBrush(brushColor))
                using (var g = Graphics.FromImage(bmp))
                {
                    //g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.FillRectangle(b, locx, locy, 1, 1);
                }
                
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }

        }

        private void HiresPictureBox_MouseHover(object sender, EventArgs e)
        {
            HiresPictureBox.Focus();
        }

        private void HiresPictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (ShowGrid)
            {
                DrawGrid(e.Graphics);
                if (ZoomLevel > 8)
                    DrawMiniGrid(e.Graphics);
            }
            if (ZoomLevel > 4)
                DrawAttribLabels(e.Graphics);
        }

        private void DrawGrid(Graphics g)
        {
            int columnCount = (int) (HiresPictureBox.Width/(ZoomLevel * 6));
            int rowCount = (int) (HiresPictureBox.Height/(ZoomLevel * 8));
            using (Pen myPen = new Pen(Color.OrangeRed, ZoomLevel > 8 ? 3 : 1))
            { 
                for (int xPos = 1; xPos < columnCount; xPos++)
                {
                    for (int yPos = 1; yPos < rowCount; yPos++)
                    {
                        g.DrawLine(
                            myPen,
                            xPos * (6 * ZoomLevel),// this.HiresPictureBox.Width / columnCount,
                            0,
                            xPos * (6 * ZoomLevel),//this.HiresPictureBox.Width / columnCount,
                            this.HiresPictureBox.Height);
                        g.DrawLine(
                            myPen,
                            0,
                            yPos * (8 * ZoomLevel),//this.HiresPictureBox.Height / rowCount, 
                            this.HiresPictureBox.Width,
                            yPos * (8 * ZoomLevel)); //this.HiresPictureBox.Height / rowCount);
                    }
                }
            }
        }

        private void DrawMiniGrid(Graphics g)
        {
            int columnCount = (int)(HiresPictureBox.Width / (ZoomLevel ));
            int rowCount = (int)(HiresPictureBox.Height / (ZoomLevel ));
            using (Pen myPen = new Pen(Color.MediumPurple, 1))
            {
                for (int xPos = 1; xPos < columnCount; xPos++)
                {
                    for (int yPos = 1; yPos < rowCount; yPos++)
                    {
                        g.DrawLine(
                            myPen,
                            xPos * (ZoomLevel),// this.HiresPictureBox.Width / columnCount,
                            0,
                            xPos * (ZoomLevel),//this.HiresPictureBox.Width / columnCount,
                            this.HiresPictureBox.Height);
                        g.DrawLine(
                            myPen,
                            0,
                            yPos * (ZoomLevel),//this.HiresPictureBox.Height / rowCount, 
                            this.HiresPictureBox.Width,
                            yPos * (ZoomLevel)); //this.HiresPictureBox.Height / rowCount);
                    }
                }
            }
        }

        private int Inverse(int n)
        {
            return (n ^ 0xff) & (0x07);
        }

        private void DrawAttribLabels(Graphics g)
        {
             for(int i=0; i< PictureDefinitions.MaxScans; i++)
                for (int j = 0; j < PictureDefinitions.MaxLines; j++)
                {
                    if (AttributeMap.isInkAttribute[i, j] || AttributeMap.isPaperAttribute[i, j])
                    {
                        int ink = Inverse(AttributeMap.CurrentPaper[i, j]);
                        String aString = "";

                        if (AttributeMap.isInkAttribute[i, j])
                            aString = "Ink: " + AttributeMap.CurrentInk[i, j];
                        else
                            aString = "Paper: " + AttributeMap.CurrentPaper[i, j];

                        using (Font aFont = new Font("Calibri", 6*ZoomLevel/8))
                        using (Pen aPen = new Pen(PictureDefinitions.ListColors[ink], 1))
                        using (SolidBrush aBrush = new SolidBrush(PictureDefinitions.ListColors[ink]))
                        {

                            g.DrawString(aString, aFont, aBrush, i*6*ZoomLevel+10,j*ZoomLevel);
                        }
                    }
                }
        }

    }
}


