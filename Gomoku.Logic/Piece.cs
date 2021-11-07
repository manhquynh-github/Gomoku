namespace Gomoku.Logic
{
  public enum Pieces
  {
    None,
    X,
    O,
    Y
  };

  /// <summary>
  /// Defines a piece.
  /// </summary>
  public struct Piece
  {
    public Piece(byte typeIndex)
    {
      TypeIndex = typeIndex;
    }

    public Piece(Pieces pieces) :
      this((byte)pieces)
    {
    }

    public Pieces Type => this;
    public byte TypeIndex { get; }

    public static explicit operator Piece(byte i)
    {
      return new Piece(i);
    }

    public static explicit operator Piece(Pieces p)
    {
      return new Piece(p);
    }

    public static implicit operator byte(Piece p)
    {
      return p.TypeIndex;
    }

    public static implicit operator Pieces(Piece p)
    {
      return (Pieces)p.TypeIndex;
    }

    public static bool operator !=(Piece p1, Piece p2)
    {
      return p1.TypeIndex != p2.TypeIndex;
    }

    public static bool operator !=(Piece p1, Pieces p2)
    {
      return p1.TypeIndex != (byte)p2;
    }

    public static bool operator ==(Piece p1, Piece p2)
    {
      return p1.TypeIndex == p2.TypeIndex;
    }

    public static bool operator ==(Piece p1, Pieces p2)
    {
      return p1.TypeIndex == (byte)p2;
    }

    public override bool Equals(object obj)
    {
      return obj is Piece piece
        && TypeIndex == piece.TypeIndex;
    }

    public override int GetHashCode()
    {
      return 1970110603 + TypeIndex.GetHashCode();
    }

    public override string ToString()
    {
      return TypeIndex.ToString();
    }
  }
}