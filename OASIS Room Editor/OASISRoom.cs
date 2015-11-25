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
            if (size == 0)
            {
                roomImage = null;
            }
            else
                roomImage = new OricPicture(size, 17 * 8);
            roomName = "No name";
            roomID = 0;
            roomSize = size;
            roomZPlanes = 0;
            walkBoxes = new WalkBoxManager();
        }

        public OASISRoom(String name, int id, int size)
        {
            roomImage = new OricPicture(size, 17 * 8);
            roomName = name;
            roomID = id;
            roomSize = size;
            roomZPlanes = 0;
            walkBoxes = new WalkBoxManager();
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
                m = new RoomMemento(roomName, roomID, roomSize, roomImage.CreateCheckPoint(), walkBoxes.CreateCheckPoint());
            else
                m = new RoomMemento(roomName, roomID, roomSize, null, walkBoxes.CreateCheckPoint());

            return m;
        }

        public void RestoreCheckPoint(RoomMemento memento)
        {
            roomName = String.Copy(memento.roomName);
            roomID = memento.roomID;
            roomSize = memento.roomSize;

            //Is an image included in the memento)?
            if (memento.roomImage != null)
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

            if (roomImage != null)
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
            var ims = r.ReadInt32();

            roomImage = new OricPicture(ims, imr);
            roomImage.ReadHiresData(r);

            walkBoxes.LoadWalkboxes(r);

            r.Close();

        }


        private TileSectioner SectionInTiles()
        {
            var ts = new TileSectioner();

            var p = roomImage.EncodeAsHires();
            ts.doSection(p, true);

            return ts;
        }

        public void ExportAsResource(string fileName)
        {
            TileSectioner ts = SectionInTiles();

            System.IO.StreamWriter rf = new System.IO.StreamWriter(fileName); // add 2nd param true so it can append to a file

            rf.Write(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;\r\n");
            rf.Write("; Room: "+roomName+ "\r\n");
            rf.Write(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;\r\n");

            /*
            Resources following XA are:
             .(
                .byt <TYPE>    
                .word res_end-res_start + 4
                .byt <ID>
                res_start
                .byt <resource data>
                res_end
            .)
            */

            /* Exporting as resource implies the following format: 
             - A header including type (1 byte), length (2 bytes), id (1 byte) */

            rf.Write(String.Format(".(\r\r\n.byt RESOURCE_ROOM\r\r\n.word (res_end - res_start + 4)\r\r\n.byt {0:d}\r\r\nres_start\r\r\n", roomID));

            /*
             - Data with the following contents:
             - Number of columns
             - Offset to column map
             - Offset to tiles
             - Number of zplanes
             - Offset to zplane 1 map
             - Offset to zplane 1 tiles
             ... */

            rf.Write("; No. columns, offset to tile map, offset to tiles\r\n");
            rf.Write(String.Format(".byt {0:d}, <(column_data-res_start), >(column_data-res_start), <(tiles_start-res_start), >(tiles_start-res_start)\r\n", roomSize-2));

            int roomNZplanes = 0;
            rf.Write("; No. zplanes and offsets to zplanes\r\n");
            rf.Write(String.Format(".byt {0:d}\r\n", roomNZplanes));
            for (int i = 0; i < roomNZplanes; i++)
            {
                rf.Write(String.Format(".byt <(zplane{0:d}_data-res_start), >(zplane{0:d}_data-res_start)\r\n", i + 1));
                rf.Write(String.Format(".byt <(zplane{0:d}_tiles-res_start), >(zplane{0:d}_tiles-res_start)\r\n", i + 1));
            }

            /*
             - Number of walkboxes
             - Offset to walkbox data
             - Offset to walkbox matrix */

            rf.Write("; No. Walkboxes and offsets to wb data and matrix\r\n");
            rf.Write(String.Format(".byt {0:d}, <(wb_data-res_start), >(wb_data-res_start), <(wb_matrix-res_start), >(wb_matrix-res_start)\r\n", walkBoxes.GetNumBoxes()));


            /*
             - Offset to palette information
             - Entry Script ID
             - Exit Script ID
             - Number of objects in room
             - ID object 1
             - ...
             - Name (null-terminated string) */

            rf.Write("; Offset to palette\r\n");
            rf.Write(".byt <(palette-res_start), >(palette-res_start)\r\n");
            rf.Write("; Entry and exit scripts\r\n");
            rf.Write(String.Format(".byt {0:d}, {1:d}\r\n", 255, 255));
            rf.Write("; Number of objects in the room and list of ids\r\n");
            rf.Write(String.Format(".byt {0:d}\r\n", 0));
            rf.Write("; Room name (null terminated)\r\n");
            rf.Write(".asc \""+roomName+"\", 0\r\n");

            /*
            - Room tile map
            - Room tile set
            */

            rf.Write("; Room tile map\r\n");
            rf.Write("column_data");
            for (int r = 0; r < ts.tileMap.GetLength(0); r++)
            {
                rf.Write("\r\n.byt ");
                for (int c = 0; c < ts.tileMap.GetLength(1); c++)
                {
                    if (c > 0) rf.Write(", ");
                    rf.Write(String.Format("{0:D3}", (int)(ts.tileMap[r, c])));
                }
            }

            rf.Write("\r\n\r\n; Room tile set\r\n");
            rf.Write("\r\ntiles_start\r\n");
            for (int r = 0+1; r < ts.tileSet.Count(); r++) // Tile 0 is not stored
            {
                var tile = ts.tileSet[r];
                rf.Write("\r\n.byt ");
                for (int c = 0; c < 8; c++)
                {
                    if (c > 0) rf.Write(", ");
                    rf.Write(String.Format("${0:X2}", (int)(tile[c])));
                }

                rf.Write(String.Format("\t; tile #{0:d}", r));
            }


            /*    
            - Zplane1 tile map
            - Zplane1 tile set
            - ...
            */

            /*
            - Walkbox data, 5 bytes per box:
              col-min, col-max, row-min, row-max, flags:
                ;               76543210
                ;			   	|||||\_/
                ;			   	||||| |
                ;			   	||||| +- z-plane
                ;			   	|||||
                ;			   	||||+- free
                ;			   	|||+- free
                ;			   	||+- left corner
                ;			   	|+- right corner
                ;				+- not walkable

            - Walkbox matrix n x n bytes (n is number of walkboxes)
            */


            /*
            - Palette information 17*8*2 bytes
            */

            rf.Write("\r\nres_end\r\n,)\r\n");
            rf.Close();
        }
        #endregion
    }

    }
