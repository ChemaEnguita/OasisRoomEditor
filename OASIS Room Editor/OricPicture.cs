using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace OASIS_Room_Editor
{
    // <summary>
    // Class holding attribute information in a HIRES Oric picture
    // Much better using a structure to gain speed.
    // <!summary>

    struct Attribute
    {
        public bool isPaperAttribute;
        public bool isInkAttribute;
        public int CurrentInk;
        public int CurrentPaper;
        public bool isInverse;
    }
    // <summary>
    // Class representing a HIRES Oric picture
    // <!summary>
    class OricPicture
    {
        public readonly Color[] ListColors = new Color[] { Color.Black, Color.Red, Color.GreenYellow, Color.Yellow, Color.DarkBlue, Color.Magenta, Color.Cyan, Color.White };
        public int nScans { get; private set; }
        public int nRows { get; private set; }
        private Attribute[,] Attributes;
        private bool[,] isPixelInk;
        public Bitmap theBitmap { get; private set; }

        public OricPicture(int scans, int rows)
        {
            // Set the number of rows and scans
            nRows = rows;
            nScans = scans;

            // Create a bitmap of the correct size
            theBitmap = new Bitmap(nScans * 6, nRows);

            // Create the attribute map
            Attributes = new Attribute[nScans, nRows];

            // Create the real bitmap (true if a pixel is set to ink
            // false otherwise)
            isPixelInk = new bool[nScans * 6, nRows];

            // And set it all to black. And set pixel mode
            // to Half to avoid losing half pixels on the
            // borders when zooming.
            using (var g = Graphics.FromImage(theBitmap))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.FillRectangle(new SolidBrush(Color.Black), 0, 0, theBitmap.Width, theBitmap.Height);
            }

            // Reset all attributes to black paper and white ink (Oric's default)
            for (int i = 0; i < nScans; i++)
                for (int j = 0; j < nRows; j++)
                {
                    Attributes[i, j].isPaperAttribute = false;
                    Attributes[i, j].isInkAttribute = false;
                    Attributes[i, j].CurrentInk = 7;
                    Attributes[i, j].CurrentPaper = 0;
                    Attributes[i, j].isInverse = false;
                }
            // Set all the bits to paper (0);
            for (int i = 0; i < nScans * 6; i++)
                for (int j = 0; j < nRows; j++)
                    isPixelInk[i, j] = false;
        }

        public void InsertColumnsLeft(int nColumns)
        {
            var aNewOricPic = new OricPicture(nScans + nColumns, nRows);

            for (int i=0;i< nScans;i++)
                for(int j=0;j<nRows; j++)
                    aNewOricPic.Attributes[i+nColumns, j]=Attributes[i,j];
            for (int i = 0; i < nScans * 6; i++)
                for (int j = 0; j < nRows; j++)
                    aNewOricPic.isPixelInk[i + (nColumns*6), j] = isPixelInk[i, j];
            aNewOricPic.ResetAllAttributes();

            Attributes = aNewOricPic.Attributes;
            isPixelInk = aNewOricPic.isPixelInk;
            theBitmap = aNewOricPic.theBitmap;
            nScans+=nColumns;

            aNewOricPic = null;
        }


        public void InsertColumnsRight(int nColumns)
        {
            var aNewOricPic = new OricPicture(nScans + nColumns, nRows);

            for (int i = 0; i < nScans; i++)
                for (int j = 0; j < nRows; j++)
                    aNewOricPic.Attributes[i, j] = Attributes[i, j];
            for (int i = 0; i < nScans * 6; i++)
                for (int j = 0; j < nRows; j++)
                    aNewOricPic.isPixelInk[i, j] = isPixelInk[i, j];
            aNewOricPic.ResetAllAttributes();

            Attributes = aNewOricPic.Attributes;
            isPixelInk = aNewOricPic.isPixelInk;
            theBitmap = aNewOricPic.theBitmap;
            nScans += nColumns;

            aNewOricPic = null;
        }


        // Returns the inverse of a color
        public int GetInverse(int color)
        {
            return (color ^ 0xff) & (0x07);
        }

        #region PIXELS
        // Methods to set/clear pixels in the bitmap
        // Value means not 0=ink, 0=paper;
        // Overloaded to use Point objects too.
        public bool SetPixelToValue(int x, int y, int Value)
        {

            // If outside range, return false
            if ( (x / 6 > nScans) || (y > nRows)) return false;

            var scan = x / 6;
            var line = y;

            int ink, paper;

            // Avoid drawing over attributes, and return false
            if ((Attributes[scan, line].isInkAttribute) || (Attributes[scan, line].isPaperAttribute))
            {
                return false;
            }

            // Get the colors for this scan
            ink = Attributes[scan, line].CurrentInk;
            paper = Attributes[scan, line].CurrentPaper;

            // If inverse bit is on, calculate the inverse color
            if (Attributes[scan, line].isInverse)
            {
                ink = GetInverse(ink);
                paper = GetInverse(paper);
            }

            Color brushColor;

            if (Value == 0)
            {
                // Draw in paper color
                brushColor = ListColors[paper];
                // And set bit to 0 in bitmap
                isPixelInk[x, y] = false;
            }
            else
            {
                // Draw in ink color
                brushColor = ListColors[ink];
                // And set bit to 1 in bitmap
                isPixelInk[x, y] = true;
            }

            // Do the actual drawing
            using (Brush b = new SolidBrush(brushColor))
            using (var g = Graphics.FromImage(theBitmap))
            {
                g.FillRectangle(b, x, y, 1, 1);
            }

            return true;
        }

        public bool SetPixelToValue(Point p, int Value)
        {
            return SetPixelToValue(p.X, p.Y, Value);
        }

        public bool SetPixel(int x, int y)
        {
            return SetPixelToValue(x, y, 1);
        }

        public bool SetPixel(Point p)
        {
            return SetPixel(p.X, p.Y);
        }

        public bool ClearPixel(int x, int y)
        {
            return SetPixelToValue(x, y, 0);
        }

        public bool ClearPixel(Point p)
        {
            return ClearPixel(p.X, p.Y);
        }

        public int GetPixel(int x, int y)
        {
            int val;

            if (isPixelInk[x, y])
                val = 1;
            else
                val = 0;
            return val;
        }

        public int GetPixel(Point p)
        {
            return GetPixel(p.X, p.Y);
        }

        #endregion

        #region ATTRIBUTES

        // Methods for dealing with attributes.
        // Calculate paper&ink for all the scans in a line
        public void ResetLineAttributes(int line)
        {
            int cInk, cPaper;
            // Sets the attributes
            cInk = 7; cPaper = 0;
            for (int scan = 0; scan < nScans; scan++)
            {
                if (Attributes[scan, line].isPaperAttribute)
                {
                    cPaper = Attributes[scan, line].CurrentPaper;
                }
                else
                {
                    if (Attributes[scan, line].isInkAttribute)
                    {
                        cInk = Attributes[scan, line].CurrentInk;
                    }
                    else
                    {
                        Attributes[scan, line].CurrentInk = cInk;
                        Attributes[scan, line].CurrentPaper = cPaper;
                    }
                }

                // Create brushes for all the colors, trying to make this routine more
                // efficient
                SolidBrush[] oricBrushes=new SolidBrush[8];
                for (int i=0; i<8; i++)
                    oricBrushes[i] = new SolidBrush(ListColors[i]);

                for (int k = 0; k < 6; k++)
                {
                    // Do the actual drawing
                    var cb = isPixelInk[scan * 6 + k, line] ? cInk : cPaper;
                    cb = isInverse(scan, line) ? GetInverse(cb) : cb;
                    //Color brushColor = ListColors[cb];
                    //using (Brush b = new SolidBrush(brushColor))
                    using (var g = Graphics.FromImage(theBitmap))
                    {
                        g.FillRectangle(oricBrushes[cb], scan * 6 + k, line, 1, 1);
                    }
                }
                // Dispose the brushes
                for (int i = 0; i < 8; i++)
                    oricBrushes[i].Dispose();

            }
        }


        // Calculate paper&ink for all the scans in the image
        public void ResetAllAttributes()
        {
            for (int line = 0; line < nRows; line++)
            {
                ResetLineAttributes(line);
            }
        }


        // Set ink
        public void SetInk(int inkCode, int scan, int line)
        {
            // Mark scan as containing an attribute
            Attributes[scan, line].isInkAttribute = true;
            Attributes[scan, line].isPaperAttribute = false;

            // Store attribute
            Attributes[scan, line].CurrentInk = inkCode;

            // Mark bits as paper
            for (int i = 0; i < 6; i++)
                isPixelInk[scan * 6 + i, line] = false;

            ResetLineAttributes(line);

        }

        // Set paper
        public void SetPaper(int paperCode, int scan, int line)
        {
            // Mark scan as containing an attribute
            Attributes[scan, line].isInkAttribute = false;
            Attributes[scan, line].isPaperAttribute = true;

            // Store attribute
            Attributes[scan, line].CurrentPaper = paperCode;

            // Mark bits as paper
            for (int i = 0; i < 6; i++)
                isPixelInk[scan * 6 + i, line] = false;

            ResetLineAttributes(line);

        }

        // Set inverse
        public void SetInverse(bool value, int scan, int line)
        {
            // Set the scan as Inverse
            Attributes[scan, line].isInverse = value;

            // Repaint the pixels
            var cInk = Attributes[scan, line].CurrentInk;
            var cPaper = Attributes[scan, line].CurrentPaper;

            if (value)
            {
                cInk = GetInverse(cInk);
                cPaper = GetInverse(cPaper);
            }

            for (int i = 0; i < 6; i++)
            {
                // Do the actual drawing
                Color brushColor = ListColors[isPixelInk[scan * 6 + i, line] ? cInk : cPaper];
                using (Brush b = new SolidBrush(brushColor))
                using (var g = Graphics.FromImage(theBitmap))
                {
                    g.FillRectangle(b, scan * 6 + i, line, 1, 1);
                }
            }
        }

        // Remove the attribute 
        public void RemoveAttribute(int scan, int line)
        {
            // Set the scan as Inverse
            Attributes[scan, line].isPaperAttribute = false;
            Attributes[scan, line].isInkAttribute = false;
            ResetLineAttributes(line);
        }

        public int GetScanPaperCode(int scan, int line)
        {
            return Attributes[scan, line].CurrentPaper;
        }

        public Color GetScanPaperColor(int scan, int line)
        {
            var attr = Attributes[scan, line];
            var c = GetScanPaperCode(scan, line);

            if (attr.isInverse)
                c = GetInverse(c);

            return ListColors[c];
        }

        public int GetScanInkCode(int scan, int line)
        {
            return Attributes[scan, line].CurrentInk;
        }

        public Color GetScanInkColor(int scan, int line)
        {
            var attr = Attributes[scan, line];
            var c = GetScanInkCode(scan, line);

            if (attr.isInverse)
                c = GetInverse(c);

            return ListColors[c];
        }

        public bool isInverse(int scan, int line)
        {
            return Attributes[scan, line].isInverse;
        }

        public bool isPaperAttribute(int scan, int line)
        {
            return Attributes[scan, line].isPaperAttribute;
        }

        public bool isInkAttribute(int scan, int line)
        {
            return Attributes[scan, line].isInkAttribute;
        }

        public bool isAttribute(int scan, int line)
        {
            return (isPaperAttribute(scan, line) || isInkAttribute(scan, line));
        }

        #endregion

        #region file I/O

        // Decodes an attribute from a byte value in a scan,
        private void DecodeAttribute(byte val, int scan, int line)
        {
            val = (byte)(val & ~0x80);

            if (val < 8)
                SetInk(val, scan, line);
            else
                SetPaper((val & 0x7), scan, line);
        }

        // Reads and decodes a HIRES image stored in a matrix b
        public void ReadHiresData(byte[,] b)
        {
            nRows = b.GetLength(1);
            nScans = b.GetLength(0);
            // Read & decode data
            for (int line = 0; line < nRows; line++)
                for (int scan = 0; scan < nScans; scan++)
                {
                     if ((b[scan,line] & 0x40) == 0)
                        DecodeAttribute(b[scan, line], scan, line); // It is an attribute
                    else
                    {
                        // Pixel values...
                        int mask = 1;
                        for (int k = 1; k < 7; k++)
                        {
                            SetPixelToValue(scan * 6 + 6 - k, line, b[scan, line] & mask);
                            mask = mask * 2;
                        }
                    }

                    // Set the inverse mode
                    if ((b[scan, line] & 0x80) != 0)
                        SetInverse(true, scan, line);
                    else
                        SetInverse(false, scan, line);

                }
            ResetAllAttributes();
        }

        // Reads and decodes a HIRES image stored in a file
        public void ReadHiresData(string fileName)
        {
            // Create the reader for data.
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);

            // Create the matrix to hold the data
            var b = new byte[nScans, nRows];

            // Read data
            for (int line = 0; line < nRows; line++)
                for (int scan = 0; scan < nScans; scan++)
                    b[scan,line] = r.ReadByte();
            // Close the reader
            r.Close();

            ReadHiresData(b);

        }
        

        // Importing data from a Bitmap. I am using an object here, as opening a file
        // to create it would be a bit restrictive, we may want to use this with an already-created
        // bitmap object.
        // Second param is the threshold (0-1) in brightness of a pixel to consider it ink or paper
        // but b&w pictures are expected.
        // Defaulted to 0.05, which seems to work ok.
        public void ReadBMPData(Bitmap bmp, double threshold=0.05)
        {
            // Some (maybe silly defensive programming here)
            if (bmp == null) return;

            // Read data
            for (int line = 0; line < bmp.Height; /*nRows*/ line++)
                for (int x = 0; x < bmp.Width /*nScans * 6*/; x++)
                {
                    if (bmp.GetPixel(x, line).GetBrightness() < threshold)
                        ClearPixel(x, line);
                    else
                        SetPixel(x, line);
                }
            // This is not necessary (I think), but does no harm.
            ResetAllAttributes();
        }

        public byte[,] EncodeAsHires()
        {
            byte[,] b = new byte[nScans, nRows];

            for (int row = 0; row < nRows; row++)
                for (int scan = 0; scan<nScans; scan++)
                {
                    if(isAttribute(scan,row))
                    {
                        if (Attributes[scan, row].isPaperAttribute)
                            b[scan, row] = (byte)(Attributes[scan, row].CurrentPaper + 16);
                        else
                            b[scan, row] = (byte)(Attributes[scan, row].CurrentInk);
                    }
                    else
                    {
                        b[scan, row] = 0x40;
                        for (int k=0; k<6; k++)
                        {
                            b[scan, row] |= (byte)( (isPixelInk[scan * 6 + k, row] ? 1 : 0) << (5 - k));
                        }
                    }
                    if (Attributes[scan, row].isInverse)
                        b[scan, row] |= 0x80;
                }

            return b;
        }

        public void ExportToHires(String fileName)
        {
            // Create the reader for data.
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter w = new BinaryWriter(fs);

            var b = EncodeAsHires();

            // Write data
            for (int line = 0; line < nRows; line++)
                for (int scan = 0; scan < nScans; scan++)
                    w.Write(b[scan, line]);
            w.Close();
        }

        public void ExportToBmp(String filename)
        {
            theBitmap.Save(filename);
        }

        #endregion

        #region memento pattern
        public PictureMemento CreateCheckPoint()
        {
            Attribute[,] attr = new Attribute[nScans, nRows];
            bool[,] pi = new bool[nScans*6, nRows];

            for (int i = 0; i < nScans; i++)
                for (int j = 0; j < nRows; j++)
                {
                    attr[i, j].CurrentInk = Attributes[i, j].CurrentInk;
                    attr[i, j].CurrentPaper = Attributes[i, j].CurrentPaper;
                    attr[i, j].isInkAttribute = Attributes[i, j].isInkAttribute;
                    attr[i, j].isPaperAttribute = Attributes[i, j].isPaperAttribute;
                    attr[i, j].isInverse = Attributes[i, j].isInverse;
                }

            for (int i = 0; i < nScans * 6; i++)
                for (int j = 0; j < nRows; j++)
                    pi[i, j] = isPixelInk[i, j];

            return new PictureMemento(nScans,nRows,attr,pi);
        }

        public void RestoreCheckPoint(PictureMemento memento)
        {
            nScans = memento.nScans;
            nRows = memento.nRows;

            Attributes = new Attribute[nScans, nRows];
            isPixelInk = new bool[nScans*6, nRows];

            for (int i = 0; i < nScans; i++)
                for (int j = 0; j < nRows; j++)
                     Attributes[i, j]=memento.Attributes[i, j];

            for (int i = 0; i < nScans * 6; i++)
                for (int j = 0; j < nRows; j++)
                    isPixelInk[i, j]=memento.isPixelInk[i,j];

            ResetAllAttributes();
        }
        #endregion
    }


}
