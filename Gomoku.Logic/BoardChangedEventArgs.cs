using System;
using System.Collections.Generic;

namespace Gomoku.Logic
{
  /// <summary>
  /// Represents the objects related to <see cref="Game.BoardChanged"/> event.
  /// </summary>
  public class BoardChangedEventArgs : EventArgs
  {
    public BoardChangedEventArgs(
      IList<Tile> addedTiles,
      IList<Tile> removedTiles)
    {
      AddedTiles = addedTiles ?? throw new ArgumentNullException(nameof(addedTiles));
      RemovedTiles = removedTiles ?? throw new ArgumentNullException(nameof(removedTiles));
    }

    public IList<Tile> AddedTiles { get; }
    public IList<Tile> RemovedTiles { get; }
  }
}