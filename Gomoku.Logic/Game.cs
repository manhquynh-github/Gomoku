using System;
using System.Collections.Generic;
using System.Linq;

namespace Gomoku.Logic
{
  public class Game : ICloneable
  {
    public static readonly int WINPIECES = 5;

    private readonly List<Player> _players;

    public Game(int width, int height, IEnumerable<Player> players)
    {
      if (height <= WINPIECES || width <= WINPIECES)
      {
        throw new ArgumentException(
          $"{nameof(Game)} must have at least {WINPIECES}x{WINPIECES} board.");
      }

      Board = new Board(width, height);
      MaxMove = width * height;
      _players = new List<Player>(players);
      Turn = 0;
      History = new Stack<Tile>();
      IsOver = false;
      ShiftPlayersOnGameOver = true;
    }

    public Game(Game g)
    {
      if (g is null)
      {
        throw new ArgumentNullException(nameof(g));
      }

      Board = g.Board.DeepClone();
      MaxMove = g.MaxMove;
      _players = new List<Player>(g._players);
      Turn = g.Turn;
      History = new Stack<Tile>(g.History);
      IsOver = g.IsOver;
      ShiftPlayersOnGameOver = g.ShiftPlayersOnGameOver;
    }

    public event BoardChangedEventHandler BoardChanged;

    public event BoardChangingEventHandler BoardChanging;

    public event GameOverEventHandler GameOver;

    public Board Board { get; }
    public Player CurrentPlayer => _players[Turn];
    public Stack<Tile> History { get; set; }
    public bool IsOver { get; private set; }
    public bool IsTie => History.Count == MaxMove;
    public Tile LastPlayedTile => History.Count == 0 ? null : History.Peek();
    public int MaxMove { get; }
    public IReadOnlyList<Player> Players => _players;
    public bool ShiftPlayersOnGameOver { get; set; }
    public int Turn { get; set; }
    public int Height => Board.Height;
    public int Width => Board.Width;

    public bool CheckGameOver(Tile tile, out Line winningLine)
    {
      if (tile is null)
      {
        throw new ArgumentNullException(nameof(tile));
      }

      if (tile.Piece.Type != Pieces.None)
      {
        Tile _tile = Board[tile.X, tile.Y];

        for (var i = 0; i <= 3; i++)
        {
          LineGroup lineGroup = Board.GetLineGroup(
            _tile,
            (Orientation)i,
            _tile.Piece.Type,
            WINPIECES);

          if (lineGroup.IsChained
            && lineGroup.SameTileCount + 1 == WINPIECES
            && lineGroup.BlockTileCount < 2)
          {
            var winningTiles =
              (from line in lineGroup.Lines
               from t in line.SameTiles
               select t)
              .ToList();

            winningTiles.Add(tile);
            winningLine = new Line(winningTiles);
            return true;
          }
        }
      }

      winningLine = Line.EMPTY;
      return false;
    }

    public object Clone()
    {
      return MemberwiseClone();
    }

    public Game DeepClone()
    {
      return new Game(this);
    }

    public int GetPlayersTurn(Player player)
    {
      if (player == null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return _players.FindIndex(p => p == player);
    }

    public bool IsPlayersTurn(Player player)
    {
      if (player == null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return CurrentPlayer == player;
    }

    public void Play(Tile tile)
    {
      // Check if game is over
      if (IsOver)
      {
        return;
      }

      Tile _tile = Board[tile.X, tile.Y];

      // Check for already placed tile
      if (_tile.Piece.Type != Pieces.None)
      {
        return;
      }

      Player oldPlayer = CurrentPlayer;
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, oldPlayer, LastPlayedTile));

      _tile.Piece = oldPlayer.Piece;
      History.Push(_tile);

      // Check for game over
      if (IsTie)
      {
        IsOver = true;
        GameOver?.Invoke(new GameOverEventArgs(true, Turn, null, Line.EMPTY));

        if (ShiftPlayersOnGameOver)
        {
          ShiftPlayers();
        }
      }
      else if (CheckGameOver(_tile, out Line winningLine))
      {
        IsOver = true;
        GameOver?.Invoke(new GameOverEventArgs(true, Turn, oldPlayer, winningLine));

        if (ShiftPlayersOnGameOver)
        {
          ShiftPlayers();
        }
      }
      else
      {
        // Increment turn
        Turn = (Turn + 1) % _players.Count;
      }

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, CurrentPlayer, _tile));
    }

    public void Restart()
    {
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, CurrentPlayer, LastPlayedTile));

      foreach (Tile tile in History)
      {
        tile.Piece = new Piece(Pieces.None);
      }

      Turn = 0;
      History.Clear();
      IsOver = false;

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, CurrentPlayer, null));
    }

    public void Undo()
    {
      if (History.Count == 0)
      {
        return;
      }

      Tile tile = History.Pop();
      BoardChanging?.Invoke(new BoardChangingEventArgs(Turn, CurrentPlayer, tile));

      tile.Piece = new Piece(Pieces.None);
      Turn = (Turn - 1 + _players.Count) % _players.Count;
      if (IsOver)
      {
        IsOver = false;
      }

      BoardChanged?.Invoke(new BoardChangedEventArgs(Turn, CurrentPlayer, LastPlayedTile));
    }

    private void ShiftPlayers()
    {
      if (_players is null
        || !_players.Any())
      {
        throw new InvalidOperationException("Player list is null or empty");
      }

      Player last = _players[^1];
      _players.RemoveAt(_players.Count - 1);
      _players.Insert(0, last);
    }
  }
}