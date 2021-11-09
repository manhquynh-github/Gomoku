using System;

namespace Gomoku.Logic
{
  [Flags]
  public enum Directions : byte
  {
    None = 0,
    Left = 1 << 0,
    Up = 1 << 1,
    Right = 1 << 2,
    Down = 1 << 3,
    UpLeft = Left | Up,
    UpRight = Right | Up,
    DownLeft = Left | Down,
    DownRight = Right | Down,
    All = Left | Up | Right | Down,
  }
}