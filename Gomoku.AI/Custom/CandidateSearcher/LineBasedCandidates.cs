using System.Collections.Generic;

using Gomoku.AI.Custom.Algorithms;

using Gomoku.Logic;

namespace Gomoku.AI.Custom.CandidateSearcher
{
  public class LineBasedCandidates : ICandidateSearcher
  {
    public IEnumerable<IPositional> Search(Game game, int maxTile = 2, int blankTolerance = 1)
    {
      // Get all the placed tiles to determine all the correct playable tiles
      IReadOnlyList<Tile> placedTiles = game.History;

      // Get all the playable tiles using a HashSet where only distinct tiles
      // are added
      var playableTiles = new HashSet<Tile>();
      foreach (Tile tile in placedTiles)
      {
        // Loop all 4 Orientation enumeration
        Orientations[] orientations = new[]
        {
          Orientations.Horizontal,
          Orientations.Vertical,
          Orientations.Diagonal,
          Orientations.RvDiagonal
        };

        foreach (Orientations orientation in orientations)
        {
          // Retrieve line of each orientation within 2-tile range where the
          // tiles are empty
          foreach (Tile t in
            OrientedlLine.FromBoard(
              game.Board,
              tile.X,
              tile.Y,
              (Piece)Pieces.None,
              orientation,
              maxTile: maxTile,
              blankTolerance: blankTolerance)
            .SameTiles)
          {
            playableTiles.Add(t);
          }
        }
      }

      return playableTiles;
    }

    IEnumerable<IPositional> ICandidateSearcher.Search(Game game)
    {
      return Search(game);
    }
  }
}