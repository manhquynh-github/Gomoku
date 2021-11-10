using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Gomoku.Logic
{
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

    public Player CurrentPlayer => Players[Turn.Current];
    public Player NextPlayer => Players[Turn.Next];
    public ImmutableArray<Player> Players { get; }
    public Player PreviousPlayer => Players[Turn.Previous];
    public Turn Turn { get; }

    public Player this[int index] => Players[index];

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

    object IDeepCloneable.DeepClone()
    {
      return DeepClone();
    }
  }
}