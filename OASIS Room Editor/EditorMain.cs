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

            // Add the PixelBox control for the Hires picture
            HiresPictureBox = new PixelBox();
            //HiresPictureBox.CreateControl();
            HiresPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            HiresPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            panel1.Controls.Add(HiresPictureBox);


            // Make HiresPictureBox handle the events
            HiresPictureBox.MouseWheel += HiresPictureBox_MouseWheel;
            HiresPictureBox.Paint += HiresPictureBox_Paint;
            HiresPictureBox.Click += HiresPictureBox_Click;
            HiresPictureBox.MouseHover += HiresPictureBox_MouseHover;
            HiresPictureBox.MouseDown += HiresPictureBox_MouseDown;
            HiresPictureBox.MouseUp += HiresPictureBox_MouseUp;
            HiresPictureBox.MouseLeave += HiresPictureBox_MouseLeave;
            HiresPictureBox.MouseMove += HiresPictureBox_MouseMove;
        }

        private OASISRoom theRoom;                     // Holds the room information    

        private float ZoomLevel = 2;                    // Level of zoom
        private bool ShowGrid = true;                   // Is the grid showing?
        private bool PasteWithAttrib = true;            // Cut and paste with attributes?


        private Color GridColor= Color.MediumPurple;    // Default Grid color
        private Color MiniGridColor= Color.OrangeRed;   // Default color for the mini grid
        private bool WalkboxEditMode = false;           // Editing walkboxes?
        private int SelectedWalkbox = -1;               // Selected walkbox in editing mode. -1 if none
        
        // Possible drawing tools and current one selected from the toolbar
        enum DrawTools {Cursor, Pen, SelectPixels, SelectAttributes}    
        private DrawTools CurrentTool = DrawTools.Cursor;

        Point WhereClicked;                         // Position the user clicked on the picture
        Point startDrag, endDrag;                   // used when dragging 
        private bool SelectingPixels = false;       // true if the user is selecting an area
        private bool SelectionValid = false;        // true if there is an area selected
        private Rectangle SelectedRect;             // The area the user selected
        private PixelBox PastePictureBox = null;    // PictureBox used for pasting
        private bool MovingPastedPic = false;       // Is the user moving the pasted clip?

        // For copying attributes (now only inverse codes)

        Attribute[,] copiedAttr;
        bool attrValid = false;


        private Point MouseDownLocation;            // Location where the user pressed the mouse button to start dragging the clip

        // For multiple undo/redo using the memento design pattern
        MementoCaretaker undoRedo=new MementoCaretaker();

        // Filename of loaded or saved to room.
        private string roomFileName = "";

        // Has the room info been changed and not saved?
        private bool needsSaving = false;

        private void Form1_Load(object sender, EventArgs e)
        {
            // Start with the HiresPictureBox disabled
            HiresPictureBox.Enabled = false;
 
            // Disable those menus which should not be enabled.
            toolsToolStripMenuItem.Enabled = false;
            editToolStripMenuItem.Enabled = false;
            DrawingTools.Enabled = false;
            exportPictureToFileToolStripMenuItem.Enabled = false;
            exportToHIRESPictureToolStripMenuItem.Enabled = false;
            saveRoomToolStripMenuItem.Enabled = false;

            // Empty undo/redo queue
            undoRedo.Clear();

            // Disable Controls in tabs
            foreach (Control c in tabWalkbox.Controls)
                c.Enabled = false;
            foreach (Control c in tabRoom.Controls)
                c.Enabled = false;
        }

        

        private void HiresPictureBox_Paint(object sender, PaintEventArgs e)
        {
            // If there is no room, there is no picture, so don't draw anything.
            if (theRoom == null)
                return;

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
                using (var aPen = new Pen(Color.LightGray, ZoomLevel))
                {
                    //aPen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                    e.Graphics.DrawRectangle(aPen, new Rectangle((int)(SelectedRect.X*ZoomLevel), (int)(SelectedRect.Y*ZoomLevel),
                        (int)(SelectedRect.Width*ZoomLevel), (int)(SelectedRect.Height*ZoomLevel)));
                }

                using (var aPen = new Pen(Color.DarkSlateGray, ZoomLevel-2))
                {
                   // aPen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
                    e.Graphics.DrawRectangle(aPen, new Rectangle((int)(SelectedRect.X * ZoomLevel), (int)(SelectedRect.Y * ZoomLevel),
                        (int)(SelectedRect.Width * ZoomLevel), (int)(SelectedRect.Height * ZoomLevel)));
                }


                /* var r = new Rectangle((int)(SelectedRect.X * ZoomLevel), (int)(SelectedRect.Y * ZoomLevel),
                         (int)(SelectedRect.Width * ZoomLevel), (int)(SelectedRect.Height * ZoomLevel));
                 r = this.RectangleToScreen(r);
                 ControlPaint.DrawReversibleFrame(r, this.BackColor, FrameStyle.Thick);
                 */
            }

            // If we are editing walkboxes, we should draw them
            if(WalkboxEditMode)
            {
                using (var Pen1 = new Pen(Color.Black, ZoomLevel + 2))
                using (var Pen2 = new Pen(Color.LightGray, ZoomLevel - 2))
                using (var Pen3 = new Pen(Color.Gold, ZoomLevel - 2))
                using (Font aFont = new Font("Calibri", 6 * ZoomLevel))
                using (SolidBrush Brush2 = new SolidBrush(Color.LightGray))
                using (SolidBrush Brush3 = new SolidBrush(Color.Gold))
                {
                    int wb = 0;
                    foreach (Rectangle r in theRoom.walkBoxes.GetBoxes())
                    {
                        //e.Graphics.DrawString(wb.ToString(), aFont, Brush2, r.X * ZoomLevel + 10, r.Y * ZoomLevel - 4);
                        GraphicsPath p = new GraphicsPath();
                        p.AddString(
                            wb.ToString(),             // text to draw
                            FontFamily.GenericSansSerif,  // or any other font family
                            (int)FontStyle.Regular,      // font style (bold, italic, etc.)
                            e.Graphics.DpiY * 6 / 72 * ZoomLevel,       // em size
                            new Point((int)(r.X * ZoomLevel + 10), (int)(r.Y * ZoomLevel)),              // location where to draw text
                            new StringFormat());          // set options here (e.g. center alignment)
                        e.Graphics.DrawPath(Pen1, p);
                        e.Graphics.FillPath(wb == SelectedWalkbox ? Brush3 : Brush2, p);

                        e.Graphics.DrawRectangle(Pen1, new Rectangle((int)(r.X * ZoomLevel), (int)(r.Y * ZoomLevel),
                            (int)(r.Width * ZoomLevel), (int)(r.Height * ZoomLevel)));
                        e.Graphics.DrawRectangle(wb==SelectedWalkbox?Pen3:Pen2, new Rectangle((int)(r.X * ZoomLevel), (int)(r.Y * ZoomLevel),
                            (int)(r.Width * ZoomLevel), (int)(r.Height * ZoomLevel)));
                        wb++;
                    }
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


        //... and the labels for attributes
        private void DrawAttribLabels(Graphics g)
        {
             for(int i=0; i< theRoom.roomImage.nScans; i++)
                for (int j = 0; j < theRoom.roomImage.nRows; j++)
                {
                    if (theRoom.roomImage.isAttribute(i, j)||theRoom.roomImage.isInverse(i, j))
                    {
                        int ink = theRoom.roomImage.GetInverse(theRoom.roomImage.GetScanPaperCode(i, j));
                        String aString = "";

                        if (theRoom.roomImage.isInverse(i, j))
                            ink = theRoom.roomImage.GetInverse(ink);

                        if (theRoom.roomImage.isInkAttribute(i, j))
                            aString = "Ink: " + theRoom.roomImage.GetScanInkCode(i, j);

                        if (theRoom.roomImage.isPaperAttribute(i, j))
                            aString = "Paper: " + theRoom.roomImage.GetScanPaperCode(i, j);

                        if (theRoom.roomImage.isInverse(i, j))
                            aString += " [i]";

                        using (Font aFont = new Font("Calibri", 6*ZoomLevel/8))
                        using (Pen aPen = new Pen(theRoom.roomImage.ListColors[ink], 1))
                        using (SolidBrush aBrush = new SolidBrush(theRoom.roomImage.ListColors[ink]))
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

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;
            theRoom.roomImage.SetInverse(!theRoom.roomImage.isInverse(scan, row), scan, row);
            HiresPictureBox.Invalidate(); // Trigger redraw of the control.
        }


        private void removeAttributeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            if (theRoom.roomImage.isAttribute(scan, row))
            {
                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                needsSaving = true;
                theRoom.roomImage.RemoveAttribute(scan, row);
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }

        }

        private void flipAllBitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            if (!theRoom.roomImage.isAttribute(scan, row))
            {
                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                needsSaving = true;

                for (int i = 0; i < 6; i++)
                    theRoom.roomImage.SetPixelToValue(scan * 6 + i, row, theRoom.roomImage.GetPixel(scan * 6 + i, row) == 0 ? 1 : 0);
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }            
        }

        private void doSetPaper(int color)
        {
            var scan = WhereClicked.X / 6;
            var row = WhereClicked.Y;

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            theRoom.roomImage.SetPaper(color, scan, row);
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

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;
            theRoom.roomImage.SetInk(color, scan, row);
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
            if (ZoomLevel == 1) return;

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

        private void ButtonPasteWithAttrib_Click(object sender, EventArgs e)
        {
            PasteWithAttrib = !PasteWithAttrib;
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
            toolStripScanLabel.Text = "Press SHIFT to snap to scan, CTRL to snap to tile";
            StatusBar.Update();
        }

        private void PastePictureBox_MouseMove(object sender, MouseEventArgs e /*EventArgs e*/)
        {
            if (MovingPastedPic)
            {
                var x = e.X + PastePictureBox.Left - MouseDownLocation.X;
                var y = e.Y + PastePictureBox.Top - MouseDownLocation.Y;

                if ((Control.ModifierKeys == Keys.Shift) || (Control.ModifierKeys == Keys.Control))
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
            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            using (var bmp = new Bitmap(PastePictureBox.Image))
            {
                var ini_y = (int)((PastePictureBox.Top /*- panel1.AutoScrollPosition.Y*/) / ZoomLevel);
                var ini_x = (int)((PastePictureBox.Left/* - panel1.AutoScrollPosition.X*/) / ZoomLevel);

            
                for (int i = 0; i < bmp.Width; i++)
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        var val = bmp.GetPixel(i, j).GetBrightness() > 0.2 ? 1 : 0;
                        var p = new Point(i + ini_x, j + ini_y);
                        if((p.X>=0)&&(p.Y>=0))
                            theRoom.roomImage.SetPixelToValue(p, val);
                    }

                if(attrValid && PasteWithAttrib)
                {
                    for (int i = 0; i < bmp.Width/6; i++)
                        for (int j = 0; j < bmp.Height; j++)
                        {
                            if ((i + ini_x / 6) >= 0 && (j + ini_y) >= 0)
                            {
                                theRoom.roomImage.SetInverse(copiedAttr[i, j].isInverse, i + ini_x / 6, j + ini_y);
                                if (copiedAttr[i, j].isInkAttribute)
                                    theRoom.roomImage.SetInk(copiedAttr[i, j].CurrentInk, i + ini_x / 6, j + ini_y);
                                if (copiedAttr[i, j].isPaperAttribute)
                                    theRoom.roomImage.SetPaper(copiedAttr[i, j].CurrentPaper, i + ini_x / 6, j + ini_y);
                            }
                        }
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

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            for (int x = 0; x < SelectedRect.Width; x++)
                for (int y = 0; y < SelectedRect.Height; y++)
                {
                    theRoom.roomImage.ClearPixel(SelectedRect.X + x, SelectedRect.Y + y);
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
                        if (theRoom.roomImage.GetPixel(SelectedRect.X + x, SelectedRect.Y + y) == 1)
                            bmpCopy.SetPixel(x, y, Color.White);
                        else
                            bmpCopy.SetPixel(x, y, Color.Black);
                    }

                // And pass it to the clipboard
                Clipboard.SetImage(bmpCopy);
            }

            copiedAttr = new Attribute[SelectedRect.Width/6,SelectedRect.Height];
            var sx = SelectedRect.X / 6;
            var sy = SelectedRect.Y;
            for (int x=0; x< SelectedRect.Width / 6;x++)
                for(int y=0; y< SelectedRect.Height;y++)
                {
                    copiedAttr[x, y].isInverse = theRoom.roomImage.isInverse(x+sx, y+sy);
                    copiedAttr[x, y].isInkAttribute = theRoom.roomImage.isInkAttribute(x + sx, y + sy);
                    copiedAttr[x, y].isPaperAttribute = theRoom.roomImage.isPaperAttribute(x + sx, y + sy);
                    copiedAttr[x, y].CurrentPaper = theRoom.roomImage.GetScanPaperCode(x + sx, y + sy);
                    copiedAttr[x, y].CurrentInk = theRoom.roomImage.GetScanInkCode(x + sx, y + sy);
                }
            attrValid = true;

            SelectionValid = false;
            HiresPictureBox.Invalidate();
        }

        #region MAIN MENU


        private void EditorMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "CloseReason", e.CloseReason);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "Cancel", e.Cancel);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "FormClosing Event");
            */

            // Have we got something modified?
            if (needsSaving)
            {
                var result = MessageBox.Show("Changes in room will be lost. Are you sure?", "There are unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    e.Cancel=true;
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            // Have we got something modified?
            if (needsSaving)
            {
                var result = MessageBox.Show("Changes in room will be lost. Are you sure?", "There are unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }
            */
            this.Dispose();
        }


        // Load/save rooms

        private void openRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Have we got something modified?
            if (needsSaving)
            {
                var result = MessageBox.Show("Room changes will be lost. Are you sure?", "Room has changed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }
            
            // Open a dialog requesting the file
            openFileDialog1.Filter = "OASIS room files (*.room)| *.ROOM";
            openFileDialog1.Title = "Select an OASIS room file";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                
                if(theRoom==null)
                {
                    // Create a temporary room
                    theRoom = new OASISRoom(0);
                }
                try {
                    theRoom.LoadOASISRoom(openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Loading error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    theRoom = new OASISRoom(0);
                    
                }
                finally
                {
                    roomFileName = openFileDialog1.FileName;
                    needsSaving = false;
                    // Call the common actions after reloading an image
                    ReloadActions();
                    this.Cursor = Cursors.Default;
                }
            }

        }


        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (theRoom == null) return;

            // Open a dialog requesting the file
            saveFileDialog1.Filter = "OASIS room files (*.room)| *.ROOM";
            saveFileDialog1.Title = "Select an OASIS room file";
            saveFileDialog1.FileName = "*.room";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                doSaving(saveFileDialog1.FileName);
            }

        }

        private void doSaving(string filename)
        {
            this.Cursor = Cursors.WaitCursor;
            needsSaving = false;
            try
            {
                theRoom.SaveOASISRoom(filename);
                roomFileName = filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error while saving room", MessageBoxButtons.OK, MessageBoxIcon.Error);
                needsSaving = true;
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void saveRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (theRoom == null) return;

            if (roomFileName == "")
                saveAsToolStripMenuItem_Click(sender, e);
            else
                doSaving(roomFileName);
        }


        // Importing images to the room
        private bool ConfirmNewImage()
        {
            if (theRoom != null)
                if (theRoom.roomImage != null)
                {
                    var result = MessageBox.Show("Current image will be lost. Are you sure?", "An image already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    return (result == DialogResult.Yes);
                }
            return true;
        }


        private void importHIRESPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // We are about to import an Oric HIRES picture.
            if (!ConfirmNewImage())
                return;

            // Prepare a dialog box for selecting the file
            openFileDialog1.Filter = "HIRES files|*.hir";
            openFileDialog1.Title = "Select a HIRES image file";
            openFileDialog1.FileName="*.hir";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // HIRES files do not include width/height information, open
                // a second dialog box to ask for it
                var DS = new DialogSizeHires();
                if (DS.ShowDialog()== DialogResult.Cancel)
                {
                    return;
                }

                // If there is already a picture, get rid of it
                if (HiresPictureBox.Image != null)
                    HiresPictureBox.Image.Dispose();    // not sure if setting it to null is enough

                // Create a new OricPicture object and ask it to load the file
                this.Cursor = Cursors.WaitCursor;

                // If there is no room, create one to fit, else just
                // create the picture
                if (theRoom == null)
                    theRoom = new OASISRoom(DS.picWidth / 6);
                else
                    theRoom.roomImage = new OricPicture(DS.picWidth/6, DS.picHeight);

                // Tell the picture to load HIRES data
                theRoom.roomImage.ReadHiresData(openFileDialog1.FileName);

                // Call the common actions after reloading a file
                ReloadActions();
                
                this.Cursor = Cursors.Default;
            }

        }


        private void importPictureFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // We are about to import a picture from an image file
            if (!ConfirmNewImage())
                return;
           
            // Open a dialog requesting the file
            openFileDialog1.Filter = "Image files (*.bmp; *.jpg; *.jpeg,*.png, *.tiff)| *.BMP; *.JPG; *.JPEG; *.PNG; *.TIFF; *.TIF";
            openFileDialog1.Title = "Select an image File";
            openFileDialog1.FileName = "";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var bmp = new Bitmap(openFileDialog1.FileName);
                if (bmp == null)
                    return;

                var s = bmp.Size;

                // Clear the current picture if any
                if (HiresPictureBox.Image != null)
                    HiresPictureBox.Image.Dispose();  //not sure if setting it to null is enough

                this.Cursor = Cursors.WaitCursor;
                // Create a new OASISRoom Object and ask it to read the image data from 
                // the bitmap object

                int rs = s.Width / 6;

                if ((s.Width % 6)>0)
                    rs++;

                if (theRoom==null)
                    theRoom = new OASISRoom(rs);
                else
                    theRoom.roomImage = new OricPicture(rs, s.Height);

                theRoom.roomImage.ReadBMPData(bmp);

                // Get rid of the bitmap object
                bmp.Dispose();

                // Call the common actions after reloading an image
                ReloadActions();
                this.Cursor = Cursors.Default;
            }

        }


        private void exportToHIRESPictureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "HIRES files|*.hir";
            saveFileDialog1.Title = "Select a HIRES image file";
            saveFileDialog1.FileName = "*.hir";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                theRoom.roomImage.ExportToHires(saveFileDialog1.FileName);
        }

        private void exportPictureToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Image files (*.bmp; *.jpg; *.jpeg,*.png, *.tiff)| *.BMP; *.JPG; *.JPEG; *.PNG; *.TIFF; *.TIF";
            saveFileDialog1.Title = "Select an image File";
            saveFileDialog1.FileName = "";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                theRoom.roomImage.ExportToBmp(saveFileDialog1.FileName);

        }

        private void aICHelperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (theRoom.roomImage == null)
                return;

            var DAIC = new AICHelperDialog();

            if (DAIC.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            var r = Math.Min(DAIC.Row2, theRoom.roomImage.nRows - 1);
            for (int i = DAIC.Row1; i <= r; i += 2)
            {
                if (DAIC.Attrib1 < 8)
                    theRoom.roomImage.SetInk(DAIC.Attrib1, DAIC.Column, i);
                else
                    theRoom.roomImage.SetPaper((DAIC.Attrib1 & 0x7), DAIC.Column, i);

                theRoom.roomImage.SetInverse(DAIC.Inverse1, DAIC.Column, i);

                if (DAIC.Attrib2 < 8)
                    theRoom.roomImage.SetInk(DAIC.Attrib2, DAIC.Column, i + 1);
                else
                    theRoom.roomImage.SetPaper((DAIC.Attrib2 & 0x7), DAIC.Column, i + 1);

                theRoom.roomImage.SetInverse(DAIC.Inverse2, DAIC.Column, i + 1);
            }
            theRoom.roomImage.ResetAllAttributes();
            HiresPictureBox.Invalidate();
        }

        private void atTherightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            theRoom.InsertColumnsRight(1);
            
            // Call the common actions after reloading an image
            ReloadActions();
            HiresPictureBox.Invalidate();
            
        }

        private void atTheleftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            theRoom.InsertColumnsLeft(1);
            // Call the common actions after reloading an image
            ReloadActions();
            HiresPictureBox.Invalidate();
        }

        private void sectionInTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*var ts = new TileSectioner();

            var p = theRoom.roomImage.EncodeAsHires();
            ts.doSection(p, checkBoxPalette.Checked);
            // Tell the user the number of tiles
            String s = "Size of room: " + ts.tileMap.GetLength(0) + "x" + ts.tileMap.GetLength(1);
            s += "\nNumber of tiles: " + ts.tileSet.Count;
            s += "\nMemory usage: " + (ts.tileMap.GetLength(0) * ts.tileMap.GetLength(1) + ts.tileSet.Count * 8) + " bytes";
            MessageBox.Show(s, "Room picture information");*/

            /*
            theRoom.walkBoxes.CreateWalkMatrix();
            String s="";
            int n = theRoom.walkBoxes.walkMatrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    s += theRoom.walkBoxes.walkMatrix[i, j].ToString() + " ";
                }
                s += "\n";
            }

            MessageBox.Show(s, "Walk matrix");
            */

            saveFileDialog1.Filter = "text files|*.s; *.txt; *.h";
            saveFileDialog1.Title = "Select a file";
            saveFileDialog1.FileName = "*.txt";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                theRoom.ExportAsResource(saveFileDialog1.FileName);

        }



        private void doRestoringState(RoomMemento m)
        {
            if (theRoom == null || m == null)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            // If the size of the image changes, we need to do more adjustments (due
            // to zoom, etc.
            var oldR = theRoom.roomImage.nRows;
            var oldS = theRoom.roomImage.nScans;

            theRoom.RestoreCheckPoint(m);

            if ((theRoom.roomImage.nRows != oldR) || (theRoom.roomImage.nScans != oldS))
            {
                ReloadActions();
            }
            else
            {
                HiresPictureBox.Image = theRoom.roomImage.theBitmap;// bmp; 
                HiresPictureBox.Invalidate();
                UpdateTabRoomData();
            }
            UpdateTabWalkboxData();
            SelectedWalkbox = -1;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (theRoom == null)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }
            RoomMemento m = undoRedo.Undo(theRoom.CreateCheckPoint());
            needsSaving = true;
            doRestoringState(m);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (theRoom == null)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            RoomMemento m = undoRedo.Redo(theRoom.CreateCheckPoint());
            needsSaving = true;
            doRestoringState(m);

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

            // Enable menus
            toolsToolStripMenuItem.Enabled = true;
            editToolStripMenuItem.Enabled = true;
            DrawingTools.Enabled = true;
            exportPictureToFileToolStripMenuItem.Enabled = true;
            exportToHIRESPictureToolStripMenuItem.Enabled = true;
            saveRoomToolStripMenuItem.Enabled = true;

            // and room tab controls
            foreach (Control c in tabRoom.Controls)
                c.Enabled = true;

            // default zoom level is 2
            ZoomLevel = 2;

            // Set the correcth size, interpolation mode and image
            HiresPictureBox.Height = (int)(theRoom.roomImage.nRows * ZoomLevel);
            HiresPictureBox.Width = (int)(theRoom.roomImage.nScans * 6 * ZoomLevel);
            HiresPictureBox.InterpolationMode = InterpolationMode.NearestNeighbor;
            HiresPictureBox.Image = theRoom.roomImage.theBitmap;// bmp; 

            // If we were pasting remove the box
            // The correct UI procedure is pasting over the new picture
            if (PastePictureBox != null)
            {
                PastePictureBox.Dispose();
                PastePictureBox = null;
            }

            // And the selected area too
            SelectionValid = false;

            // Update data in tabs
            if (theRoom.roomSize < theRoom.roomImage.nScans)
                theRoom.roomSize = theRoom.roomImage.nScans;

            UpdateTabRoomData();
            UpdateTabWalkboxData();
        }


        private void invertBitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int inix, iniy, nx, ny;

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

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
                nx = theRoom.roomImage.nScans * 6;
                ny = theRoom.roomImage.nRows;
            }

            for(int i = inix; i < nx; i++)
                for(int j = iniy; j < ny; j++)
                {
                    theRoom.roomImage.SetPixelToValue(i, j, theRoom.roomImage.GetPixel(i, j) == 0 ? 1 : 0);
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

        private void UpdateTabWalkboxData()
        {
            if (SelectedWalkbox == -1)
            {
                // Disable walkbox editing data in tab
                foreach (Control c in tabWalkbox.Controls)
                    c.Enabled = false;
                labelWBSelect.Text = "Select a Walkbox";
            }
            else
            {
                // Disable walkbox editing data in tab
                foreach (Control c in tabWalkbox.Controls)
                    c.Enabled = true;
                // Set the data
                labelWBSelect.Text = "Walkbox " + SelectedWalkbox.ToString();
                Rectangle r = theRoom.walkBoxes.GetBox(SelectedWalkbox);
                var prop = theRoom.walkBoxes.GetProperties(SelectedWalkbox);

                numericUpDownSPX.Value = (r.Left / 6);
                numericUpDownSPY.Value = (r.Top / 8);
                numericUpDownEPX.Value = ((r.Right - 1) / 6);
                numericUpDownEPY.Value = ((r.Bottom - 1) / 8);

                numericUpDownZPlane.Value = prop.zPlane;
                numericUpDownElevation.Value = prop.Elevation;

                checkBoxWalkable.Checked = prop.isWalkable;
                checkBoxLeftCorner.Checked = prop.isLeftCorner;
                checkBoxRightCorner.Checked = prop.isRightCorner;
            }
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
                    undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                    needsSaving = true;

                    if (mouseEventArgs.Button == MouseButtons.Right)
                    {
                        theRoom.roomImage.ClearPixel((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                    }
                    else
                    {
                        theRoom.roomImage.SetPixel((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                    }

                    
                    HiresPictureBox.Invalidate(); // Trigger redraw of the control.
                    break;
                case DrawTools.Cursor:
                    // Cursor toggles pixels with left button or shows context menu with
                    // right button. Works quite nicely for basic editting
                    // But only if not in walkbox edit mode, where it is used to 
                    // select a walkbox
                    if (mouseEventArgs.Button == MouseButtons.Right && !WalkboxEditMode)
                    {
                        WhereClicked.X = (int)(mouseEventArgs.X / ZoomLevel);
                        WhereClicked.Y = (int)(mouseEventArgs.Y / ZoomLevel);

                        removeAttributeToolStripMenuItem.Enabled = theRoom.roomImage.isAttribute(WhereClicked.X / 6, WhereClicked.Y);
                        flipAllBitsToolStripMenuItem.Enabled = !theRoom.roomImage.isAttribute(WhereClicked.X / 6, WhereClicked.Y);
                        contextMenuAttributes.Show(MousePosition);
                    }
                    else
                    {
                        var x = (int)(mouseEventArgs.X / ZoomLevel);
                        var y = (int)(mouseEventArgs.Y / ZoomLevel);

                        if (WalkboxEditMode)
                        {
                            var p = new Point(x, y);
                            int wb = 0;
                            while ((wb < theRoom.walkBoxes.GetNumBoxes()) && (!theRoom.walkBoxes.GetBox(wb).Contains(p)))
                                wb++;
                            if (wb == theRoom.walkBoxes.GetNumBoxes())
                                SelectedWalkbox = -1;
                            else
                                SelectedWalkbox = wb;
                            UpdateTabWalkboxData();
                                
                        }
                        else
                        {
                            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                            needsSaving = true;

                            if (theRoom.roomImage.GetPixel(x, y) == 1)
                                theRoom.roomImage.ClearPixel(x, y);
                            else
                                theRoom.roomImage.SetPixel(x, y);
                        }
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

                if (!WalkboxEditMode)
                {
                    // Update the Status bar including CTRL and SHIFT options
                    toolStripScanLabel.Text = "Pixel: (" + x + "," + y + ") Scan: " + x / 6 + " Tile: (" + x / 6 + "," + y / 8 + ")" + " - Press SHIFT to snap to scan, CTRL to snap to tile";
                }
                else
                {
                    // Tell the user about the walkbox coordinates
                    toolStripScanLabel.Text = "Walkbox from ("+(int)(startDrag.X/ZoomLevel/6)+","+ (int)(startDrag.Y/ZoomLevel/8) +") to (" + x / 6 + "," + y / 8 + ")";
                }
            }
            else
            {
                // Update status bar
                toolStripScanLabel.Text = "Pixel: (" + x + "," + y + ") Scan: " + x / 6 + " Tile: (" + x / 6 + "," + y / 8 + ")";
            }

            StatusBar.Update();
        }

        private void HiresPictureBox_MouseLeave(object sender, EventArgs e)
        {
            toolStripScanLabel.Text = "Outside drawing area";
        }

        private void UpdateTabRoomData()
        {
            if (theRoom != null)
            {
                textBoxName.Text = theRoom.roomName;
                numericUpDownID.Value = theRoom.roomID;
                textBoxSize.Text = theRoom.roomSize.ToString();
                textBoxZPlanes.Text=theRoom.roomZPlanes.ToString();
                labelRoomInfo.Text = "Press UPDATE to calculate\ntiles and image size";
            }
        }

        private void newRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if(theRoom!=null && needsSaving)
            {
                var result = MessageBox.Show("Changes in room will be lost. Are you sure?", "There are unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }

            var dialogNewRoom = new formNewRoom();

            // There is no Cancel button yet... is this necessary?
            if (dialogNewRoom.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            // Create a new room
            theRoom = new OASISRoom(dialogNewRoom.roomName, dialogNewRoom.roomID, dialogNewRoom.roomSize);
            needsSaving = true;

            // No walkbox is selected
            SelectedWalkbox = -1;

            // Actions after loading a new room
            ReloadActions();
        }


        private void textBoxName_Leave(object sender, EventArgs e)
        {
            if (theRoom != null)
            {
                if(theRoom.roomName!=textBoxName.Text)
                {
                    undoRedo.NewCheckPoint(theRoom.CreateCheckPoint(false));
                    needsSaving = true;
                    theRoom.roomName = textBoxName.Text;
                }
            }

        }

        private void textBoxName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                textBoxName_Leave(sender, e);
        }



        private void numericUpDownID_ValueChanged(object sender, EventArgs e)
        {
            if (theRoom != null)
            {
                var id = numericUpDownID.Value;
                if (id >= 0 && id < 256)
                    theRoom.roomID = (int)(id);
                else
                    numericUpDownID.Value = theRoom.roomID;
                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint(false));
                needsSaving = true;
            }
        }


        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            var ts = new TileSectioner();

            var p = theRoom.roomImage.EncodeAsHires();
            ts.doSection(p, checkBoxPalette.Checked);
            // Tell the user the number of tiles
            String s = "Size of room: " + ts.tileMap.GetLength(0) + "x" + ts.tileMap.GetLength(1);
            s += "\nNumber of tiles: " + ts.tileSet.Count;
            s += "\nMemory usage: " + (ts.tileMap.GetLength(0) * ts.tileMap.GetLength(1) + ts.tileSet.Count * 8) + " bytes";
            labelRoomInfo.Text = s;

        }

        private void walkboxModeButton_Click(object sender, EventArgs e)
        {
            WalkboxEditMode = !WalkboxEditMode;
            ButtonPen.Enabled = !WalkboxEditMode;
            //ButtonSelection.Enabled = !WalkboxEditMode;
            //ButtonCursor.Enabled = !WalkboxEditMode;
            //CurrentTool = DrawTools.SelectPixels;
            CurrentTool = DrawTools.Cursor;
            HiresPictureBox.Cursor = Cursors.Default;

            foreach (Control c in tabWalkbox.Controls)
                c.Enabled = WalkboxEditMode && (SelectedWalkbox!=-1);

            // Redraw to paint/remove walkbox rectangles
            HiresPictureBox.Invalidate();
        }

        private void buttonUpdateWB_Click(object sender, EventArgs e)
        {

            Rectangle r=new Rectangle();
            //r.X = Int32.Parse(textBoxSPX.Text)*6;
            r.X = (int)(numericUpDownSPX.Value) * 6;
            r.Y = (int)(numericUpDownSPY.Value) * 8;
            r.Width  = ((int)(numericUpDownEPX.Value) + 1) * 6 - r.X;
            r.Height = ((int)(numericUpDownEPY.Value) + 1) * 8 - r.Y;

            if (r.X>theRoom.roomSize*6 || r.X<0 || r.Y > 16*8 || r.Y < 0 || r.Width<6 || r.Height<8)
            {
                MessageBox.Show ("Wrong coordinates", "Wrong parameter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            theRoom.walkBoxes.ChangeWalkbox(SelectedWalkbox, r);
            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint(false));
            needsSaving = true;

            WalkBoxManager.WalkBoxProperties p;

            p.Elevation = (int)(numericUpDownElevation.Value);
            if(p.Elevation < 0 || p.Elevation >17)
            {
                MessageBox.Show("Wrong value for elevation", "Wrong parameter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            p.zPlane = (int)(numericUpDownZPlane.Value);
            if (p.zPlane < 0 || p.zPlane > 8)
            {
                MessageBox.Show("Wrong value for z-plane", "Wrong parameter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            p.isLeftCorner = checkBoxLeftCorner.Checked;
            p.isRightCorner = checkBoxRightCorner.Checked;
            p.isWalkable = checkBoxWalkable.Checked;

            theRoom.walkBoxes.SetProperties(SelectedWalkbox, p);
    
            UpdateTabWalkboxData();
            HiresPictureBox.Invalidate();
        }

        private void buttonDeleteWb_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the walkbox?",
                "Confirmation",MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint(false));
                theRoom.walkBoxes.Remove(SelectedWalkbox);
                needsSaving = true;
                SelectedWalkbox = -1;
                UpdateTabWalkboxData();
                HiresPictureBox.Invalidate();
            }
        }

        private void nuevoToolStripButton_Click(object sender, EventArgs e)
        {
            newRoomToolStripMenuItem_Click(sender, e);
        }

        private void abrirToolStripButton_Click(object sender, EventArgs e)
        {
            openRoomToolStripMenuItem_Click(sender, e);
        }

        private void guardarToolStripButton_Click(object sender, EventArgs e)
        {
            saveRoomToolStripMenuItem_Click(sender, e);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check rectangle does not exceed the image boundaries
            SelectedRect.Width = theRoom.roomImage.nScans * 6  ;
            SelectedRect.Height = theRoom.roomImage.nRows;
            SelectedRect.X = 0; SelectedRect.Y = 0;
            SelectionValid = true;
            HiresPictureBox.Invalidate();
        }

        private void inverseAllScansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Inverse scans in selection
            if(SelectionValid)
            {
                int send= (int)(SelectedRect.Right / 6);
                if (SelectedRect.Right % 6 != 0)
                    send++;

                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                for (int r = SelectedRect.Top; r<SelectedRect.Bottom; r++)
                    for(int s=(int)(SelectedRect.Left/6); s<send; s++)
                        theRoom.roomImage.SetInverse(!theRoom.roomImage.isInverse(s, r), s, r);
                needsSaving = true;
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }
        }

        private void mirrorImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int inix, iniy, nx, ny;

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            if (SelectionValid)
            {
                // Work only in the selected rectangle
                inix = SelectedRect.Left;
                iniy = SelectedRect.Top;
                nx = inix + SelectedRect.Width;
                ny = iniy + SelectedRect.Height;
            }
            else
            {
                // All image
                inix = 0; iniy = 0;
                nx = theRoom.roomImage.nScans * 6;
                ny = theRoom.roomImage.nRows;
            }

            var im = (nx - inix) / 2 + inix;

            for (int i = inix; i < im; i++)
                for (int j = iniy; j < ny; j++)
                {
                    var oldval = theRoom.roomImage.GetPixel(i, j);
                    theRoom.roomImage.SetPixelToValue(i, j, theRoom.roomImage.GetPixel(nx - i - 1 + inix, j));
                    theRoom.roomImage.SetPixelToValue(nx - i - 1 + inix, j, oldval);
                }

            // Now mirror the attributes too...
            inix = inix / 6;
            im = im / 6;
            nx = nx / 6;

            for (int i = inix; i < im; i++)
                for (int j = iniy; j < ny; j++)
                {
                    Attribute temp = theRoom.roomImage.Attributes[i, j];
                    theRoom.roomImage.Attributes[i, j] = theRoom.roomImage.Attributes[nx-i-1+inix,j];
                    theRoom.roomImage.Attributes[nx-i-1+inix, j] = temp;
                }
            theRoom.roomImage.ResetAllAttributes();
            HiresPictureBox.Invalidate();
        }

        private void inverseOddLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Inverse odd scans in selection
            if (SelectionValid)
            {
                int send = (int)(SelectedRect.Right / 6);
                if (SelectedRect.Right % 6 != 0)
                    send++;

                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                for (int r = SelectedRect.Top; r < SelectedRect.Bottom; r++)
                    for (int s = (int)(SelectedRect.Left / 6); s < send; s++)
                    {
                        if(r%2 == 1) 
                            theRoom.roomImage.SetInverse(!theRoom.roomImage.isInverse(s, r), s, r);
                    }
                needsSaving = true;
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }
        }

        private void inverseEvenLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Inverse odd scans in selection
            if (SelectionValid)
            {
                int send = (int)(SelectedRect.Right / 6);
                if (SelectedRect.Right % 6 != 0)
                    send++;

                undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
                for (int r = SelectedRect.Top; r < SelectedRect.Bottom; r++)
                    for (int s = (int)(SelectedRect.Left / 6); s < send; s++)
                    {
                        if (r % 2 == 0)
                            theRoom.roomImage.SetInverse(!theRoom.roomImage.isInverse(s, r), s, r);
                    }
                needsSaving = true;
                HiresPictureBox.Invalidate(); // Trigger redraw of the control.
            }


        }

        private void flipOddScansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int inix, iniy, nx, ny;

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            if (SelectionValid)
            {
                // Invert only in the selected rectangle
                inix = SelectedRect.Left;
                iniy = SelectedRect.Top;
                nx = inix + SelectedRect.Width;
                ny = iniy + SelectedRect.Height;
            }
            else
            {
                // Invert all image
                inix = 0; iniy = 0;
                nx = theRoom.roomImage.nScans * 6;
                ny = theRoom.roomImage.nRows;
            }

            for (int i = inix; i < nx; i++)
                for (int j = iniy; j < ny; j++)
                {
                    if(j%2 == 1)
                        theRoom.roomImage.SetPixelToValue(i, j, theRoom.roomImage.GetPixel(i, j) == 0 ? 1 : 0);
                }
            HiresPictureBox.Invalidate();

        }

        private void flipEvenScansToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int inix, iniy, nx, ny;

            undoRedo.NewCheckPoint(theRoom.CreateCheckPoint());
            needsSaving = true;

            if (SelectionValid)
            {
                // Invert only in the selected rectangle
                inix = SelectedRect.Left;
                iniy = SelectedRect.Top;
                nx = inix + SelectedRect.Width;
                ny = iniy + SelectedRect.Height;
            }
            else
            {
                // Invert all image
                inix = 0; iniy = 0;
                nx = theRoom.roomImage.nScans * 6;
                ny = theRoom.roomImage.nRows;
            }

            for (int i = inix; i < nx; i++)
                for (int j = iniy; j < ny; j++)
                {
                    if(j%2 ==0)
                        theRoom.roomImage.SetPixelToValue(i, j, theRoom.roomImage.GetPixel(i, j) == 0 ? 1 : 0);
                }
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

                /* User selected a region in the bitmap 
                    Create this rectangle (SelectedRect) and mark it as valid. The Paint methods
                    will draw it from now on
                 */

                Point trueDest = new Point((int)(mouseEventArgs.X / ZoomLevel), (int)(mouseEventArgs.Y / ZoomLevel));
                if ((Control.ModifierKeys == Keys.Shift) || (Control.ModifierKeys == Keys.Control) || WalkboxEditMode)
                {
                    WhereClicked.X = (int)Math.Round((double)WhereClicked.X / 6) * 6;
                    trueDest.X = (int)Math.Round((double)trueDest.X / 6) * 6;
                }

               
                if( (Control.ModifierKeys == Keys.Control) || WalkboxEditMode)
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

                // Check rectangle does not exceed the image boundaries
                if (SelectedRect.Width > theRoom.roomImage.nScans * 6 - 1)
                    SelectedRect.Width = theRoom.roomImage.nScans * 6 - 1;
                if (SelectedRect.Height > theRoom.roomImage.nRows - 1)
                    SelectedRect.Height = theRoom.roomImage.nRows - 1;

                if(WalkboxEditMode)
                {
                    //We are editing walkboxes. Just add it
                    undoRedo.NewCheckPoint(theRoom.CreateCheckPoint(false));
                    needsSaving = true;
                    theRoom.walkBoxes.Add(SelectedRect);
                    // And mark selection as invalid: we don't want the user
                    // to edit, cut or delete...
                    SelectionValid = false;
                }
                else
                    SelectionValid = true;

                // Trigger redraw of picture
                HiresPictureBox.Invalidate();
            }

        }

        #endregion
    }
}


