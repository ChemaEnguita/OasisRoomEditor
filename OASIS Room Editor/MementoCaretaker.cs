using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    // <summary>
    // Class manages the stacks for undo/redo using the memento design pattern 
    // <!summary>

    class MementoCaretaker
    {
        private Stack<RoomMemento> undoStack =new Stack<RoomMemento>();
        private Stack<RoomMemento> redoStack=new Stack<RoomMemento>();

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public RoomMemento Undo()
        {
            if (undoStack.Count != 0)
            {
                RoomMemento m = undoStack.Pop();
                redoStack.Push(m);
                return m;
            }
            else
                return null;
        }

        public RoomMemento Redo()
        {
            if (redoStack.Count != 0)
            {
                RoomMemento m = redoStack.Pop();
                undoStack.Push(m);
                return m;
            }
            else
                return null;
        }

        public void NewCheckPoint(RoomMemento rm)
        {
            undoStack.Push(rm);
        }

    }
}
