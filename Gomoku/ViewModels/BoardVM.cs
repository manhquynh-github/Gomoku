using System;

using Gomoku.Logic;

namespace Gomoku.ViewModels
{
  public class BoardVM
  {
    public BoardVM(Board board)
    {
      Board = board ?? throw new ArgumentNullException(nameof(board));

      TileVMs = new TileVM[board.Width, board.Height];
      for (int i = 0; i < board.Width; i++)
      {
        for (int j = 0; j < board.Height; j++)
        {
          TileVMs[i, j] = new TileVM(board[i, j]);
        }
      }
    }

    public Board Board { get; }
    private TileVM[,] TileVMs { get; }
    public TileVM this[int x, int y] => TileVMs[x, y];
    public TileVM this[Tile t] => this[t.X, t.Y];

    public void Clear(Tile tile)
    {
      var tileVM = this[tile];
      tileVM.IsHighlighted = false;
      tileVM.Piece = (Piece)Pieces.None;
    }

    public void Set(Tile tile)
    {
      var tileVM = this[tile];
      tileVM.IsHighlighted = true;
      tileVM.Piece = tile.Piece;
    }
  }
}