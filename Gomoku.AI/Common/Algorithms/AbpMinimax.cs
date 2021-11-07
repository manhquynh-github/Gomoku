using System;
using System.Collections.Generic;
using System.Text;

using Gomoku.AI.Common.DataStructures;

namespace Gomoku.AI.Common.Algorithms
{
  public class AbpMinimax<T>
  {
    protected readonly Func<NTree<T>, double> _evaluateFunc;

    public AbpMinimax(Func<NTree<T>, double> evaluateFunc)
    {
      _evaluateFunc = evaluateFunc
        ?? throw new ArgumentNullException(nameof(evaluateFunc));
    }

    public double Evaluate(
      NTree<T> node,
      int depth,
      double alpha = double.MinValue,
      double beta = double.MaxValue,
      bool isMaximizing = true)
    {
      if (depth == 0 || node.Nodes.Count == 0)
      {
        return _evaluateFunc(node);
      }

      if (isMaximizing)
      {
        var maxValue = double.MinValue;
        foreach (NTree<T> childNode in node.Nodes)
        {
          var value = Evaluate(childNode, depth - 1, alpha, beta, false);
          maxValue = Math.Max(maxValue, value);
          alpha = Math.Max(alpha, value);
          if (beta <= alpha)
          {
            return maxValue;
          }
        }
        return maxValue;
      }
      else
      {
        var minValue = double.MaxValue;
        foreach (NTree<T> childNode in node.Nodes)
        {
          var value = Evaluate(childNode, depth - 1, alpha, beta, true);
          minValue = Math.Min(minValue, value);
          beta = Math.Min(beta, value);
          if (beta <= alpha)
          {
            return minValue;
          }
        }
        return minValue;
      }
    }
  }
}