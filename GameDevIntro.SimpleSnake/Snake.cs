using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
namespace GameDevIntro.SimpleSnake;
internal class Snake
{
    #region Variables and properties
    private List<Point> _segmentLocations = new();
    public Point Direction { get; set; }
    public Point DesiredDirection { get; set; }
    public Color Color { get; set; } = Color.White;
    private bool _growing = false;
    public Vector2 SegmentSize { get; set; }
    public Texture2D SegmentTexture { get; set; }
    public Point HeadPosition { get => _segmentLocations[0]; }
    private float _msPerMove = 200, _msLeftBeforeMove;
    #endregion

    #region Constructor
    public Snake(Texture2D segmentTexture, Point headLocation, Point direction, int movesPerSecond, Vector2 _segmentSize, int initialLength = 2)
    {
        SegmentTexture = segmentTexture;
        var currentLocation = headLocation;
        Direction = DesiredDirection = direction;
        _msPerMove = 1000f / movesPerSecond;
        SegmentSize = _segmentSize;

        while (_segmentLocations.Count < initialLength)
        {
            _segmentLocations.Add(currentLocation);
            currentLocation -= direction;
        }
    } 
    #endregion

    #region Updating and Movement
    public void Update(GameTime gameTime)
    {
        _msLeftBeforeMove -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        if (_msLeftBeforeMove > 0)
            return;
        if (DesiredDirection != Point.Zero && DesiredDirection != SimpleSnakeGame.OppositeDirections[Direction])
        {
            Direction = DesiredDirection;
        }
        Move(Direction);

        _msLeftBeforeMove += _msPerMove + _msLeftBeforeMove;
    }


    private void Move(Point direction)
    {
        var lastSegment = _segmentLocations[^1];
        for (int i = _segmentLocations.Count - 1; i > 0; i--)
        {
            _segmentLocations[i] = _segmentLocations[i - 1];
        }
        _segmentLocations[0] += direction;
        if (_growing)
        {
            _segmentLocations.Add(lastSegment);
            _growing = false;
        }
    }
    public void Grow() => _growing = true;

    #endregion

    #region Methods for checking overlapping areas
    public bool BodyOverlapsPosition(Point position)
    {
        foreach (var segment in _segmentLocations)
        {
            if (segment == position)
                return true;
        }
        return false;
    }
    public bool IsHeadCollidingWithBody()
    {
        var headPosition = HeadPosition;
        for (int i = 1; i < _segmentLocations.Count; i++)
        {
            if (_segmentLocations[i] == headPosition)
                return true;
        }
        return false;
    }
    #endregion

    #region Draw()
    public void Draw(SpriteBatch spriteBatch, Vector2 offset)
    {
        foreach (var segmentLocation in _segmentLocations)
        {
            var drawPosition = offset + new Vector2(segmentLocation.X * SegmentSize.X, segmentLocation.Y * SegmentSize.Y);
            spriteBatch.Draw(SegmentTexture, new Rectangle((int)drawPosition.X, (int)drawPosition.Y, (int)SegmentSize.X, (int)SegmentSize.Y), Color);
        }
    } 
    #endregion
}