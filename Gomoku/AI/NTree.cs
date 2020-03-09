using System.Collections.Generic;

namespace Gomoku.AI
{
  public class NTree<T>
  {
    public List<NTree<T>> Nodes;
    public T Value;
    public NTree<T> ParentNode;

    public NTree(NTree<T> parentNode, T value)
    {
      ParentNode = parentNode;
      Value = value;
      Nodes = new List<NTree<T>>();
    }

    public NTree(T value) : this(null, value)
    {
    }
  }
}