﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    //<summary>
    // Class to manage walkboxes
    //<!summary>
    class WalkBoxManager
    {
        public struct WalkBoxProperties
        {
            public bool isWalkable;
            public int zPlane;
            public bool isRightCorner;
            public bool isLeftCorner;
            public int Elevation;
        }

        public List<Rectangle> walkBoxes = new List<Rectangle>();
        public List<WalkBoxProperties> wbProperties = new List<WalkBoxProperties>();
        public byte[,] walkMatrix { get; private set; }


        public List<Rectangle> GetBoxes()
        {
            return walkBoxes;
        }

        public int GetNumBoxes()
        {
            return walkBoxes.Count();
        }
  
        public void Add(Rectangle wb)
        {
            walkBoxes.Add(wb);
            WalkBoxProperties pr;

            pr.isWalkable = true;
            pr.zPlane = 0;
            pr.isRightCorner = false;
            pr.isLeftCorner = false;
            pr.Elevation = 0;
                        
            wbProperties.Add(pr);
        }

        public bool Remove(int i)
        {
            if (i < walkBoxes.Count())
            {
                walkBoxes.RemoveAt(i);
                wbProperties.RemoveAt(i);
                return true;
            }
            else return false;
        }

        public void Clear()
        {
            walkBoxes.Clear();
            wbProperties.Clear();
        }

        public Rectangle GetBox(int i)
        {
            return walkBoxes[i];
        }

        public WalkBoxProperties GetProperties(int i)
        {
            return wbProperties[i];
        }

        public bool SetProperties(int i, WalkBoxProperties p)
        {
            if (i < walkBoxes.Count())
            {
                wbProperties[i] = p;
                return true;
            }
            else return false;
        }

        public void MoveLeft(int nCols)
        {
            //var tempList= new List<Rectangle>();
            for (int i = 0; i < walkBoxes.Count; i++)
            {
                var r = walkBoxes[i];
                r.Location = new Point(r.Left + (nCols * 6), r.Top);
                //tempList.Add(r);
                walkBoxes[i] = r;
            }
            //walkBoxes = tempList;
        }

        public void MoveRight(int nCols)
        {
            var tempList = new List<Rectangle>();
            for (int i = 0; i < walkBoxes.Count; i++)
            {
                var r = walkBoxes[i];
                r.Location = new Point(r.Left - (nCols * 6), r.Top);
                tempList.Add(r);
            }
            walkBoxes = tempList;
        }

        public void ChangeWalkbox(int index, Rectangle r)
        {
            if (index < walkBoxes.Count())
            {
                walkBoxes[index] = r;
            }
        }

        private bool areBoxesNeighbours(int i, int j)
        {
            Rectangle r1 = walkBoxes[i];
            Rectangle r2 = walkBoxes[j];


            /* META: Maybe we should not consider walkability here, as it is checked in the engine!!! */
            if ((!wbProperties[i].isWalkable) || (!wbProperties[j].isWalkable))
                return false;

            return Rectangle.Intersect(r1, r2) != Rectangle.Empty;
        }

        public void CreateWalkMatrix()
        {
            /* Creates the walk matrix from the walkbox data */
            /* Adapted from the original code in scummvm     */
            /* void ScummEngine::calcItineraryMatrix(byte *itineraryMatrix, int num) */

            byte i, j, k;
            int num = walkBoxes.Count();

            byte InvalidBox = 0x80;

            // Needs an auxiliar matrix, the adjacentMatrix
            byte[,] adjacentMatrix = new byte[num, num];

            // Create the matrix
            walkMatrix = new byte[num, num];

            // Initialise the adjacent matrix: each box has distance 0 to itself,
            // and distance 1 to its direct neighbors. Initially, it has distance
            // 255 (= infinity) to all other boxes.
            for (i = 0; i < num; i++)
            {
                for (j = 0; j < num; j++)
                {
                    if (i == j)
                    {
                        adjacentMatrix[i, j] = 0;
                        walkMatrix[i, j] = j;
                    }
                    else if (areBoxesNeighbours(i, j))
                    {
                        adjacentMatrix[i, j] = 1;
                        walkMatrix[i, j] = j;
                    }
                    else
                    {
                        adjacentMatrix[i, j] = 255;
                        walkMatrix[i, j] = InvalidBox;
                    }
                }
            }

            // Compute the shortest routes between boxes via Kleene's algorithm.
            // The original code used some kind of mangled Dijkstra's algorithm;
            // while that might in theory be slightly faster, it was
            // a) extremly obfuscated
            // b) incorrect: it didn't always find the shortest paths
            // c) not any faster in reality for our sparse & small adjacent matrices
            for (k = 0; k < num; k++)
            {
                for (i = 0; i < num; i++)
                {
                    for (j = 0; j < num; j++)
                    {
                        if (i == j)
                            continue;
                        byte distIK = adjacentMatrix[i, k];
                        byte distKJ = adjacentMatrix[k, j];
                        if (adjacentMatrix[i, j] > distIK + distKJ)
                        {
                            adjacentMatrix[i, j] = (byte)(distIK + distKJ);
                            walkMatrix[i, j] = walkMatrix[i, k];
                        }
                    }
                }
            }
        }

        // Undo/redo following the memento pattern
        public WalkBoxesMemento CreateCheckPoint()
        {
            return (new WalkBoxesMemento(walkBoxes, wbProperties));
        }

        public void RestoreCheckpoint(WalkBoxesMemento m)
        {
            walkBoxes.Clear(); wbProperties.Clear();
            for (int i=0; i< m.walkBoxes.Count();i++)
            {
                walkBoxes.Add(m.walkBoxes[i]);
                wbProperties.Add(m.wbProperties[i]);
            }
           // CreateWalkMatrix();
        }

        // Save/load
        public void SaveWalkboxes(BinaryWriter w)
        {
            w.Write(walkBoxes.Count);
            for (int i = 0; i < walkBoxes.Count(); i++)
            {
                w.Write(walkBoxes[i].X);
                w.Write(walkBoxes[i].Y);
                w.Write(walkBoxes[i].Width);
                w.Write(walkBoxes[i].Height);

                w.Write(wbProperties[i].Elevation);
                w.Write(wbProperties[i].isLeftCorner);
                w.Write(wbProperties[i].isRightCorner);
                w.Write(wbProperties[i].isWalkable);
                w.Write(wbProperties[i].zPlane);
            }
        }

        public void LoadWalkboxes(BinaryReader r)
        {
            walkBoxes.Clear();
            wbProperties.Clear();

            var num = r.ReadInt32();
            for(int i=0; i< num;i++)
            {
                walkBoxes.Add(new Rectangle(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()));

                WalkBoxProperties p;
                p.Elevation = r.ReadInt32();
                p.isLeftCorner = r.ReadBoolean();
                p.isRightCorner = r.ReadBoolean();
                p.isWalkable = r.ReadBoolean();
                p.zPlane = r.ReadInt32();

                wbProperties.Add(p);
            }

        }


    }

}
