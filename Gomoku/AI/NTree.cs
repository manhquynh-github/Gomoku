using System.Collections.Generic;

namespace Gomoku.AI
{
  public class NTree<T>
  {
    public List<NTree<T>> Nodes;
    public T Value;

    public NTree(T value)
    {
      Value = value;
      Nodes = new List<NTree<T>>();
    }
  }
}