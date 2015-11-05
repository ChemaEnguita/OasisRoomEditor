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
using System.Windows.Input;

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

        private float ZoomLevel = 2;
        private bool ShowGrid = true;
        private Color GridColor= Color.MediumPurple;
        private Color MiniGridColor= Color.OrangeRed;
        private OricPicture TheOricPic;
        
        enum DrawTools {Cursor, Pen, SelectPixels, SelectAttributes}
        private DrawTools CurrentTool = DrawTools.Cursor;

        Point WhereClicked;                         // Position the user clicked on the picture
        Point startDrag, endDrag;                   // used when dragging 
        private bool SelectingPixels=false;         // true if the user is selecting an area
        private bool SelectionValid = false;        // true if there is an area selected
        private Rectangle SelectedRect;             // The area the user selected
        private PixelBox PastePictureBox = null;    // PictureBox used for pasting
        private bool MovingPastedPic = false;       // Is the user moving the pasted clip?

        private void Form1_Load(object sender, EventArgs e)
        {

            //TheOricPic = new OricPicture(40, 200);// 768 / 6, 136);

            //TheOricPic.ReadHiresData("d:\\dbug_1337_logo.hir");

            // For testing set alternating ink colors
            /*for (int j = 0; j < TheOricPic.nRows; j++)
            {
                TheOricPic.SetInk(j % 8, 0, j);
                TheOricPic.SetPaper((j + 1) % 8, 1, j);
            }
            /*
            HiresPictureBox.Height = (int)(TheOricPic.nRows * ZoomLevel);
            HiresPictureBox.Width= (int)(TheOricPic.nScans*6 * ZoomLevel);

            HiresPictureBox.Image = TheOricPic.theBitmap;// bmp;
            HiresPictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;
            */
            //HiresPictureBox.Anchor = AnchorStyles.Top;
            HiresPictureBox.Enabled = false;
            //HiresPictureBox.Location = new Point(0, 0);

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

            if (SelectionValid)
            {
                using (var aPen = new Pen(Color.PaleGoldenrod,ZoomLevel))
                {
                    aPen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                    e.Graphics.DrawRectangle(aPen, new Rectangle((int)(SelectedRect.X*ZoomLevel), (int)(SelectedRect.Y*ZoomLevel),
                        (int)(SelectedRect.Width*ZoomLevel), (int)(SelectedRect.Height*ZoomLevel)));
                }
            }
        }

        private void DrawGrid(Graphics g)
        {
            int columnCount = (int) (HiresPictureBox.Width/(ZoomLevel * 6));
            int rowCount = (int) (HiresPictureBox.Height/(ZoomLevel * 8));
            using (Pen myPen = new Pen(GridColor, ZoomLevel > 8 ? 3 : 1))
            { 
                for (int xPos = 1; xPos < columnCount; xPos++)
                {
                    for (int yPos = 1; yPos < rowCount; yPos++)
                    {
                        g.DrawLine(
                            myPen,
                            xPos * (6 * ZoomLevel),
                            0,
                            xPos * (6 * ZoomLevel),
                            this.HiresPictureBox.Height);
                        g.DrawLine(
                            myPen,
                            0,
                            yPos * (8 * ZoomLevel),
                            this.HiresPictureBox.Width,
                            yPos * (8 * ZoomLevel));
                    }
                }
            }
        }

        private void DrawMiniGrid(Graphics g)
        {
            int columnCount = (int)(HiresPictureBox.Width / (ZoomLevel ));
            int rowCount = (int)(HiresPictureBox.Height / (ZoomLevel ));
            using (Pen myPen = new Pen(MiniGridColor, 1))
            {
                for (int xPos = 1; xPos < columnCount; xPos++)
                {
                    for (int yPos = 1; yPos < rowCount; yPos++)
                    {
                        g.DrawLine(
                            myPen,
                            xPos * (ZoomLevel),
                            0,
                            xPos * (ZoomLevel),
                            this.HiresPictureBox.Height);
                        g.DrawLine(
                            myPen,
                            0,
                            yPos * (ZoomLevel),
                            this.HiresPictureBox.Width,
                            yPos * (ZoomLevel));
                    }
                }
            }
        }

        private void DrawAttribLabels(Graphics g)
        {
             for(int i=0; i< TheOricPic.nScans; i++)
                for (int j = 0; j < TheOricPic.nRows; j++)
                {
                    if (TheOricPic.isAttribute(i, j)||TheOricPic.isInverse(i, j))
                    {
                        int ink = TheOricPic.GetInverse(TheOricPic.GetScanPaperCode(i, j));
                        String aString = "";

                        if (TheOricPic.isInverse(i, j))
                            ink = TheOricPic.GetInverse(ink);

                        if (TheOricPic.isInkAttribute(i, j))
                            aString = "Ink: " + TheOricPic.GetScanInkCode(i, j);

                        if (TheOricPic.isPaperAttribute(i, j))
                            aString = "Paper: " + TheOricPic.GetScanPaperCode(i, j);

                        if (TheOricPic.isInverse(i, j))
                            aString += " [i]";

                        using (Font aFont = new Font("Calibri", 6*ZoomLevel/8))
                        using (Pen aPen = new Pen(TheOricPic.ListColors[ink], 1))
                        using (SolidBrush aBrush = new SolidBrush(TheOricPic.ListColors[ink]))
                        {
                            g.DrawString(aString, aFont, aBrush, i*6* ZoomLevel + 10, j * ZoomLevel-4);
                        }
                    }
                }
        }

        #region COMMANDS OVER PICTURE
        private void toggleInverseFlagToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            TheOricPic.SetInverse(!TheOricPic.isInverse(scan, row), scan, row);
            HiresPictureBox.Invalidate(); // Trigger redraw of the control.
        }


        private void removeAttributeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            TheOricPic.RemoveAttribute(scan, row);
            HiresPictureBox.Invalidate(); // Trigger redraw of the control.

        }

        private void flipAllBitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            if (TheOricPic.isAttribute(scan, row)) return;

            for (int i = 0; i < 6; i++)
                TheOricPic.SetPixelToValue(scan * 6 + i, row, TheOricPic.GetPixel(scan * 6 + i, row) == 0 ? 1 : 0);
            HiresPictureBox.Invalidate(); // Trigger redraw of the control.
        }

        private void doSetPaper(int color)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            TheOricPic.SetPaper(color, scan, row);
            HiresPictureBox.Invalidate(); // Trigger redraw of the control.
        }

        private void paperBlackMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(0);
        }

        private void paperRedMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(1);
        }

        private void paperGreenMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(2);
        }

        private void paperYellowMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(3);
        }

        private void paperBlueMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(4);
        }

        private void paperMagentaMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(5);
        }

        private void paperCyanMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(6);
        }

        private void paperWhiteMenuItem_Click(object sender, EventArgs e)
        {
            doSetPaper(7);
        }

        private void doSetInk(int color)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            TheOricPic.SetInk(color, scan, row);
            HiresPictureBox.Invalidate(); // Trigger redraw of the control.
        }

        private void inkBlackMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(0);
        }

        private void inkRedMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(1);
        }

        private void inkGreenMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(2);
        }

        private void inkYellowMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(3);
        }

        private void inkBlueMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(4);
        }

        private void inkMagentaMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(5);
        }

        private void inkCyanMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(6);
        }

        private void inkWhiteMenuItem_Click(object sender, EventArgs e)
        {
            doSetInk(7);
        }

        #endregion

        #region TOOLBAR
        private void ButtonCursor_Click(object sender, EventArgs e)
        {
            CurrentTool = DrawTools.Cursor;
            HiresPictureBox.Cursor = Cursors.Default;
        }

        private void ButtonPen_Click(object sender, EventArgs e)
        {
            CurrentTool = DrawTools.Pen;
            HiresPictureBox.Cursor = Cursors.Hand;
        }


        private void ButtonSelection_Click(object sender, EventArgs e)
        {
            CurrentTool = DrawTools.SelectPixels;
            HiresPictureBox.Cursor = Cursors.Cross;
        }


        private void ButtonZoomIn_Click(object sender, EventArgs e)
        {
            if (ZoomLevel == 32) return;

            // Enlarge the Picture box
            HiresPictureBox.Scale(new SizeF(2f, 2f));
            ZoomLevel *= 2;

            // This is needed to keep picture always aligned at top left.
            HiresPictureBox.Location = panel1.AutoScrollPosition;

            // This is needed to try to mantain the area we are 
            // watching when zooming in
            var p = panel1.AutoScrollPosition;
            p.X *= -2;  // The position is retreived as <0, while should be set >0
            p.Y *= -2;
            panel1.AutoScrollPosition = p;

            HiresPictureBox.Invalidate();
        }
  
        private void ButtonZoomOut_Click(object sender, EventArgs e)
        {
            if (ZoomLevel == 2) return;

            // This is needed to try to mantain the area we are 
            // watching when zooming out
            var p = panel1.AutoScrollPosition;
            p.X /= -2;  // The position is retreived as <0, while should be set >0
            p.Y /= -2;
            panel1.AutoScrollPosition = p;

            // Shrink the Picture box
            HiresPictureBox.Scale(new SizeF(0.5f, 0.5f));
            ZoomLevel /= 2;

            // This is needed to keep picture always aligned at top left.
            HiresPictureBox.Location = panel1.AutoScrollPosition;

            HiresPictureBox.Invalidate();
        }

        private void ButtonGrid_Click(object sender, EventArgs e)
        {
            ShowGrid = !ShowGrid;
            HiresPictureBox.Invalidate();
        }


        #endregion

        #region COMMON TOOLBAR

        private void copiarToolStripButton_Click(object sender, EventArgs e)
        {
            CopyCommand();
        }

        private void cortarToolStripButton_Click(object sender, EventArgs e)
        {
            CutCommand();
        }

        private void pegarToolStripButton_Click(object sender, EventArgs e)
        {
            PasteCommand();
        }


        #endregion

        private void PasteCommand()
        {
            // Do nothing if the clipboard doesn't hold an image.
            if (!Clipboard.ContainsImage()) return;

            var bmp = new Bitmap(Clipboard.GetImage());
            PastePictureBox = new PixelBox();

            PastePictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;
            PastePictureBox.Parent = HiresPictureBox;
            PastePictureBox.Size = new Size((int)(bmp.Width*ZoomLevel), (int)(bmp.Height*ZoomLevel));
            PastePictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            PastePictureBox.Location = HiresPictureBox.PointToScreen(HiresPictureBox.Location);
            PastePictureBox.Image = bmp;
            PastePictureBox.Visible = true;

            PastePictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PastePictureBox_MouseDown);
            PastePictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PastePictureBox_MouseMove);
            PastePictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PastePictureBox_MouseUp);

            PastePictureBox.Invalidate();

        }

        private void PastePictureBox_MouseDown(object sender, EventArgs e)
        {
            MovingPastedPic = true;
            PastePictureBox.Capture=true;
        }

        private void PastePictureBox_MouseMove(object sender, EventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            if (MovingPastedPic)
                PastePictureBox.Location = new Point(mouseEventArgs.X, mouseEventArgs.Y);
                
        }

        private void PastePictureBox_MouseUp(object sender, EventArgs e)
        {
            MovingPastedPic = false;
        }

        private void CutCommand()
        {
            CopyCommand();
            for (int x = 0; x < SelectedRect.Width; x++)
                for (int y = 0; y < SelectedRect.Height; y++)
                {
                    TheOricPic.ClearPixel(SelectedRect.X + x, SelectedRect.Y + y);
                }

            HiresPictureBox.Invalidate();
        }

        private void CopyCommand()
        {
            if (!SelectionValid) return;

            using (var bmpCopy = new Bitmap(SelectedRect.Width, SelectedRect.Height))
            {
                for (int x = 0; x < SelectedRect.Width; x++)
                    for (int y = 0; y < SelectedRect.Height; y++)
                    {
                        if (TheOricPic.GetPixel(SelectedRect.X + x, SelectedRect.Y + y) == 1)
                            bmpCopy.SetPixel(x, y, Color.White);
                        else
                            bmpCopy.SetPixel(x, y, Color.Black);
                    }
                Clipboard.SetImage(bmpCopy);
            }
        }

        #region MAIN MENU

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }



        private void importHIRESPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "HIRES Files|*.hir";
            openFileDialog1.Title = "Select a HIRES image File";
            openFileDialog1.FileName="*.hir";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var DS = new DialogSizeHires();
                if (DS.ShowDialog()== System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }


                if (HiresPictureBox.Image != null)
                    HiresPictureBox.Image.Dispose();

                this.Cursor = Cursors.WaitCursor;
                TheOricPic = new OricPicture(DS.picWidth/6, DS.picHeight);
                TheOricPic.ReadHiresData(openFileDialog1.FileName);

                ReloadActions();
                this.Cursor = Cursors.Default;
            }

        }

        private void importPictureFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image files (*.bmp; *.jpg; *.jpeg,*.png, *.tiff)| *.BMP; *.JPG; *.JPEG; *.PNG; *.TIFF; *.TIF";
            openFileDialog1.Title = "Select an image File";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bmp = new Bitmap(openFileDialog1.FileName);
                if (bmp == null)
                    return;

                var s = bmp.Size;

                if (HiresPictureBox.Image != null)
                    HiresPictureBox.Image.Dispose();

                this.Cursor = Cursors.WaitCursor;
                TheOricPic = new OricPicture(s.Width / 6, s.Height);
                TheOricPic.ReadBMPData(bmp);

                bmp.Dispose();

                ReloadActions();
                this.Cursor = Cursors.Default;
            }

        }

        private void ReloadActions()
        {
            if (HiresPictureBox.Enabled == false)
                HiresPictureBox.Enabled = true;
            HiresPictureBox.Height = (int)(TheOricPic.nRows * ZoomLevel);
            HiresPictureBox.Width = (int)(TheOricPic.nScans * 6 * ZoomLevel);
            HiresPictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;
            HiresPictureBox.Image = TheOricPic.theBitmap;// bmp; 

            if (PastePictureBox != null)
            {
                PastePictureBox.Dispose();
                PastePictureBox = null;
            }
            SelectionValid = false;

        }

        #endregion

        #region MOUSE AND EVENT HANDLERS

        private void HiresPictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            /* int numberOfTextLinesToMove = e.Delta;

             if (e.Delta > 0 && ZoomLevel<32)
             {
                 HiresPictureBox.Scale(new SizeF(2f, 2f));
                 ZoomLevel *= 2;
                 //HiresPictureBox.Location = new Point(0, 0);
                 HiresPictureBox.Invalidate();
             }
             if (e.Delta < 0 && ZoomLevel > 2)
             {
                 HiresPictureBox.Scale(new SizeF(0.5f, 0.5f));
                 ZoomLevel /= 2;
                 //HiresPictureBox.Location = new Point(0, 0);
                 HiresPictureBox.Invalidate();
             }
             */
        }

        private void HiresPictureBox_Click(object sender, EventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            SelectionValid = false;

            switch (CurrentTool)
            {
                case DrawTools.Pen:
                    if (mouseEventArgs.Button == MouseButtons.Right)
                    {
                        TheOricPic.ClearPixel((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                    }
                    else
                    {
                        TheOricPic.SetPixel((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                    }

                    HiresPictureBox.Invalidate(); // Trigger redraw of the control.
                    break;
                case DrawTools.Cursor:
                    if (mouseEventArgs.Button == MouseButtons.Right)
                    {
                        WhereClicked.X = (int)(mouseEventArgs.X / ZoomLevel);
                        WhereClicked.Y = (int)(mouseEventArgs.Y / ZoomLevel);

                        removeAttributeToolStripMenuItem.Enabled = TheOricPic.isAttribute(WhereClicked.X / 6, WhereClicked.Y);
                        flipAllBitsToolStripMenuItem.Enabled = !TheOricPic.isAttribute(WhereClicked.X / 6, WhereClicked.Y);
                        contextMenuAttributes.Show(MousePosition);
                    }
                    else
                    {
                        var x = (int)(mouseEventArgs.X / ZoomLevel);
                        var y = (int)(mouseEventArgs.Y / ZoomLevel);
                        if (TheOricPic.GetPixel(x, y) == 1)
                            TheOricPic.ClearPixel(x, y);
                        else
                            TheOricPic.SetPixel(x, y);
                        HiresPictureBox.Invalidate(); // Trigger redraw of the control.
                    }
                    break;
            }

        }

        private void HiresPictureBox_MouseHover(object sender, EventArgs e)
        {
            HiresPictureBox.Focus();
        }

        private void HiresPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            switch (CurrentTool)
            {
                case DrawTools.SelectPixels:
                    startDrag = new Point(e.X,e.Y);
                    endDrag = new Point(e.X, e.Y);
                    WhereClicked.X = (int)(mouseEventArgs.X / ZoomLevel);
                    WhereClicked.Y = (int)(mouseEventArgs.Y / ZoomLevel);
                    HiresPictureBox.Capture = true;
                    SelectingPixels = true;
                 break;
            }


        }

        private void HiresPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            var x = (int)(mouseEventArgs.X / ZoomLevel);
            var y = (int)(mouseEventArgs.Y / ZoomLevel);

            toolStripScanLabel.Text = "Pixel: (" + x + "," + y + ") Scan: " + x / 6 + " Tile: (" + x/6 + "," + y/8 + ")" ;
            StatusBar.Update();

            if (SelectingPixels)
            {
                using (var g = Graphics.FromImage(HiresPictureBox.Image))
                {
                    Point dst = ((Control)sender).PointToScreen(endDrag);
                    Point org = ((Control)sender).PointToScreen(startDrag);
                    ControlPaint.DrawReversibleFrame(new Rectangle(org, new Size(dst.X - org.X, dst.Y - org.Y)), this.BackColor, FrameStyle.Dashed);
                    endDrag = new Point(e.X, e.Y);
                    dst= ((Control)sender).PointToScreen(endDrag);
                    ControlPaint.DrawReversibleFrame(new Rectangle(org, new Size(dst.X - org.X, dst.Y - org.Y)), this.BackColor, FrameStyle.Dashed);
                }
            }
        }

        private void HiresPictureBox_MouseLeave(object sender, EventArgs e)
        {
            toolStripScanLabel.Text = "Outside drawing area";
        }


        private void HiresPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            if (SelectingPixels)
            {
                HiresPictureBox.Capture = false;
                SelectingPixels = false;

                Point dst = ((Control)sender).PointToScreen(endDrag);
                Point org = ((Control)sender).PointToScreen(startDrag);
                ControlPaint.DrawReversibleFrame(new Rectangle(org, new Size(dst.X - org.X, dst.Y - org.Y)), this.BackColor, FrameStyle.Dashed);

                /* User selected a region in the bitmap from 
                    WhereClicked.X
                    WhereClicked.Y

                    to

                    (int)(mouseEventArgs.X / ZoomLevel);
                    (int)(mouseEventArgs.Y / ZoomLevel);
                 */

                Point trueDest = new Point((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                if (Control.ModifierKeys == Keys.Shift)
                {
                    WhereClicked.X = (int)(WhereClicked.X / 6) * 6;
                    trueDest.X = (int)(trueDest.X / 6) * 6;
                }

                SelectedRect = new Rectangle(WhereClicked, new Size(trueDest.X - WhereClicked.X, trueDest.Y - WhereClicked.Y));
                SelectionValid = true;
                HiresPictureBox.Invalidate();
            }

        }

        #endregion
    }
}


