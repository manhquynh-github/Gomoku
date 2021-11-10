using System;

namespace Gomoku.Logic
{
  public class Turn : IShallowCloneable<Turn>
  {
    public Turn(int maxTurn)
    {
      if (maxTurn < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(maxTurn));
      }

      Max = maxTurn;
      IsReverse = false;
      Start = 0;
      Current = 0;
    }

    public int Current { get; set; }

    public bool IsReverse { get; set; }

    public int Max { get; }

    public int Next => GetNext(Current, IsReverse);

    public int Previous => GetNext(Current, IsReverse);

    public int Start { get; private set; }

    public void MoveBack()
    {
      Current = Previous;
    }

    public void MoveNext()
    {
      Current = Next;
    }

    public void Reset()
    {
      Current = Start;
    }

    public void Reverse()
    {
      IsReverse = !IsReverse;
    }

    public Turn ShallowClone()
    {
      return (Turn)MemberwiseClone();
    }

    public void ShiftStartBackwards()
    {
      Start = GetNext(Start, true);
    }

    public void ShiftStartForwards()
    {
      Start = GetNext(Start, false);
    }

    public override string ToString()
    {
      return $"{nameof(Current)}={Current}, {nameof(Start)}={Start}";
    }

    object IShallowCloneable.ShallowClone()
    {
      return ShallowClone();
    }

    private int GetNext(int from, bool isReverse)
    {
      var OrderModifier = isReverse ? -1 : 1;
      return ((from + 1) * OrderModifier + Max) % Max;
    }
  }
}