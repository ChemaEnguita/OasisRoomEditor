using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OASIS_Room_Editor
{
    // <summary>
    // Dropout Stack. A stack with a fixed size, which 
    // drops the oldest elements if necessary
    // <!summary>

    public class DropOutStack<T> : LinkedList<T>
    {
        private readonly int _maxSize;
        public DropOutStack(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Push(T item)
        {
            this.AddFirst(item);

            if (this.Count > _maxSize)
                this.RemoveLast();
        }

        public T Pop()
        {
            var item = this.First.Value;
            this.RemoveFirst();
            return item;
        }

        public bool IsEmpty()
        {
            return (this.Count == 0);
        }
    }
}
