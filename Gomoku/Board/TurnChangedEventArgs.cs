﻿using System;

namespace Gomoku.Board
{
    public class TurnChangedEventArgs : EventArgs
    {
        public readonly int Order;
        public readonly Player Player;

        public TurnChangedEventArgs(int order, Player player)
        {
            Order = order;
            Player = player;
        }
    }
}
