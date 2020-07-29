using System;
using System.Collections.Generic;
using System.Linq;

namespace Gomoku.Logic.AI
{
  public class AnalysisResult
  {
    public AnalysisResult(IEnumerable<Tile> possibleChoices, Tile selectedChoice)
    {
      if (possibleChoices is null)
      {
        throw new ArgumentNullException(nameof(possibleChoices));
      }

      if (!possibleChoices.Any())
      {
        throw new ArgumentException("Collection must not be empty", nameof(possibleChoices));
      }

      PossibleChoices = possibleChoices;

      SelectedChoice = selectedChoice
        ?? throw new ArgumentNullException(nameof(selectedChoice));
    }

    public IEnumerable<Tile> PossibleChoices { get; }
    public Tile SelectedChoice { get; }
  }
}