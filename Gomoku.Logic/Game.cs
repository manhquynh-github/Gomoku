using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines a Gomoku game
  /// </summary>
  public class Game : IDeepCloneable<Game>, IShallowCloneable<Game>
  {
    public static readonly int WINPIECES = 5;

    private readonly Stack<Tile> _history;
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
      _history = new Stack<Tile>();
      IsOver = false;
      ShiftPlayersOnGameOver = true;
    }

    private Game(Game g)
    {
      if (g is null)
      {
        throw new ArgumentNullException(nameof(g));
      }

      Board = g.Board.DeepClone();
      MaxMove = g.MaxMove;
      _players = new List<Player>(g._players);
      Turn = g.Turn;
      _history = new Stack<Tile>(g._history);
      IsOver = g.IsOver;
      ShiftPlayersOnGameOver = g.ShiftPlayersOnGameOver;
    }

    /// <summary>
    /// Fires when the <see cref="Game"/> has successfully changed its
    /// <see cref="Board"/> state.
    /// </summary>
    public event EventHandler<BoardChangedEventArgs> BoardChanged;

    /// <summary>
    /// Fires when the <see cref="Game"/> is going to change its
    /// <see cref="Board"/> state.
    /// </summary>
    public event EventHandler<BoardChangingEventArgs> BoardChanging;

    /// <summary>
    /// Fires when the <see cref="Game"/> is over or tie.
    /// </summary>
    public event EventHandler<GameOverEventArgs> GameOver;

    public Board Board { get; }

    public Player CurrentPlayer => _players[Turn];
    public IReadOnlyList<Tile> History => _history.ToImmutableList();
    public bool IsOver { get; private set; }
    public bool IsTie => _history.Count == MaxMove;

    /// <summary>
    /// The last <see cref="Tile"/> that was put on the <see cref="Board"/> chronologically.
    /// </summary>
    public Tile LastMove => _history.Count == 0 ? null : _history.Peek();

    /// <summary>
    /// Max number of moves the <see cref="Game"/> can make.
    /// </summary>
    public int MaxMove { get; }

    public IReadOnlyList<Player> Players => _players.AsReadOnly();

    /// <summary>
    /// Shift the players' orders when the <see cref="Game"/> is over. For
    /// example, the first <see cref="Player"/> will be the second
    /// <see cref="Player"/> in the next <see cref="Game"/>.
    /// </summary>
    public bool ShiftPlayersOnGameOver { get; set; }

    public int Turn { get; set; }

    /// <summary>
    /// Checks if <see cref="Game"/> is over at position <paramref name="x"/>,
    /// <paramref name="y"/> and outputs the <paramref name="winningTiles"/>.
    /// </summary>
    /// <param name="x">x-axis position to check</param>
    /// <param name="y">y-axis position to check</param>
    /// <param name="winningTiles">
    /// a collection of <see cref="Tile"/> if <see cref="true"/>, otherwise an
    /// empty collection is returned.
    /// </param>
    /// <returns>a <see cref="bool"/></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool CheckGameOver(int x, int y, out IEnumerable<Tile> winningTiles)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      if (_history.Count < 9)
      {
        winningTiles = Enumerable.Empty<Tile>();
        return false;
      }

      Tile tile = Board[x, y];

      if (tile.Piece.Type != Pieces.None)
      {
        Orientations[] orientations = new[]
        {
          Orientations.Horizontal,
          Orientations.Vertical,
          Orientations.Diagonal,
          Orientations.RvDiagonal
        };

        foreach (Orientations orientation in orientations)
        {
          var line = OrientedlLine.FromBoard(
            Board,
            tile.X,
            tile.Y,
            tile.Piece,
            orientation,
            maxTile: WINPIECES,
            blankTolerance: 0);

          if (line.IsChained
            && line.SameTileCount + 1 == WINPIECES
            && line.BlockTilesCount < 2)
          {
            var result = line.Tiles.ToList();
            result.Add(tile);
            winningTiles = result;
            return true;
          }
        }
      }

      winningTiles = Enumerable.Empty<Tile>();
      return false;
    }

    public Game DeepClone()
    {
      return new Game(this);
    }

    /// <summary>
    /// Gets the <paramref name="player"/>'s turn.
    /// </summary>
    /// <param name="player">a <see cref="Player"/></param>
    /// <returns>
    /// an <see cref="int"/> that represents the <paramref name="player"/>'s turn
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int GetPlayersTurn(Player player)
    {
      if (player is null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return _players.FindIndex(p => p == player);
    }

    /// <summary>
    /// Checks if the <see cref="CurrentPlayer"/> is <paramref name="player"/>
    /// </summary>
    /// <param name="player">a <see cref="Player"/></param>
    /// <returns>a <see cref="bool"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool IsPlayersTurn(Player player)
    {
      if (player is null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return CurrentPlayer == player;
    }

    /// <summary>
    /// Puts a <see cref="Tile"/> corresponding to <see cref="CurrentPlayer"/>
    /// and advances the <see cref="Game"/> to the next state. at position
    /// <paramref name="x"/>, <paramref name="y"/>.
    /// </summary>
    /// <param name="x">x-axis position</param>
    /// <param name="y">y-axis position</param>
    /// <exception cref="ArgumentException"></exception>
    public void Play(int x, int y)
    {
      if (x < 0 || x > Board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > Board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      // Check if game is over
      if (IsOver)
      {
        return;
      }

      Tile tile = Board[x, y];

      // Check for already placed tile
      if (tile.Piece.Type != Pieces.None)
      {
        return;
      }

      Player oldPlayer = CurrentPlayer;
      BoardChanging?.Invoke(this, new BoardChangingEventArgs(Turn, oldPlayer, LastMove));

      tile.Piece = oldPlayer.Piece;
      _history.Push(tile);

      // Check for game over
      if (IsTie)
      {
        IsOver = true;
        GameOver?.Invoke(this, new GameOverEventArgs(true, Turn, null, new List<Tile>()));

        if (ShiftPlayersOnGameOver)
        {
          ShiftPlayers();
        }
      }
      else if (CheckGameOver(x, y, out IEnumerable<Tile> winningLine))
      {
        IsOver = true;
        GameOver?.Invoke(this, new GameOverEventArgs(true, Turn, oldPlayer, winningLine));

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

      BoardChanged?.Invoke(this, new BoardChangedEventArgs(Turn, CurrentPlayer, tile));
    }

    /// <summary>
    /// Resets the <see cref="Game"/> to its original state.
    /// </summary>
    public void Restart()
    {
      BoardChanging?.Invoke(this, new BoardChangingEventArgs(Turn, CurrentPlayer, LastMove));

      foreach (Tile tile in _history)
      {
        tile.Piece = new Piece(Pieces.None);
      }

      Turn = 0;
      _history.Clear();
      IsOver = false;

      BoardChanged?.Invoke(this, new BoardChangedEventArgs(Turn, CurrentPlayer, null));
    }

    public Game ShallowClone()
    {
      return (Game)MemberwiseClone();
    }

    /// <summary>
    /// Reverts the <see cref="Game"/> back to the <see cref="LastMove"/>'s state.
    /// </summary>
    public void Undo()
    {
      if (_history.Count == 0)
      {
        return;
      }

      Tile tile = _history.Pop();
      BoardChanging?.Invoke(this, new BoardChangingEventArgs(Turn, CurrentPlayer, tile));

      tile.Piece = new Piece(Pieces.None);
      Turn = (Turn - 1 + _players.Count) % _players.Count;
      if (IsOver)
      {
        IsOver = false;
      }

      BoardChanged?.Invoke(this, new BoardChangedEventArgs(Turn, CurrentPlayer, LastMove));
    }

    object IDeepCloneable.DeepClone()
    {
      return DeepClone();
    }

    object IShallowCloneable.ShallowClone()
    {
      return ShallowClone();
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