using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Gomoku.Logic.AI;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines a Gomoku game
  /// </summary>
  public class Game : IDeepCloneable<Game>, IShallowCloneable<Game>
  {
    public static readonly int WINPIECES = 5;
    private readonly Stack<Tile> _history;

    public Game(int width, int height, IEnumerable<Player> players)
    {
      if (height <= WINPIECES || width <= WINPIECES)
      {
        throw new ArgumentException(
          $"{nameof(Game)} must have at least {WINPIECES}x{WINPIECES} board.");
      }

      Board = new Board(width, height);
      MaxMove = width * height;
      Manager = new PlayerManager(players);
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
      Manager = g.Manager.DeepClone();
      _history = new Stack<Tile>(g._history.Reverse());
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

    /// <summary>
    /// Gets the <see cref="Logic.Board"/> of this <see cref="Game"/>.
    /// </summary>
    public Board Board { get; }

    /// <summary>
    /// Checks if the <see cref="Game"/> is undoable.
    /// </summary>
    public bool CanUndo => _history.Count > 0;

    /// <summary>
    /// Gets the history of the game, last-in-first-out.
    /// </summary>
    public IReadOnlyList<Tile> History => _history.ToArray();

    /// <summary>
    /// Checks if the game is over.
    /// </summary>
    public bool IsOver { get; private set; }

    /// <summary>
    /// Checks if the game is tie.
    /// </summary>
    public bool IsTie => _history.Count == MaxMove;

    /// <summary>
    /// Gets the last <see cref="Tile"/> that was put on the <see cref="Board"/>
    /// chronologically. Returns <see cref="null"/> if no history.
    /// </summary>
    public Tile LastMove => _history.Count == 0 ? null : _history.Peek();

    /// <summary>
    /// Gets the <see cref="PlayerManager"/> of this <see cref="Game"/>.
    /// </summary>
    public PlayerManager Manager { get; }

    /// <summary>
    /// Max number of moves the <see cref="Game"/> can make.
    /// </summary>
    public int MaxMove { get; }

    /// <summary>
    /// Shift the players' orders when the <see cref="Game"/> is over. For
    /// example, the first <see cref="Player"/> will be the second
    /// <see cref="Player"/> in the next <see cref="Game"/>.
    /// </summary>
    public bool ShiftPlayersOnGameOver { get; set; }

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
    public bool CheckGameOver(int x, int y, out IList<Tile> winningTiles)
    {
      if (IsOver || IsTie)
      {
        winningTiles = new Tile[0];
        return true;
      }

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
        winningTiles = new Tile[0];
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
            var result = line.GetSameTiles().ToList();
            result.Add(tile);
            winningTiles = result;
            return true;
          }
        }
      }

      winningTiles = new Tile[0];
      return false;
    }

    public Game DeepClone()
    {
      return new Game(this);
    }

    public void Play(IPositional positional)
    {
      Play(positional.X, positional.Y);
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

      Player oldPlayer = Manager.CurrentPlayer;
      tile.Piece = oldPlayer.Piece;
      Tile previousTile = LastMove;
      _history.Push(tile);

      BoardChanging?.Invoke(
        this,
        new BoardChangingEventArgs(
          new Tile[] { tile },
          new Tile[0]));

      // Check for game over
      if (CheckGameOver(x, y, out IList<Tile> winningLine))
      {
        IsOver = true;

        if (ShiftPlayersOnGameOver)
        {
          Manager.Turn.ShiftStartForwards();
        }
      }

      // Increment turn
      Manager.Turn.MoveNext();

      BoardChanged?.Invoke(
        this,
        new BoardChangedEventArgs(
          new Tile[] { tile },
          new Tile[0]));

      if (IsOver)
      {
        GameOver?.Invoke(
        this,
        new GameOverEventArgs(
          Manager.Turn.Current,
          oldPlayer,
          winningLine));
      }
    }

    /// <summary>
    /// Resets the <see cref="Game"/> to its original state.
    /// </summary>
    public void Restart()
    {
      Tile[] history = _history.ToArray();
      BoardChanging?.Invoke(
        this,
        new BoardChangingEventArgs(
          new Tile[0],
          history));

      foreach (Tile tile in _history)
      {
        tile.Piece = new Piece(Pieces.None);
      }

      Manager.Turn.Reset();
      _history.Clear();
      IsOver = false;

      BoardChanged?.Invoke(
        this,
        new BoardChangedEventArgs(
          new Tile[0],
          history));
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
      if (!CanUndo)
      {
        return;
      }

      Tile removedTile = _history.Pop();
      BoardChanging?.Invoke(
        this,
        new BoardChangingEventArgs(
          new Tile[0],
          new Tile[] { removedTile }));

      removedTile.Piece = new Piece(Pieces.None);
      Manager.Turn.MoveBack();
      if (IsOver)
      {
        IsOver = false;
        Manager.Turn.ShiftStartBackwards();
      }

      BoardChanged?.Invoke(
        this,
        new BoardChangedEventArgs(
          new Tile[0],
          new Tile[] { removedTile }));
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