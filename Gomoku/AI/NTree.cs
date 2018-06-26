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
        public List<NTree<T>> Nodes;

        public NTree(T value)
        {
            Value = value;
            Nodes = new List<NTree<T>>();
        }
    }
}
