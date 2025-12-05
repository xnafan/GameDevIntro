using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameDevIntro.SimpleZuul.Model;
internal class Tile
{
    public enum TileType {Empty, Wall, Slime, Skeleton, Dragon, Chest }

    public TileType Type { get; set; }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {

    }

}
