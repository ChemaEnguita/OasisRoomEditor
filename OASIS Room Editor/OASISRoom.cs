using System;
using System.Collections.Generic;
using System.Drawing;
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
        public int roomZPlanes { get; private set; }
        public int roomSize { get; set; }
        public WalkBoxManager walkBoxes { get; set; }

        public OASISRoom(int size)
        {
            roomImage = new OricPicture(size, 17*8);
            roomName = "No name";
            roomID = 0;
            roomSize = size;
            roomZPlanes = 0;
            walkBoxes=new WalkBoxManager();
        }

        public OASISRoom(String name, int id, int size)
        {
            roomImage = new OricPicture(size, 17*8);
            roomName = name;
            roomID = id;
            roomSize = size;
            roomZPlanes = 0;
            walkBoxes=new WalkBoxManager();
        }


        public void InsertColumnsLeft(int nCols)
        {
            roomImage.InsertColumnsLeft(nCols);
            roomSize += nCols;
            walkBoxes.MoveLeft(nCols);
        }

        public void InsertColumnsRight(int nCols)
        {
            roomImage.InsertColumnsRight(nCols);
            roomSize += nCols;
        }


        public RoomMemento CreateCheckPoint()
        {
            return new RoomMemento(roomName, roomID, roomSize, roomImage.CreateCheckPoint(), walkBoxes.CreateCheckPoint());
        }

        public void RestoreCheckPoint(RoomMemento memento)
        {
            roomName = String.Copy(memento.roomName);
            roomID = memento.roomID;
            roomSize = memento.roomSize;
            roomImage.RestoreCheckPoint(memento.roomImage);
            walkBoxes.RestoreCheckpoint(memento.walkBoxes);
        }


    }
}
