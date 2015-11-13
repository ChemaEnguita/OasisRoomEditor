using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    // <summary>
    // Class holding info for an image in OASIS for undo/redo using the memento pattern 
    // <!summary>

    class PictureMemento
    {
        // Picture data
        public int nScans { get; set; }
        public int nRows { get; set; }
        public Attribute[,] Attributes;
        public bool[,] isPixelInk;

        public PictureMemento(int scans, int rows, Attribute[,] attr, bool[,] pi)
        {
            nScans = scans;
            nRows = rows;
            Attributes = attr;
            isPixelInk = pi;
        }
    }
}
