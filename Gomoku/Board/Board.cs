using System;
using System.Collections.Generic;
using System.Windows;

namespace Gomoku.Board
{
    public class Board : ICloneable
    {
        public readonly int Height;
        public readonly int Width;
        public readonly Tile[,] Tiles;
        public readonly List<Player> Players;
        private int _turn;
        private bool _isGameOver;

        public static readonly int WINPIECES = 5;

        public int Turn
        {
            get => _turn;
            set
            {
                _turn = value;
                TurnChanged?.Invoke(new TurnChangedEventArgs(this, _turn, GetCurrentPlayer()));
            }
        }

        public bool IsGameOver
        {
            get => _isGameOver;
            private set
            {
                _isGameOver = value;
                if (value == true && GameOver != null)
                {
                    GameOver.Invoke(new GameOverEventArgs(this, true, Turn, GetCurrentPlayer()));
                }
            }
        }

        public event TurnChangedEventHandler TurnChanged;
        public event GameOverEventHandler GameOver;

        public Board(int width, int height, IList<Player> players)
        {
            if (height <= 5 || width <= 5)
            {
                throw new ArgumentException();
            }

            Width = width;
            Height = height;
            Tiles = new Tile[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Tiles[i, j] = new Tile(i, j);
            Players = new List<Player>(players);
            Turn = 0;
            IsGameOver = false;
        }

        public void RunTilesFunction(
            Tile tile,
            Direction direction,
            Predicate<Tile> predicate)
        {
            switch (direction)
            {
                case Direction.Left:
                    for (int i = tile.X - 1, j = tile.Y;
                        i >= 0 && predicate(Tiles[i, j]);
                        i--) ;
                    break;
                case Direction.Right:
                    for (int i = tile.X + 1, j = tile.Y;
                        i < Width && predicate(Tiles[i, j]);
                        i++) ;
                    break;
                case Direction.Up:
                    for (int i = tile.X, j = tile.Y - 1;
                        j >= 0 && predicate(Tiles[i, j]);
                        j--) ;
                    break;
                case Direction.Down:
                    for (int i = tile.X, j = tile.Y + 1;
                        j < Height && predicate(Tiles[i, j]);
                        j++) ;
                    break;
                case Direction.UpLeft:
                    for (int i = tile.X - 1, j = tile.Y - 1;
                        i >= 0 && j >= 0 && predicate(Tiles[i, j]);
                        i--, j--) ;
                    break;
                case Direction.DownRight:
                    for (int i = tile.X + 1, j = tile.Y + 1;
                        i < Width && j < Height && predicate(Tiles[i, j]);
                        i++, j++) ;
                    break;
                case Direction.UpRight:
                    for (int i = tile.X + 1, j = tile.Y - 1;
                        i < Width && j >= 0 && predicate(Tiles[i, j]);
                        i++, j--) ;
                    break;
                case Direction.DownLeft:
                    for (int i = tile.X - 1, j = tile.Y + 1;
                        i >= 0 && j < Height && predicate(Tiles[i, j]);
                        i--, j++) ;
                    break;
            }
        }

        public Line GetLine(
            Tile tile,
            Direction direction,
            Piece piece,
            int max = 5)
        {
            List<Tile> chainTiles = new List<Tile>();
            List<Tile> blankTiles = new List<Tile>();
            List<Tile> blockTiles = new List<Tile>();

            string symbol = piece.Symbol;
            int count = 0;

            RunTilesFunction(
                tile,
                direction,
                t =>
                {
                    if (count == max)
                        return false;

                    if (t.Piece.Symbol == symbol)
                    {
                        chainTiles.Add(t);
                        count++;
                        return true;
                    }
                    else
                    {
                        if (t.Piece.Symbol == Piece.EMPTY.Symbol)
                            blankTiles.Add(t);
                        else
                            blockTiles.Add(t);

                        return false;
                    }
                });

            return new Line(chainTiles, blankTiles, blockTiles);
        }

        public LineGroup GetLineGroup(
            Tile tile,
            Orientation orientation,
            Piece piece,
            int max = 5)
        {
            switch (orientation)
            {
                case Orientation.Horizontal:
                    return new LineGroup(
                        GetLine(tile, Direction.Left, piece, max),
                        GetLine(tile, Direction.Right, piece, max));
                case Orientation.Vertical:
                    return new LineGroup(
                        GetLine(tile, Direction.Up, piece, max),
                        GetLine(tile, Direction.Down, piece, max));
                case Orientation.Diagonal:
                    return new LineGroup(
                        GetLine(tile, Direction.UpLeft, piece, max),
                        GetLine(tile, Direction.DownRight, piece, max));
                case Orientation.RvDiagonal:
                    return new LineGroup(
                        GetLine(tile, Direction.UpRight, piece, max),
                        GetLine(tile, Direction.DownLeft, piece, max));
            }

            return new LineGroup();
        }

        public Player GetCurrentPlayer()
        {
            return Players[Turn];
        }

        public void Play(Tile tile)
        {
            // Check if game is over
            if (IsGameOver)
                return;

            Tile _tile = Tiles[tile.X, tile.Y];

            // Check for already placed tile
            if (_tile.Piece.Symbol != Piece.EMPTY.Symbol)
                return;

            _tile.Piece = GetCurrentPlayer().Piece;

            // Check for game over
            for (int i = 0; i <= 3; i++)
            {
                LineGroup lineGroup = GetLineGroup(_tile, (Orientation)i, _tile.Piece);

                if (lineGroup.CountChainTiles() + 1 == WINPIECES
                    && lineGroup.CountBlockTiles() < 2)
                {
                    IsGameOver = true;
                    return;
                }
            }

            // Increment turn
            Turn = (Turn + 1) % Players.Count;        
        }

        public void Restart()
        {
            foreach (var tile in Tiles)
            {
                tile.Piece = Piece.EMPTY;
            }

            Turn = 0;
            IsGameOver = false;
        }

        public object Clone()
        {
            Board b = new Board(Width, Height, Players);
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    Tile tile = Tiles[i, j];
                    b.Tiles[i, j] = new Tile(i, j)
                    {
                        Piece = tile.Piece
                    };
                }
            b._turn = _turn;
            b._isGameOver = _isGameOver;
            return b;

            // return this.MemberwiseClone();
        }
    }
}
