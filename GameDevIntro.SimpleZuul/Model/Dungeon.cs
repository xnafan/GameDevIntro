using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace GameDevIntro.SimpleZuul.Model;
internal class Dungeon : IEnumerable<Tile>
{
    #region Properties
    public Tile[,] Tiles { get; set; }
    public Point PlayerPosition { get; set; }

    public int Width { get => Tiles.GetLength(0); }
    public int Height { get => Tiles.GetLength(1); }

    #endregion
    public Dungeon(int width, int height) => Tiles = new Tile[width, height];
    public IEnumerator<Tile> GetEnumerator() => new TileDoubleArrayIterator(this.Tiles);
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}