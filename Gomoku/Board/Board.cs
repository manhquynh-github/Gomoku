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
        public readonly int MaxMove;
        public readonly List<Player> Players;
        public int Turn { get; set; }
        public Stack<Tile> History;
        public bool IsGameOver { get; private set; }

        public static readonly int WINPIECES = 5;

        public Tile LastPlayedTile => History.Count == 0 ? null : History.Peek();
        public bool IsTie => History.Count == MaxMove;

        public event BoardChangingEventHandler BoardChanging;
        public event BoardChangedEventHandler BoardChanged;
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
            MaxMove = Width * Height;
            Players = new List<Player>(players);
            Turn = 0;
            History = new Stack<Tile>();
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
            List<Tile> sameTiles = new List<Tile>();
            List<Tile> blankTiles = new List<Tile>();
            List<Tile> blockTiles = new List<Tile>();

            string symbol = piece.Symbol;
            int count = 0;
            bool chainBreak = false;
            bool previouslyBlank = false;

            RunTilesFunction(
                tile,
                direction,
                t =>
                {
                    if (count++ == max)
                        return false;

                    if (t.Piece.Symbol == symbol)
                    {
                        if (previouslyBlank)
                            chainBreak = true;
                        
                        previouslyBlank = false;

                        sameTiles.Add(t);
                        return true;
                    }
                    else if (t.Piece.Symbol == Piece.EMPTY.Symbol)
                    {
                        if (previouslyBlank || chainBreak)
                            return false;
                        
                        previouslyBlank = true;

                        blankTiles.Add(t);
                        return true;
                    }
                    else
                    {
                        blockTiles.Add(t);
                        return false;
                    }
                });

            return new Line(sameTiles, blankTiles, blockTiles, !chainBreak);
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

            Player oldPlayer = GetCurrentPlayer();
            BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, oldPlayer, LastPlayedTile));

            _tile.Piece = oldPlayer.Piece;
            History.Push(_tile);

            // Check for game over
            if (IsTie)
            {
                IsGameOver = true;
                GameOver?.Invoke(new GameOverEventArgs(true, Turn, null));
            }
            else
            {
                for (int i = 0; i <= 3; i++)
                {
                    LineGroup lineGroup = GetLineGroup(_tile, (Orientation)i, _tile.Piece);

                    if (lineGroup.IsChained
                        && lineGroup.SameTileCount + 1 == WINPIECES
                        && lineGroup.BlockTileCount < 2)
                    {
                        IsGameOver = true;
                        GameOver?.Invoke(new GameOverEventArgs(true, Turn, oldPlayer));
                    }
                }
            }

            // Increment turn
            Turn = (Turn + 1) % Players.Count;

            BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, GetCurrentPlayer(), _tile));
        }

        public void Restart()
        {
            BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, GetCurrentPlayer(), LastPlayedTile));
            
            foreach (var tile in History)
            {
                tile.Piece = Piece.EMPTY;
            }
            
            Turn = 0;
            History.Clear();
            IsGameOver = false;

            BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, GetCurrentPlayer(), null));
        }

        public void Undo()
        {
            if (History.Count == 0)
                return;

            Tile tile = History.Pop();
            BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, GetCurrentPlayer(), tile));

            tile.Piece = Piece.EMPTY;
            Turn = (Turn - 1 + Players.Count) % Players.Count;
            if (IsGameOver)
                IsGameOver = false;

            BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, GetCurrentPlayer(), LastPlayedTile));
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
            b.Turn = Turn;
            b.History = new Stack<Tile>(History);
            b.IsGameOver = IsGameOver;
            return b;

            // return this.MemberwiseClone();
        }
    }
}
