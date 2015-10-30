using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    public static class PictureDefinitions
    {
        public static Color[] ListColors= new Color[] {Color.Black, Color.Red, Color.Green, Color.Yellow, Color.DarkBlue, Color.Magenta, Color.Cyan, Color.White};
        public const int MaxScans = 128;
        public const int MaxLines = 17 * 8;
    }

    public class ActiveAttributes
    {
        public bool[,] isPaperAttribute=new bool[PictureDefinitions.MaxScans,PictureDefinitions.MaxLines];
        public bool[,] isInkAttribute = new bool[PictureDefinitions.MaxScans, PictureDefinitions.MaxLines];
        public int[,]  CurrentInk = new int[PictureDefinitions.MaxScans, PictureDefinitions.MaxLines];
        public int[,]  CurrentPaper = new int[PictureDefinitions.MaxScans, PictureDefinitions.MaxLines];
        public bool[,] isInverse = new bool[PictureDefinitions.MaxScans, PictureDefinitions.MaxLines];

        public ActiveAttributes()
        {
            for(int i=0;i< PictureDefinitions.MaxScans;i++)
                for(int j=0;j< PictureDefinitions.MaxLines;j++)
                {
                    isPaperAttribute[i, j] = false;
                    isInkAttribute[i, j] = false;
                    CurrentInk[i, j] = 7;
                    CurrentPaper[i, j] = 0;
                    isInverse[i, j] = false;
                }
        }

        public void ResetAllAttributes()
        {
            int cInk, cPaper;
            // Sets the attributes
            for (int line = 0; line < PictureDefinitions.MaxLines; line++)
            {
                cInk = 7; cPaper = 0;
                for (int scan = 0; scan < PictureDefinitions.MaxScans; scan++)
                {
                    if (isPaperAttribute[scan, line])
                    {
                        cPaper = CurrentPaper[scan, line];
                    }
                    else
                    {
                        if (isInkAttribute[scan, line])
                        {
                            cInk = CurrentInk[scan, line];
                        }
                        else
                        {
                            CurrentInk[scan, line] = cInk;
                            CurrentPaper[scan, line] = cPaper;
                        }
                    }

                }
            }
        }

        public void SetInk(int inkCode, int scan, int line)
        {
            // Mark scan as containing an attribute
            isInkAttribute[scan, line] = true;
            isPaperAttribute[scan, line] = false;

            // Store attribute
            CurrentInk[scan,line] = inkCode;

            ResetAllAttributes();
           
        }

        public void SetPaper(int paperCode, int scan, int line)
        {
            // Mark scan as containing an attribute
            isInkAttribute[scan, line] = false;
            isPaperAttribute[scan, line] = true;

            // Store attribute
            CurrentPaper[scan, line] = paperCode;

            ResetAllAttributes();

        }

        public void SetInverse(bool value, int scan, int line)
        {
            // Set the scan as Inverse
            isInverse[scan, line] = value;
        }

        public void RemoveAttribute(int scan, int line)
        {
            // Set the scan as Inverse
            isPaperAttribute[scan, line] = false;
            isInkAttribute[scan, line] = false;
            ResetAllAttributes();
        }
    }
}
