﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Gomoku.Logic
{
  /// <summary>
  /// Defines group of <see cref="Tile"/> that is collected by traversing from
  /// one <see cref="Tile"/> on a <see cref="Board"/> towards a
  /// <see cref="Directions"/> until it reaches two <see cref="Tile"/> of
  /// <see cref="Pieces.None"/> or a <see cref="Tile"/> with a different <see cref="Piece"/>.
  /// </summary>
  public class DirectionalLine
  {
    /// <summary>
    /// Gets an instance of an empty <see cref="DirectionalLine"/>.
    /// </summary>
    public static readonly DirectionalLine EMPTY = new DirectionalLine();

    private DirectionalLine() : this(
      (Piece)Pieces.None,
      Directions.None,
      new Tile[0],
      new Tile[0],
      new Tile[0],
      new Tile[0],
      false)
    {
    }

    private DirectionalLine(
      Piece piece,
      Directions diretion,
      Tile[] tiles,
      Tile[] sameTiles,
      Tile[] blankTiles,
      Tile[] blockTiles,
      bool isChained)
    {
      Piece = piece;
      Direction = diretion;
      Tiles = tiles;
      SameTiles = sameTiles;
      BlankTiles = blankTiles;
      BlockTiles = blockTiles;
      IsChained = isChained;
    }

    /// <summary>
    /// All the tiles that have <see cref="Pieces.None"/>.
    /// </summary>
    public Tile[] BlankTiles { get; }

    /// <summary>
    /// All the tiles that have a different <see cref="Logic.Piece"/> other than <see cref="Piece"/>.
    /// </summary>
    public Tile[] BlockTiles { get; }

    /// <summary>
    /// The <see cref="Directions"/> that this <see cref="DirectionalLine"/> traverses.
    /// </summary>
    public Directions Direction { get; }

    /// <summary>
    /// Determines if all <see cref="SameTiles"/> are all next to each other
    /// without a <see cref="Pieces.None"/> tile in-between.
    /// </summary>
    public bool IsChained { get; }

    /// <summary>
    /// The original <see cref="Piece"/> of this <see cref="DirectionalLine"/>
    /// </summary>
    public Piece Piece { get; }

    /// <summary>
    /// All the tiles that have <see cref="Piece"/>.
    /// </summary>
    public Tile[] SameTiles { get; }

    /// <summary>
    /// All tiles
    /// </summary>
    public Tile[] Tiles { get; }

    /// <summary>
    /// Creates a new <see cref="DirectionalLine"/> by traversing the
    /// <paramref name="board"/> starting at position <paramref name="x"/>,
    /// <paramref name="y"/> towards <paramref name="direction"/> where
    /// <paramref name="piece"/> will determine <see cref="SameTiles"/> until
    /// <paramref name="maxTile"/> is reached, or a different
    /// <see cref="Logic.Piece"/> than <paramref name="piece"/> is encountered,
    /// or more than <paramref name="blankTolerance"/> number of
    /// <see cref="Pieces.None"/> tiles are encountered.
    /// </summary>
    /// <param name="board">the <see cref="Board"/> to be used</param>
    /// <param name="x">the starting <see cref="Tile.X"/></param>
    /// <param name="y">the starting <see cref="Tile.Y"/></param>
    /// <param name="piece">
    /// the <see cref="Logic.Piece"/> that will determine <see cref="SameTiles"/>
    /// </param>
    /// <param name="direction">the <see cref="Directions"/> to traverse</param>
    /// <param name="maxTile">the max number of <see cref="Tile"/> to traverse</param>
    /// <param name="blankTolerance">
    /// the max number of <see cref="Pieces.None"/> tiles before the traversing stops
    /// </param>
    /// <returns>a <see cref="DirectionalLine"/></returns>
    public static DirectionalLine FromBoard(
      Board board,
      int x,
      int y,
      Piece piece,
      Directions direction,
      int maxTile,
      int blankTolerance)
    {
      if (board is null)
      {
        throw new ArgumentNullException(nameof(board));
      }

      if (x < 0 || x > board.Width)
      {
        throw new ArgumentException("Value is out of range", nameof(x));
      }

      if (y < 0 || y > board.Height)
      {
        throw new ArgumentException("Value is out of range", nameof(y));
      }

      var tiles = new Queue<Tile>();
      var sameTiles = new Queue<Tile>();
      var blankTiles = new Queue<Tile>();
      var blockTiles = new Queue<Tile>();

      var count = 0;
      var chainBreak = false;
      var blank = 0;

      board.IterateTiles(
        x,
        y,
        direction,
        t =>
        {
          if (count++ == maxTile)
          {
            return false;
          }

          if (t.Piece.Type == piece)
          {
            if (blank > 0)
            {
              chainBreak = true;
            }

            tiles.Enqueue(t);
            sameTiles.Enqueue(t);
            return true;
          }
          else if (t.Piece.Type == Pieces.None)
          {
            if (blank++ == blankTolerance)
            {
              return false;
            }

            tiles.Enqueue(t);
            blankTiles.Enqueue(t);
            return true;
          }
          else
          {
            tiles.Enqueue(t);
            blockTiles.Enqueue(t);
            return false;
          }
        });

      return new DirectionalLine(
        piece,
        direction,
        tiles.ToArray(),
        sameTiles.ToArray(),
        blankTiles.ToArray(),
        blockTiles.ToArray(),
        !chainBreak);
    }

    public string ToString(bool includesSelf, bool followsLTRB)
    {
      IEnumerable<Tile> source = Tiles;

      if (followsLTRB)
      {
        switch (Direction)
        {
          case Directions.Left:
          case Directions.Up:
          case Directions.UpLeft:
          case Directions.DownLeft:
            source = Tiles.Reverse();
            break;

          case Directions.Right:
          case Directions.Down:
          case Directions.UpRight:
          case Directions.DownRight:
            break;

          default:
            throw new InvalidOperationException("Unexpected value.");
        }
      }

      var tilesString = string.Join(
        ',',
        source.Select(t => t.Piece.ToString()));

      return includesSelf
        ? string.Join(
          ',',
          Piece.ToString(),
          tilesString)
        : tilesString;
    }

    public override string ToString()
    {
      return ToString(includesSelf: true, followsLTRB: false);
    }
  }
}