using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            if(size==0)
            {
                roomImage = null;
            }
            else
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


        public RoomMemento CreateCheckPoint(bool includeImage = true)
        {
            RoomMemento m;

            if (includeImage)
                m= new RoomMemento(roomName, roomID, roomSize, roomImage.CreateCheckPoint(), walkBoxes.CreateCheckPoint());
            else
                m= new RoomMemento(roomName, roomID, roomSize, null, walkBoxes.CreateCheckPoint());

            return m;
        }

        public void RestoreCheckPoint(RoomMemento memento)
        {
            roomName = String.Copy(memento.roomName);
            roomID = memento.roomID;
            roomSize = memento.roomSize;

            //Is an image included in the memento)?
            if (memento.roomImage!= null)
                roomImage.RestoreCheckPoint(memento.roomImage);
            walkBoxes.RestoreCheckpoint(memento.walkBoxes);
        }


        #region saving&loading

        public void SaveOASISRoom(string fileName)
        {
            // Create the writer for data.
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryWriter w = new BinaryWriter(fs);

            w.Write(roomName);
            w.Write(roomID);
            w.Write(roomSize);
            w.Write(roomZPlanes);
            
            if(roomImage!=null)
            {
                w.Write(roomImage.nRows);
                w.Write(roomImage.nScans);
                roomImage.WriteHiresData(w);
            }

            walkBoxes.SaveWalkboxes(w);
            w.Close();

        }


        public void LoadOASISRoom(string fileName)
        {
            // Create the reader for data.
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            BinaryReader r = new BinaryReader(fs);

            roomName = r.ReadString();
            roomID = r.ReadInt32();
            roomSize = r.ReadInt32(); 
            roomZPlanes = r.ReadInt32();

            var imr = r.ReadInt32();
            var ims= r.ReadInt32();

            roomImage = new OricPicture(ims, imr);
            roomImage.ReadHiresData(r);

            walkBoxes.LoadWalkboxes(r);
            
            r.Close();

        }


        #endregion

    }
}
