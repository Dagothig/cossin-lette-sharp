using System;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Lette.Systems
{
    public class Renderer : IEcsRunSystem
    {
        SpriteBatch batch = null;
        CossinLette game = null;
        SpatialMap<EcsEntity> spatialMap = null;
        EcsFilter<Camera, Pos> cameras = null;

        public void Run()
        {
            var bounds = game.GraphicsDevice.Viewport.Bounds;
            var n = cameras.GetEntitiesCount();
            if (n == 0) {
                return;
            }
            var cols = (int)MathF.Ceiling(MathF.Sqrt(n));
            var rows = (int)MathF.Ceiling(n / cols);
            // TODO take into account lost decimals.
            var camSize = new Point(
                game.GraphicsDevice.Viewport.Width / cols,
                game.GraphicsDevice.Viewport.Height / rows);

            foreach (var i in cameras)
            {
                ref var camera = ref cameras.Get1(i);
                ref var pos = ref cameras.Get2(i);
                var camPos = new Point(i % cols, (int)(i / cols));

                var prevScissors = game.GraphicsDevice.ScissorRectangle;
                game.GraphicsDevice.ScissorRectangle = new Rectangle(camPos, camSize);

                batch.Begin(
                    SpriteSortMode.BackToFront,
                    BlendState.AlphaBlend,
                    SamplerState.PointWrap,
                    DepthStencilState.None,
                    RasterizerState.CullNone,
                    null,
                    Matrix.CreateTranslation(new Vector3(
                        camSize.ToVector2() / 2 - pos + camPos.ToVector2(),
                        0f)));

                var boundsCenter = bounds.Center.ToVector2();
                foreach (var entity in spatialMap.Region((AABB)bounds + pos - boundsCenter, true)) {
                    ref var entityPos = ref entity.Get<Pos>();
                    ref var sprite = ref entity.Get<Sprite>();

                    var sheet = sprite.Sheet;
                    var entry = sheet.Entries[sprite.Entry];
                    var strip = entry.Strips[sprite.Strip];
                    var tile = strip.Tiles[sprite.Tile];

                    batch.Draw(
                        sheet.Texture,
                        entityPos,
                        tile.Quad,
                        Color.White,
                        0,
                        entry.Decal,
                        tile.Scale,
                        SpriteEffects.None,
                        entityPos.Value.Y);
                }

                batch.End();

                game.GraphicsDevice.ScissorRectangle = prevScissors;
            }
        }
    }
}
