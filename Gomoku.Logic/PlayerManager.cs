using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gomoku.Logic
{
  /// <summary>
  /// Represents the game's manager for players and order.
  /// </summary>
  public class PlayerManager : IDeepCloneable<PlayerManager>
  {
    public PlayerManager(IEnumerable<Player> collection)
    {
      if (collection is null)
      {
        throw new ArgumentNullException(nameof(collection));
      }

      if (!collection.Any() || collection.Count() < 1)
      {
        throw new ArgumentException(nameof(collection));
      }

      Players = collection.ToImmutableArray();
      Turn = new Turn(Players.Length);
    }

    public PlayerManager(int numberofPlayers) : this(new Player[numberofPlayers])
    {
    }

    private PlayerManager(PlayerManager playerManager)
    {
      Players = ImmutableArray.Create(playerManager.Players, 0, playerManager.Players.Length);
      Turn = playerManager.Turn.ShallowClone();
    }

    /// <summary>
    /// Gets the <see cref="Player"/> in the current turn.
    /// </summary>
    public Player CurrentPlayer => Players[Turn.Current];

    /// <summary>
    /// Gets the <see cref="Player"/> in the next turn.
    /// </summary>
    public Player NextPlayer => Players[Turn.Next];

    /// <summary>
    /// Gets the array of <see cref="Player"/> s this
    /// <see cref="PlayerManager"/> holds.
    /// </summary>
    public ImmutableArray<Player> Players { get; }

    /// <summary>
    /// Gets the <see cref="Player"/> in the previous turn.
    /// </summary>
    public Player PreviousPlayer => Players[Turn.Previous];

    /// <summary>
    /// Gets the <see cref="Logic.Turn"/> object.
    /// </summary>
    public Turn Turn { get; }

    /// <summary>
    /// Gets the <see cref="Player"/> at <paramref name="index"/>.
    /// </summary>
    /// <param name="index">the zero-based index to get.</param>
    /// <returns>the <see cref="Player"/> at <paramref name="index"/>.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public Player this[int index] => Players[index];

    /// <summary>
    /// Returns a new deep clone.
    /// </summary>
    /// <returns></returns>
    public PlayerManager DeepClone()
    {
      return new PlayerManager(this);
    }

    /// <summary>
    /// Gets the <paramref name="player"/>'s turn.
    /// </summary>
    /// <param name="player">a <see cref="Player"/></param>
    /// <returns>
    /// An <see cref="int"/> that represents the <paramref name="player"/>'s
    /// turn. If not found, returns -1.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int GetPlayersTurn(Player player)
    {
      if (player is null)
      {
        throw new ArgumentNullException(nameof(player));
      }

      return Players.IndexOf(player);
    }

    /// <summary>
    /// Gets the <paramref name="player"/>'s turn by name.
    /// </summary>
    /// <param name="name">the name to look up</param>
    /// <returns>
    /// An <see cref="int"/> that represents the <paramref name="player"/>'s
    /// turn. If not found, returns -1.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public int GetPlayersTurn(string name)
    {
      if (name is null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      for (var i = 0; i < Players.Length; i++)
      {
        Player player = Players[i];
        if (player.Name == name)
        {
          return i;
        }
      }

      return -1;
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

    public override string ToString()
    {
      return $"{nameof(CurrentPlayer)}={CurrentPlayer.Name}";
    }

    object IDeepCloneable.DeepClone()
    {
      return DeepClone();
    }
  }
}