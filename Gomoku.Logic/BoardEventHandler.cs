﻿namespace Gomoku.Logic
{
  public delegate void BoardChangedEventHandler(BoardChangedEventArgs e);

  public delegate void BoardChangingEventHandler(BoardChangingEventArgs e);

  public delegate void GameOverEventHandler(GameOverEventArgs e);
}