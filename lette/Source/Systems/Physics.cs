using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Dynamics;

namespace Lette.Systems
{
    public class Physics
    {
        public Vector2 Gravity;

        World world;

        public void Initialize()
        {
            world = new World(Gravity);
        }

        public void Update(GameTime gameTime)
        {
            world.Step(gameTime.ElapsedGameTime);
        }
    }
}
