using System;

using Gomoku.Logic;

namespace Gomoku.WindowsGUI.ViewModels
{
  /// <summary>
  /// Defines a tile in the board
  /// </summary>
  public class TileVM : VMBase
  {
    private bool _isHighlighted;

    public TileVM(Tile tile)
    {
      Tile = tile ?? throw new ArgumentNullException(nameof(tile));
    }

    public bool IsHighlighted
    {
      get => _isHighlighted;
      set => SetProperty(ref _isHighlighted, value);
    }

    public Piece Piece
    {
      get => Tile.Piece;
      set
      {
        Tile.Piece = value;
        NotifyPropertyChanged();
      }
    }

    public Tile Tile { get; }
  }
}