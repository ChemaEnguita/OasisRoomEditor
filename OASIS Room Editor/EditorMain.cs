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

        private float ZoomLevel = 2;                    // Level of zoom
        private bool ShowGrid = true;                   // Is the grid showing?
        private Color GridColor= Color.MediumPurple;    // Default Grid color
        private Color MiniGridColor= Color.OrangeRed;   // Default color for the mini grid

        private OricPicture TheOricPic;                 // Holds the Oric picture     
        
        // Possible drawing tools and current one selected from the toolbar
        enum DrawTools {Cursor, Pen, SelectPixels, SelectAttributes}    
        private DrawTools CurrentTool = DrawTools.Cursor;

        Point WhereClicked;                         // Position the user clicked on the picture
        Point startDrag, endDrag;                   // used when dragging 
        private bool SelectingPixels=false;         // true if the user is selecting an area
        private bool SelectionValid = false;        // true if there is an area selected
        private Rectangle SelectedRect;             // The area the user selected
        private PixelBox PastePictureBox = null;    // PictureBox used for pasting
        private bool MovingPastedPic = false;       // Is the user moving the pasted clip?

        private Point MouseDownLocation;            // Location where the user pressed the mouse button to start dragging the clip

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

            // Start with the HiresPictureBox disabled
            HiresPictureBox.Enabled = false;
        }

        

        private void HiresPictureBox_Paint(object sender, PaintEventArgs e)
        {
            // If the grid is showing, just draw it.
            // If zoom level is beyond 8 also draw the mini grid
            if (ShowGrid)
            {
                DrawGrid(e.Graphics);
                if (ZoomLevel > 8)
                    DrawMiniGrid(e.Graphics);
            }

            // If zoom level is beyond 4, also draw attribute labels
            if (ZoomLevel > 4)
                DrawAttribLabels(e.Graphics);

            // If there is a valid selection, draw the selection rectangle
            // This handles when the user dragged to select a picture area
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

        // Method for drawing the grid
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

        // ... and the mini grid
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


        //... and the labes for attributes
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


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch(keyData)
            {
                case (Keys.Control | Keys.C):
                    break;
                case (Keys.Control | Keys.X):
                    break;

            }
            return base.ProcessCmdKey(ref msg, keyData);
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

            // Put back cursor as current toos
            CurrentTool = DrawTools.Cursor;
            HiresPictureBox.Cursor = Cursors.Default;
            
            // Create a new bitmap with the clipboard data
            var bmp = new Bitmap(Clipboard.GetImage());

            // And draw it into a newly created floating picture box
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
            PastePictureBox.Leave += new System.EventHandler(this.PastePictureBox_Leave);

            PastePictureBox.Location=new Point(0 - panel1.AutoScrollPosition.X, 0 - panel1.AutoScrollPosition.Y);
            PastePictureBox.Invalidate();

        }

        private void PastePictureBox_MouseDown(object sender, MouseEventArgs e /*EventArgs e*/)
        {
            MovingPastedPic = true;
            PastePictureBox.Capture=true;
            MouseDownLocation = e.Location;
        }

        private void PastePictureBox_MouseMove(object sender, MouseEventArgs e /*EventArgs e*/)
        {
            if (MovingPastedPic)
            {
                var x = e.X + PastePictureBox.Left - MouseDownLocation.X;
                var y = e.Y + PastePictureBox.Top - MouseDownLocation.Y;

                if (Control.ModifierKeys == Keys.Shift || Control.ModifierKeys == Keys.Control)
                {
                    x = (int)(Math.Round((double) x / (6*ZoomLevel)) * (6*ZoomLevel));
                }
                if (Control.ModifierKeys == Keys.Control)
                {
                    y = (int)(Math.Round((double)y / (8*ZoomLevel)) * (8*ZoomLevel));
                }
                PastePictureBox.Top = y;
                PastePictureBox.Left = x;

            }


        }

        private void PastePictureBox_MouseUp(object sender, EventArgs e)
        {
            MovingPastedPic = false;
            PastePictureBox.Capture = false;
        }

        private void PastePictureBox_Leave(object sender, EventArgs e)
        {
 
        }


        private void CommonPasteCommand()
        {
            // Common actions after pasting or abort pasting
            MovingPastedPic = false;
            PastePictureBox.Dispose();
            PastePictureBox = null;

            HiresPictureBox.Invalidate();
        }

        private void abortPasteCommand()
        {
            // An action aborted pasting the picture
            CommonPasteCommand();
        }

        private void PerformPasting()
        {
            // Do the actual copy of the bitmap contents to the
            // Oric picture 
            using (var bmp = new Bitmap(PastePictureBox.Image))
            {
                var ini_y = (int)((PastePictureBox.Top /*- panel1.AutoScrollPosition.Y*/) / ZoomLevel);
                var ini_x = (int)((PastePictureBox.Left/* - panel1.AutoScrollPosition.X*/) / ZoomLevel);

            
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        var val = bmp.GetPixel(i, j).GetBrightness() > 0.2 ? 1 : 0;
                        var p = new Point(i + ini_x, j + ini_y);
                        if((p.X>0)&&(p.Y>0))
                            TheOricPic.SetPixelToValue(p, val);
                    }
            }
            // Run the common actions for pasting or abort pasting (basically getting rid of
            // the picture box, invalidating, ...
            CommonPasteCommand();
        }

        private void DeleteCommand()
        {
            // If there is no valid selection ignore command
            if (!SelectionValid) return;

            for (int x = 0; x < SelectedRect.Width; x++)
                for (int y = 0; y < SelectedRect.Height; y++)
                {
                    TheOricPic.ClearPixel(SelectedRect.X + x, SelectedRect.Y + y);
                }

            SelectionValid = false;
            HiresPictureBox.Invalidate();

        }

        private void CutCommand()
        {
            // If there is no valid selection ignore command
            if (!SelectionValid) return;

            // Basically call CopyCommand and then clear the
            // original image in the corresponding area
            CopyCommand();
            SelectionValid = true; // Trick the delete command
            DeleteCommand();
        }

        private void CopyCommand()
        {
            // If there is no valid selection ignore command
            if (!SelectionValid) return;

            // Put back cursor as current toos
            CurrentTool = DrawTools.Cursor;
            HiresPictureBox.Cursor = Cursors.Default;

            // Create a bitmap holding the picture data inside the selection rectangle
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

                // And pass it to the clipboard
                Clipboard.SetImage(bmpCopy);
            }

            SelectionValid = false;
            HiresPictureBox.Invalidate();
        }

        #region MAIN MENU

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void importHIRESPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // We are about to import an Oric HIRES picture.
            // Prepare a dialog box for selecting the file
            openFileDialog1.Filter = "HIRES Files|*.hir";
            openFileDialog1.Title = "Select a HIRES image File";
            openFileDialog1.FileName="*.hir";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // HIRES files do not include width/height information, open
                // a second dialog box to ask for it
                var DS = new DialogSizeHires();
                if (DS.ShowDialog()== System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }

                // If there is already a picture, get rid of it
                if (HiresPictureBox.Image != null)
                    HiresPictureBox.Image.Dispose();    // not sure if setting it to null is enough

                // Create a new OricPicture object and ask it to load the file
                this.Cursor = Cursors.WaitCursor;
                TheOricPic = new OricPicture(DS.picWidth/6, DS.picHeight);
                TheOricPic.ReadHiresData(openFileDialog1.FileName);

                // Call the common actions after reloading a file
                ReloadActions();

                this.Cursor = Cursors.Default;
            }

        }

        private void importPictureFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // We are about to import a picture from an image file
            // Open a dialog requesting the file
            openFileDialog1.Filter = "Image files (*.bmp; *.jpg; *.jpeg,*.png, *.tiff)| *.BMP; *.JPG; *.JPEG; *.PNG; *.TIFF; *.TIF";
            openFileDialog1.Title = "Select an image File";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var bmp = new Bitmap(openFileDialog1.FileName);
                if (bmp == null)
                    return;

                var s = bmp.Size;

                // Clear the current picture if any
                if (HiresPictureBox.Image != null)
                    HiresPictureBox.Image.Dispose();  //not sure if setting it to null is enough

                this.Cursor = Cursors.WaitCursor;
                // Create a new OricPicture Object and ask it to read the data from 
                // the bitmap object
                TheOricPic = new OricPicture(s.Width / 6, s.Height);
                TheOricPic.ReadBMPData(bmp);

                // Get rid of the bitmap object
                bmp.Dispose();

                // Call the common actions after reloading an image
                ReloadActions();
                this.Cursor = Cursors.Default;
            }

        }


        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutCommand();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyCommand();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteCommand();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteCommand();
        }

        // Common actions after reloading a file
        private void ReloadActions()
        {
            // Enable the PictureBox (really a PixelBox) control
            if (HiresPictureBox.Enabled == false)
                HiresPictureBox.Enabled = true;

            // default zoom level is 2
            ZoomLevel = 2;

            // Set the correcth size, interpolation mode and image
            HiresPictureBox.Height = (int)(TheOricPic.nRows * ZoomLevel);
            HiresPictureBox.Width = (int)(TheOricPic.nScans * 6 * ZoomLevel);
            HiresPictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;
            HiresPictureBox.Image = TheOricPic.theBitmap;// bmp; 

            // If we were pasting remove the box
            // The correct UI procedure is pasting over the new picture
            if (PastePictureBox != null)
            {
                PastePictureBox.Dispose();
                PastePictureBox = null;
            }

            // And the selected area too
            SelectionValid = false;

        }


        private void invertBitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int inix, iniy, nx, ny;

            if (SelectionValid)
            {
                // Invert only in the selected rectangle
                inix = SelectedRect.Left;
                iniy = SelectedRect.Top;
                nx = inix+SelectedRect.Width;
                ny = iniy+SelectedRect.Height;
            }
            else
            {
                // Invert all image
                inix = 0; iniy = 0;
                nx = TheOricPic.nScans * 6;
                ny = TheOricPic.nRows;
            }

            for(int i = inix; i < nx; i++)
                for(int j = iniy; j < ny; j++)
                {
                    TheOricPic.SetPixelToValue(i, j, TheOricPic.GetPixel(i, j) == 0 ? 1 : 0);
                }
            HiresPictureBox.Invalidate();
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

            // If there is a selected area, invalidate it
            if (SelectionValid)
            { 
                SelectionValid = false;
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }

            // If there is a clip with data to paste, do it
            if (PastePictureBox != null)
            {
                PerformPasting();
                return;
            }

            // Act depending on the tool selected
            switch (CurrentTool)
            {
                case DrawTools.Pen:
                    // Pen sets/clears pixel depending on mouse button
                    // this is not confortable to use, and may need modification
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
                    // Cursor toggles pixels with left button or shows context menu with
                    // right button. Works quite nicely for basic editting
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

            // button down
            switch (CurrentTool)
            {
                case DrawTools.SelectPixels:
                    // If the tool is the selection, take note of 
                    // everything and capture the mouse
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

            // Update status bar
            toolStripScanLabel.Text = "Pixel: (" + x + "," + y + ") Scan: " + x / 6 + " Tile: (" + x/6 + "," + y/8 + ")" ;
            StatusBar.Update();

            // If the user is selecting an image area, update the selected rectangle
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

        private void aICHelperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TheOricPic == null)
                return;

            var DAIC = new AICHelperDialog();

            if (DAIC.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            var r = Math.Min(DAIC.Row2, TheOricPic.nRows-1);
            for (int i=DAIC.Row1;i<=r;i+=2)
            {
                if (DAIC.Attrib1 < 8)
                    TheOricPic.SetInk(DAIC.Attrib1, DAIC.Column, i);
                else
                    TheOricPic.SetPaper((DAIC.Attrib1 & 0x7), DAIC.Column, i);

                TheOricPic.SetInverse(DAIC.Inverse1, DAIC.Column, i);

                if (DAIC.Attrib2 < 8)
                    TheOricPic.SetInk(DAIC.Attrib2, DAIC.Column, i+1);
                else
                    TheOricPic.SetPaper((DAIC.Attrib2 & 0x7), DAIC.Column, i+1);

                TheOricPic.SetInverse(DAIC.Inverse2, DAIC.Column, i+1);
            }
            TheOricPic.ResetAllAttributes();
            HiresPictureBox.Invalidate();
        }

        private void atTherightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheOricPic.InsertColumnsRight(1);
            // Call the common actions after reloading an image
            ReloadActions();
            HiresPictureBox.Invalidate();
        }

        private void atTheleftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheOricPic.InsertColumnsLeft(1);
            // Call the common actions after reloading an image
            ReloadActions();
            HiresPictureBox.Invalidate();
        }

        private void HiresPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs == null) return;

            // Button up... If we were selecting an area, it is done
            if (SelectingPixels)
            {
                HiresPictureBox.Capture = false;
                SelectingPixels = false;

                // Erase the frame
                Point dst = ((Control)sender).PointToScreen(endDrag);
                Point org = ((Control)sender).PointToScreen(startDrag);
                ControlPaint.DrawReversibleFrame(new Rectangle(org, new Size(dst.X - org.X, dst.Y - org.Y)), this.BackColor, FrameStyle.Dashed);

                /* User selected a region in the bitmap from 
                    WhereClicked.X
                    WhereClicked.Y

                    to

                    (int)(mouseEventArgs.X / ZoomLevel);
                    (int)(mouseEventArgs.Y / ZoomLevel);

                    Create this rectangle (SelectedRect) and mark it as valid. The Paint methods
                    will draw it from now on
                 */

                Point trueDest = new Point((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                if ((Control.ModifierKeys == Keys.Shift) || (Control.ModifierKeys == Keys.Control))
                {
                    WhereClicked.X = (int)Math.Round((double)WhereClicked.X / 6) * 6;
                    trueDest.X = (int)Math.Round((double)trueDest.X / 6) * 6;
                }

                

                if(Control.ModifierKeys == Keys.Control)
                {
                    WhereClicked.Y = (int)Math.Round((double)WhereClicked.Y / 8) * 8;
                    trueDest.Y = (int)Math.Round((double)trueDest.Y / 8) * 8; ;
                }

                SelectedRect = new Rectangle(WhereClicked, new Size(trueDest.X - WhereClicked.X, trueDest.Y - WhereClicked.Y));

                // If the selection rectangle has no heigtht or width, 
                // mark it as invalid and return
                if ((SelectedRect.Width == 0) || (SelectedRect.Height == 0))
                {
                    SelectionValid = false;
                    return;
                }

                SelectionValid = true;
                HiresPictureBox.Invalidate();
            }

        }

        #endregion
    }
}


