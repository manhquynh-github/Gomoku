using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.AI
{
    public class NTree<T>
    {
        public T Value;
        public LinkedList<NTree<T>> Nodes;

        public NTree(T value)
        {
            Value = value;
            Nodes = new LinkedList<NTree<T>>();
        }
    }
}
