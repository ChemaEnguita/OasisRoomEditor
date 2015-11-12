using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    // <summary>
    // Class representing a room in OASIS
    // <!summary>
    class OASISRoom
    {
        public OricPicture roomImage { get; set; }
        public String roomName { get; set; }
        public int roomID { get; set; }
        public int roomZPlanes { get; set; }
        public int roomSize { get; set; }

        public OASISRoom(int size)
        {
            roomImage = new OricPicture(size, 17);
            roomName = "No name";
            roomID = 0;

        }

        public OASISRoom(String name, int id, int size)
        {
            roomImage = new OricPicture(size, 17);
            roomName = name;
            roomID = id;
        }

        public RoomMemento CreateCheckPoint()
        {
            return new RoomMemento(roomName, roomID, roomSize, roomImage);
        }

        public void RestoreCheckPoint(RoomMemento memento)
        {
            roomName = memento.roomName;
            roomID = memento.roomID;
            roomSize = memento.roomSize;

            roomImage= new OricPicture(memento.nScans,memento.nRows);

            for (int i = 0; i < memento.nScans; i++)
                for (int j = 0; j < memento.nRows; j++)
                {
                    roomImage.SetInverse(memento.Attributes[i, j].isInverse, i, j);
                    if (memento.Attributes[i, j].isPaperAttribute)
                        roomImage.SetPaper(memento.Attributes[i, j].CurrentPaper, i, j);

                    if (memento.Attributes[i, j].isInkAttribute)
                        roomImage.SetPaper(memento.Attributes[i, j].CurrentInk, i, j);
                }

            for (int i = 0; i < memento.nScans * 6; i++)
                for (int j = 0; j < memento.nRows; j++)
                {
                    if (memento.isPixelInk[i, j])
                        roomImage.SetPixel(i, j);
                    else
                        roomImage.ClearPixel(i, j);
                }

            roomImage.ResetAllAttributes();
        }

    }
}
