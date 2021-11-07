using System;
using System.Collections.Generic;
using System.Linq;

namespace Gomoku.Logic.AI
{
  public class AnalysisResult
  {
    public AnalysisResult(IPositional selectedChoice)
    {
      SelectedChoice = selectedChoice;
      PossibleChoices = new List<IPositional>()
      {
        selectedChoice
      };
    }

    public AnalysisResult(IPositional selectedChoice, IEnumerable<IPositional> possibleChoices)
    {
      if (possibleChoices is null)
      {
        throw new ArgumentNullException(nameof(possibleChoices));
      }

      PossibleChoices = possibleChoices;
      SelectedChoice = selectedChoice;
    }

    public IEnumerable<IPositional> PossibleChoices { get; }
    public IPositional SelectedChoice { get; }
  }
}