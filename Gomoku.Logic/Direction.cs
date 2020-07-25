using System;

namespace Gomoku.Logic
{
  [Flags]
  public enum Direction
  {
    Left = 1,
    Up = 2,
    Right = 4,
    Down = 8,
    UpLeft = Left | Up,
    UpRight = Right | Up,
    DownLeft = Left | Down,
    DownRight = Right | Down,
    All = Left | Up | Right | Down
  }
}