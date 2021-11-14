using System;

using Gomoku.Logic;

namespace Gomoku.AI.Custom.Evaluator
{
  /// <summary>
  /// Evaluates a <see cref="Game"/>'s state by using weighted scoring
  /// mechanism, allowing higher scores for states that are likely to result in
  /// a win.
  /// </summary>
  public class WeightedPointBasedEvaluator : ITileEvaluator
  {
    /// <summary>
    /// Evaluates a <paramref name="game"/>'s state at
    /// <paramref name="positional"/> for <paramref name="piece"/>.
    /// </summary>
    /// <param name="game">the <see cref="Game"/> to be evaluated.</param>
    /// <param name="positional">the <see cref="IPositional"/> to evaluate.</param>
    /// <param name="piece">the <see cref="Piece"/> to evaluate for.</param>
    /// <returns>
    /// A non-negative number representing the probability of winning the game
    /// at <paramref name="positional"/>. The only negative number
    /// <see cref="double.MinValue"/> is returned when the
    /// <paramref name="game"/> is lost but the winner is not
    /// <paramref name="piece"/>. If the <paramref name="piece"/> wins the
    /// <paramref name="game"/>, returns <see cref="double.MaxValue"/>
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public double Evaluate(Game game, IPositional positional, Piece piece)
    {
      if (game is null)
      {
        throw new ArgumentNullException(nameof(game));
      }

      if (positional is null)
      {
        throw new ArgumentNullException(nameof(positional));
      }

      if (piece == Pieces.None)
      {
        throw new ArgumentException($"{nameof(piece)} must not be {Pieces.None}");
      }

      var point = 0.0;

      // If game is over the point should be high enough so that this node is
      // more likely to get noticed
      if (game.IsOver)
      {
        if (game.Manager.PreviousPlayer.Piece == piece)
        {
          point = double.MaxValue;
        }
        else
        {
          point = double.MinValue;
        }
      }

      // Otherwise evaluate the point matching lines and other information
      else
      {
        // Retrieve line group of each orientation to fully evaluate a tile
        Orientations[] orientations = new[]
        {
          Orientations.Horizontal,
          Orientations.Vertical,
          Orientations.Diagonal,
          Orientations.RvDiagonal
        };

        foreach (Orientations orientation in orientations)
        {
          // Get line within 5-tile range
          var line =
            OrientedlLine.FromBoard(
              game.Board,
              positional.X,
              positional.Y,
              piece,
              orientation,
              maxTile: Game.WINPIECES,
              blankTolerance: 1);

          // Calculate points
          var sameTilesCount = line.SameTileCount;
          var blockTilesCount = line.BlockTilesCount;

          // When the line is not blocked
          if (blockTilesCount == 0)
          {
            // If the line chain has more tiles than win pieces, then this tile
            // is less worth.
            if (sameTilesCount + 1 >= Game.WINPIECES)
            {
              point += sameTilesCount;
            }

            // Otherwise
            else
            {
              // Calculate point using Geometric series of 2.0 so that the more
              // chain it has, the more valuable the line
              var _point =
                  1.0 * (1.0 - Math.Pow(2.0, sameTilesCount)) / (1.0 - 2.0);

              if (_point < 0)
              {
                throw new Exception();
              }

              // Finally the point is added with the power of itself
              point += Math.Pow(_point, line.IsChained ? 2.0 : 1.5);
            }
          }

          // When the line is partially blocked, only add the point which equals
          // to the same count
          else if (blockTilesCount == 1)
          {
            point += sameTilesCount;
          }

          // Otherwise, add no point.

          //point += (1.0 * (1.0 - Math.Pow(2.0, line.CountChainTiles())) / (1.0 - 2.0));
          //point += 0.25 * line.CountBlankTiles();
          //point -= 1.0 * line.BlockTileCount;
        }
      }

      return point;
    }
  }
}