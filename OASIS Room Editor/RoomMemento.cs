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
        public string roomName { get; set; }
        public int roomID { get; set; }
        public int roomZPlanes { get; set; }
        public int roomSize { get; set; }

        // Picture data
        public  PictureMemento roomImage;

        // Walkbox data
        public WalkBoxesMemento walkBoxes;

        public RoomMemento(string name, int id, int size, PictureMemento image, WalkBoxesMemento wb)
        {
            roomName = String.Copy(name);
            roomSize = size;
            roomID = id;
            roomImage = image;
            walkBoxes = wb;
        }
    }
}
