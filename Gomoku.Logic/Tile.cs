using System;
using System.Collections.Generic;

using Gomoku.Logic.AI;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines a tile in the board
  /// </summary>
  public class Tile : IPositional, IDeepCloneable<Tile>, IShallowCloneable<Tile>
  {
    public Tile(int x, int y) : this(x, y, (Piece)Pieces.None)
    {
    }

    private Tile(int x, int y, Piece piece)
    {
      X = x;
      Y = y;
      Piece = piece;
    }

    /// <summary>
    /// The <see cref="Piece"/> this <see cref="Tile"/> currently holds
    /// </summary>
    public Piece Piece { get; set; }

    /// <summary>
    /// The X coordinate of the <see cref="Tile"/>
    /// </summary>
    public int X { get; }

    /// <summary>
    /// The Y coordinate of the <see cref="Tile"/>
    /// </summary>
    public int Y { get; }

    public static bool operator !=(Tile t1, Tile t2)
    {
      return !(t1 == t2);
    }

    public static bool operator ==(Tile t1, Tile t2)
    {
      if (t1 is null && t2 is null)
      {
        return true;
      }

      if (t1 is null || t2 is null)
      {
        return false;
      }

      return t1.X == t2.X
        && t1.Y == t2.Y
        && t1.Piece == t2.Piece;
    }

    public Tile DeepClone()
    {
      return new Tile(X, Y, Piece);
    }

    public override bool Equals(object obj)
    {
      return obj is Tile tile
        && EqualityComparer<Piece>.Default.Equals(Piece, tile.Piece)
        && X == tile.X
        && Y == tile.Y;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Piece, X, Y);
    }

    public Tile ShallowClone()
    {
      return (Tile)MemberwiseClone();
    }

    public override string ToString()
    {
      return Piece.ToString();
    }

    object IDeepCloneable.DeepClone()
    {
      return DeepClone();
    }

    object IShallowCloneable.ShallowClone()
    {
      return ShallowClone();
    }
  }
}