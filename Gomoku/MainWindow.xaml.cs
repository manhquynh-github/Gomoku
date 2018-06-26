﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gomoku.AI;
using Gomoku.Board;

namespace Gomoku
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly GomokuAIv1 AI;
        public readonly Board.Board Board;
        private List<Tile> choices;

        public MainWindow() :
            this(15, 15,
                new List<Player>()
                {
                    new Player("Player 1", Piece.X),
                    new Player("Player 2", Piece.O, true)
                })
        {
        }

        public MainWindow(int boardWidth, int boardHeight, IList<Player> players)
        {
            InitializeComponent();

            Board = new Board.Board(boardWidth, boardHeight, players);
            InitializeBoard(boardWidth, boardHeight);
            AI = new GomokuAIv1();
        }

        private void InitializeBoard(int width, int height)
        {
            Style widthStackPanelStyle = Resources["WidthStackPanelStyle"] as Style;
            Style tileStyle = Resources["TileButtonStyle"] as Style;

            StackPanel columnStackPanel = new StackPanel
            {
                Style = widthStackPanelStyle
            };
            for (int j = -1; j < width; j++)
            {
                Button tileButton = new Button
                {
                    Style = tileStyle,
                    Content = j
                };
                columnStackPanel.Children.Add(tileButton);
            }
            HeightStackPanel.Children.Add(columnStackPanel);

            for (int i = 0; i < height; i++)
            {
                StackPanel widthStackPanel = new StackPanel
                {
                    Style = widthStackPanelStyle
                };

                Button rowButton = new Button
                {
                    Style = tileStyle,
                    Content = i
                };
                widthStackPanel.Children.Add(rowButton);

                for (int j = 0; j < width; j++)
                {
                    Button tileButton = new Button
                    {
                        DataContext = Board.Tiles[j, i],
                        Style = tileStyle
                    };
                    Board.Tiles[j, i].UIElement = tileButton;
                    widthStackPanel.Children.Add(tileButton);
                }

                HeightStackPanel.Children.Add(widthStackPanel);
            }

            Board.GameOver += Board_GameOver;
        }

        private void Board_GameOver(GameOverEventArgs e)
        {
            MessageBox.Show(e.Winner.Name + " wins!");
        }

        private void TileButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.DataContext != null)
            {
                if (choices != null && choices.Count > 0)
                    foreach (var tile in choices)
                    {
                        ((Button)tile.UIElement).BorderThickness = new Thickness(1.0);
                    }

                Board.Play(button.DataContext as Tile);
                button.Content = (button.DataContext as Tile).Piece.Symbol;

                // AI
                if (Board.GetCurrentPlayer().IsAuto && UseAI.IsChecked == true)
                {
                    Board.Play(AI.Play(Board, out choices));
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AI.Play(Board, out choices);
            foreach (var tile in choices)
            {
                ((Button)tile.UIElement).BorderThickness = new Thickness(2.0);
            }
        }
    }
}
