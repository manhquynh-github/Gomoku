using System;

namespace Gomoku.Logic
{
  [Flags]
  public enum Orientations : byte
  {
    None = 0,
    Horizontal = 1 << 0,
    Vertical = 1 << 2,

    /// <summary>
    /// Diagonal from top left to bottom right
    /// </summary>
    Diagonal = 1 << 3,

    /// <summary>
    /// Reverse diagonal from top right to bottom left
    /// </summary>
    RvDiagonal = 1 << 4,

    All = Horizontal | Vertical | Diagonal | RvDiagonal
  }
}