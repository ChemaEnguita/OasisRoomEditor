using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    //<summary>
    // Class to section a picture in tiles. Works with any
    // array of bytes, and just needs to know the number of
    // rows
    //<!summary>
    class TileSectioner
    {
        public List<byte[]> tileSet { get; private set; }
        public byte[,] tileMap { get; private set; }

        // Constructor. Pass the value for a blank tile
        // usually 0x40 or 0x7f
        public TileSectioner(byte blankTile=0x40)
        {
            tileSet = new List<byte[]>();
            byte[] t=new byte[8];

            for (int i=0;i<8;i++)
                t[i] = blankTile;

            tileSet.Add(t);
        }

        // Check if two tiles are equal
        private bool areTilesEqual(byte []t1, byte[] t2)
        {
            int i=0;
            while((i<8) && (t1[i]==t2[i]))
                i++;
            return !(i==8);
        }

        // Searches for tile t in the tile set and return its
        // code. If t is not in the set add it and return code.
        private byte GetTileCode(byte[] t)
        {
            // Search the code in the tile set
            int code = 0;
            while ((code < tileSet.Count) && (areTilesEqual(tileSet[code],t)))
                code++;
            if(code==tileSet.Count)
                tileSet.Add(t);

            return (byte)(code);
        }

        // Method to perform the sectioning of a picture in tiles.
        // Fills in the tilemap and tileset.
        public void doSection(byte[,] pic, bool usePalette=false)
        {
            // Section the picture in tiles
            tileMap = new byte[pic.GetLength(0), pic.GetLength(1) / 8];

            for (int tiley = 0; tiley < tileMap.GetLength(1); tiley++) 
                for (int tilex= (usePalette?2:0) ; tilex < tileMap.GetLength(0); tilex++)
                {
                    // Calculate the tile at (tilex,tiley)
                    var t = new byte[8];
                    for (int k=0; k<8; k++)
                    {
                        t[k] = pic[tilex, tiley*8+ k];
                    }
                    // Get entry in array (creates one if tile does not exist)
                    var code=GetTileCode(t);
                    // Store in the tilemap
                    tileMap[tilex, tiley] = code;
                }
        }

    }
}
