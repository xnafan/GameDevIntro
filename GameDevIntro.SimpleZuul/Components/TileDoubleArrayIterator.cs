using GameDevIntro.SimpleZuul.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDevIntro.SimpleZuul.Components;
internal class TileDoubleArrayIterator : IEnumerator<Tile>
{
    private Tile[,] _tilesToIterateOver;
    private int _currentX, _currentY;
    public TileDoubleArrayIterator(Tile[,] tilesToIterateOver)
    {
        _tilesToIterateOver = tilesToIterateOver;
    }

    public Tile Current => _tilesToIterateOver[_currentX, _currentY];

    object IEnumerator.Current => Current;

    public void Dispose()
    {
        return;
    }

    public bool MoveNext()
    {
        _currentX++;
        if (_currentX >= _tilesToIterateOver.GetLength(0))
        {
            _currentX = 0;
            _currentY++;
        }
        return _currentY <= _tilesToIterateOver.GetLength(1);
    }

    public void Reset()
    {
        _currentX = _currentY = 0;
    }
}