namespace Gomoku.BoardNS
{
  public enum Pieces
  {
    None,
    X,
    O,
  };

  /// <summary>
  /// Defines a piece.
  /// </summary>
  public class Piece : VMBase
  {
    private int _typeIndex;

    public Piece(int typeIndex)
    {
      TypeIndex = typeIndex;
    }

    public Piece(Pieces pieces) :
      this((int)pieces)
    {
    }

    public Pieces Type
    {
      get => (Pieces)TypeIndex;
      set
      {
        TypeIndex = (int)value;
        NotifyPropertyChanged();
      }
    }

    public int TypeIndex
    {
      get => _typeIndex;
      set => SetProperty(ref _typeIndex, value);
    }
  }
}