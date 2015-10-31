using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace OASIS_Room_Editor
{
    public class Attribute
    {
        public bool isPaperAttribute;
        public bool isInkAttribute;
        public int CurrentInk;
        public int CurrentPaper;
        public bool isInverse;

        public Attribute()
        {
            isPaperAttribute = false;
            isInkAttribute = false;
            CurrentInk = 7;
            CurrentPaper = 0;
            isInverse = false;
        }
    }
    class OricPicture
    {
        public readonly Color[] ListColors = new Color[] { Color.Black, Color.Red, Color.Green, Color.Yellow, Color.DarkBlue, Color.Magenta, Color.Cyan, Color.White };
        public readonly int nScans=40;
        public readonly int nRows = 200;
        public readonly Attribute[,] Attributes;
        private bool[,] isPixelInk;
        public readonly Bitmap theBitmap;

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
            isPixelInk = new bool[nScans*6, nRows];

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
                    Attributes[i, j] = new Attribute();
                    Attributes[i,j].isPaperAttribute = false;
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
            if (x / 6 > nScans || y > nRows) return false;

            var scan = x / 6;
            var line = y;

            int ink, paper;

            // Avoid drawing over attributes, and return false
            if (Attributes[scan,line].isInkAttribute || Attributes[scan,line].isPaperAttribute)
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

            if (Value==0)
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
                if (Attributes[scan,line].isPaperAttribute)
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
                for (int k = 0; k < 6; k++)
                {
                    // Do the actual drawing
                    Color brushColor = ListColors[isPixelInk[scan * 6 + k, line] ? cInk : cPaper];
                    using (Brush b = new SolidBrush(brushColor))
                    using (var g = Graphics.FromImage(theBitmap))
                    {
                        g.FillRectangle(b, scan * 6 + k, line, 1, 1);
                    }
                }
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
            var attr = Attributes[scan, line];

            if (attr.isInverse)
                return GetInverse(attr.CurrentPaper);
            else
                return attr.CurrentPaper;
        }

        public Color GetScanPaperColor(int scan, int line)
        {
            return ListColors[GetScanPaperCode(scan, line)];
        }

        public int GetScanInkCode(int scan, int line)
        {
            var attr = Attributes[scan, line];

            if (attr.isInverse)
                return GetInverse(attr.CurrentInk);
            else
                return attr.CurrentInk;
        }

        public Color GetScanInkColor(int scan, int line)
        {
            return ListColors[GetScanInkCode(scan, line)];
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

    }


}
