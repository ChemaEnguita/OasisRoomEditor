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


        private TileSectioner SectionInTiles(bool trim1=true, bool trim2=true)
        {
            var ts = new TileSectioner();

            var p = roomImage.EncodeAsHires();
            ts.doSection(p, trim1, trim2);

            return ts;
        }

        public void ExportAsResource(string fileName, bool trimCol1=true, bool trimCol2=true, bool includePalette=false)
        {
            TileSectioner ts = SectionInTiles(trimCol1, trimCol2);

            System.IO.StreamWriter rf = new System.IO.StreamWriter(fileName); // add 2nd param true so it can append to a file

            rf.WriteLine(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            rf.WriteLine("; Room: "+roomName);
            rf.WriteLine(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");

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

            rf.Write(String.Format(".(\r\n\t.byt RESOURCE_ROOM\r\n\t.word (res_end - res_start + 4)\r\n\t.byt {0:d}\r\nres_start\r\n", roomID));

            /*
             - Data with the following contents:
             - Number of columns
             - Offset to column map
             - Offset to tiles
             - Number of zplanes
             - Offset to zplane 1 map
             - Offset to zplane 1 tiles
             ... */

            rf.WriteLine("; No. columns, offset to tile map, offset to tiles");
            rf.WriteLine(String.Format("\t.byt {0:d}, <(column_data-res_start), >(column_data-res_start), <(tiles_start-res_start), >(tiles_start-res_start)", roomSize-2));

            int roomNZplanes = 0;
            rf.WriteLine("; No. zplanes and offsets to zplanes");
            rf.WriteLine(String.Format("\t.byt {0:d}", roomNZplanes));
            for (int i = 0; i < roomNZplanes; i++)
            {
                rf.WriteLine(String.Format("\t.byt <(zplane{0:d}_data-res_start), >(zplane{0:d}_data-res_start)", i + 1));
                rf.WriteLine(String.Format("\t.byt <(zplane{0:d}_tiles-res_start), >(zplane{0:d}_tiles-res_start)", i + 1));
            }

            /*
             - Number of walkboxes
             - Offset to walkbox data
             - Offset to walkbox matrix */

            rf.WriteLine("; No. Walkboxes and offsets to wb data and matrix");
            rf.WriteLine(String.Format("\t.byt {0:d}, <(wb_data-res_start), >(wb_data-res_start), <(wb_matrix-res_start), >(wb_matrix-res_start)", walkBoxes.GetNumBoxes()));


            /*
             - Offset to palette information
             - Entry Script ID
             - Exit Script ID
             - Number of objects in room
             - ID object 1
             - ...
             - Name (null-terminated string) */

            rf.WriteLine("; Offset to palette");
            if(includePalette)
                rf.WriteLine("\t.byt <(palette-res_start), >(palette-res_start)");
            else
                rf.WriteLine("\t.byt 0, 0\t; No palette information");

            rf.WriteLine("; Entry and exit scripts");
            rf.WriteLine(String.Format("\t.byt {0:d}, {1:d}", 255, 255));
            rf.WriteLine("; Number of objects in the room and list of ids");
            rf.WriteLine(String.Format("\t.byt {0:d}", 0));
            rf.WriteLine("; Room name (null terminated)");
            rf.WriteLine("\t.asc \""+roomName+"\", 0");

            /*
            - Room tile map
            - Room tile set
            */

            rf.WriteLine("; Room tile map");
            rf.Write("column_data");
            for (int r = 0; r < ts.tileMap.GetLength(0); r++)
            {
                rf.Write("\r\n\t.byt ");
                for (int c = 0; c < ts.tileMap.GetLength(1); c++)
                {
                    if (c > 0) rf.Write(", ");
                    rf.Write(String.Format("{0:D3}", (int)(ts.tileMap[r, c])));
                }
            }

            rf.WriteLine("\r\n\r\n; Room tile set");
            rf.Write("tiles_start");
            for (int r = 1; r < ts.tileSet.Count(); r++) // Tile 0 is not stored
            {
                var tile = ts.tileSet[r];
                rf.Write("\r\n\t.byt ");
                for (int c = 0; c < 8; c++)
                {
                    if (c > 0) rf.Write(", ");
                    rf.Write(String.Format("${0:X2}", (int)(tile[c])));
                }

                rf.Write(String.Format("\t; tile #{0:d}", r));
            }

            rf.WriteLine("");

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

            rf.WriteLine("; Walkbox Data");
            rf.WriteLine("wb_data");
            int flags;
            WalkBoxManager.WalkBoxProperties p;
            Rectangle wbr;
            int skip = 0;

            if (trimCol1) skip++;
            if (trimCol2) skip++;

            for(int i=0; i < walkBoxes.GetNumBoxes(); i++)
            {
                p= walkBoxes.GetProperties(i);
                flags = 0;
                if (p.isLeftCorner)     flags |= 0x20;
                if (p.isRightCorner)    flags |= 0x40;
                if (!p.isWalkable)      flags |= 0x80;
                flags |= p.zPlane & 0x7;

                wbr = walkBoxes.GetBox(i);
                /* if(trim2Cols)
                     rf.WriteLine("\t.byt {0:D3}, {1:D3}, {2:D3}, {3:D3}, ${4:x2}", wbr.Left / 6 - 2, (wbr.Right - 1) / 6 - 2, wbr.Top / 8, (wbr.Bottom - 1) / 8, flags);
                 else
                     rf.WriteLine("\t.byt {0:D3}, {1:D3}, {2:D3}, {3:D3}, ${4:x2}", wbr.Left/6, (wbr.Right-1)/6, wbr.Top/8, (wbr.Bottom-1)/8, flags);
                */
                rf.WriteLine("\t.byt {0:D3}, {1:D3}, {2:D3}, {3:D3}, ${4:x2}", wbr.Left / 6 - skip, (wbr.Right - 1) / 6 - skip, wbr.Top / 8, (wbr.Bottom - 1) / 8, flags);
            }

            // Now the walk matrix
            rf.WriteLine("; Walk matrix");
            rf.WriteLine("wb_matrix");
            walkBoxes.CreateWalkMatrix();
            String s = "";
            int n = walkBoxes.walkMatrix.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                s += "\t.byt ";
                for (int j = 0; j < n; j++)
                {
                    s += walkBoxes.walkMatrix[i, j].ToString()+ (j==(n-1)?"":", ");
                }
                s += "\r\n";
            }

            rf.Write(s);


            /*
            - Palette information 17*8*2 bytes
            */

            if(includePalette)
            {
                rf.WriteLine("; Palette Information is stored as one column only for now...");
                rf.WriteLine("; Palette");
                rf.WriteLine("palette");

                for (int i = 0; i < 17 * 8; i++)
                {
                    if (i % 16 == 0)
                        s += ".byt ";
                    s += roomImage.GetScanInkCode(0, i);
                    if (i % 16 != 15)
                    {
                      if (i < (17 * 8 - 1)) s += ", ";
                    }
                    else
                        s += "\r\n";
                }
                rf.WriteLine(s);
            }
            rf.WriteLine("\r\n\r\nres_end\r\n.)");
            rf.Close();
        }
        #endregion
    }

    }
