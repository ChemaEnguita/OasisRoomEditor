using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    // <summary>
    // Class holding info for a Room in OASIS for undo/redo using the memento pattern 
    // <!summary>
    class RoomMemento
    {
        public String roomName { get; set; }
        public int roomID { get; set; }
        public int roomZPlanes { get; set; }
        public int roomSize { get; set; }

        // Picture data
        public int nScans { get; set; }
        public int nRows { get; set; }
        public Attribute[,] Attributes;
        public bool[,] isPixelInk;


        public RoomMemento(String name, int id, int size, OricPicture image)
        {
            roomName = name;
            roomSize = size;
            roomID = id;

            nScans = image.nScans;
            nRows = image.nRows;
            Attributes = new Attribute[nScans, nRows];
            for (int i=0; i<nScans; i++)
                for (int j=0; j<nRows; j++)
                {
                    Attributes[i, j] = new Attribute();
                    Attributes[i, j].CurrentInk = image.GetScanInkCode(i, j);
                    Attributes[i, j].CurrentPaper = image.GetScanPaperCode(i, j);
                    Attributes[i, j].isInkAttribute = image.isInkAttribute(i, j);
                    Attributes[i, j].isPaperAttribute = image.isPaperAttribute(i, j);
                    Attributes[i, j].isInverse = image.isInverse(i, j);
                }

            isPixelInk = new bool[nScans*6, nRows];
            for(int i=0; i<nScans*6; i++)
                for(int j=0; j<nRows; j++)
                {
                    isPixelInk[i,j]=(image.GetPixel(i,j)==1);
                }
        }
    }
}
