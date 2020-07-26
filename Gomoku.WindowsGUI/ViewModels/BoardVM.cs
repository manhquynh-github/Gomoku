using System;

using Gomoku.Logic;

namespace Gomoku.WindowsGUI.ViewModels
{
  public class BoardVM
  {
    public BoardVM(Board board)
    {
      Board = board ?? throw new ArgumentNullException(nameof(board));

      TileVMs = new TileVM[board.Width, board.Height];
      for (var i = 0; i < board.Width; i++)
      {
        for (var j = 0; j < board.Height; j++)
        {
          TileVMs[i, j] = new TileVM(board[i, j]);
        }
      }
    }

    public Board Board { get; }
    private TileVM[,] TileVMs { get; }
    public TileVM this[int x, int y] => TileVMs[x, y];

    public void Clear(int x, int y)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      TileVM tileVM = this[x, y];
      tileVM.IsHighlighted = false;
      tileVM.Piece = (Piece)Pieces.None;
    }

    public void Set(int x, int y, Piece piece)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      TileVM tileVM = this[x, y];
      tileVM.IsHighlighted = true;
      tileVM.Piece = piece;
    }
  }
}