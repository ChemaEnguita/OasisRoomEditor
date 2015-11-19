using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    // <summary>
    // Class holding info for the walkboxes in a room in OASIS for undo/redo using the memento pattern 
    // <!summary>

    class WalkBoxesMemento
    {
        public List<Rectangle> walkBoxes = new List<Rectangle>();
        public List<WalkBoxManager.WalkBoxProperties> wbProperties = new List<WalkBoxManager.WalkBoxProperties>();

        public WalkBoxesMemento(List<Rectangle> l, List<WalkBoxManager.WalkBoxProperties> p)
        {
            walkBoxes.Clear(); wbProperties.Clear();
            for (int i = 0; i < l.Count(); i++)
            {
                walkBoxes.Add(l[i]);
                wbProperties.Add(p[i]);
            }
        }

    }
}
