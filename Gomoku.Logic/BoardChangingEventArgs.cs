using System;
using System.Collections.Generic;

namespace Gomoku.Logic
{
  /// <summary>
  /// Represents the objects related to <see cref="Game.BoardChanging"/> event.
  /// </summary>
  public class BoardChangingEventArgs : EventArgs
  {
    public BoardChangingEventArgs(
      IList<Tile> willBeAddedTiles,
      IList<Tile> willBeRemovedTiles)
    {
      WillBeAddedTiles = willBeAddedTiles ?? throw new ArgumentNullException(nameof(willBeAddedTiles));
      WillBeRemovedTiles = willBeRemovedTiles ?? throw new ArgumentNullException(nameof(willBeRemovedTiles));
    }

    public IList<Tile> WillBeAddedTiles { get; }
    public IList<Tile> WillBeRemovedTiles { get; }
  }
}