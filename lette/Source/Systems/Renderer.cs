using System;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.MathF;

namespace Lette.Systems
{
    public class Renderer : IEcsRunSystem
    {
        SpriteBatch batch = null;
        CossinLette game = null;
        SpatialMap<EcsEntity> spatialMap = null;
        EcsFilter<Tiles, Pos> positionedTiles = null;
        EcsFilter<Camera, Pos> cameras = null;

        public void RenderSprites(AABB region, float zend, float zextent)
        {
            foreach (var entity in spatialMap.Region(region)) {
                // TODO 'A fonciton juste parce que ya que les sprites qui ont des AABBs
                var entityPos = entity.Get<Pos>().Value * Constants.PIXELS_PER_METER;
                ref var sprite = ref entity.Get<Sprite>();

                var sheet = sprite.Sheet;
                if (sheet == null)
                    continue;
                var entry = sheet.Entries[sprite.Entry];
                var strip = entry.Strips[sprite.Strip];
                var tile = strip.Tiles[sprite.Tile];

                batch.Draw(
                    entry.Texture,
                    entityPos,
                    tile.Quad,
                    Color.White,
                    0,
                    entry.Decal,
                    tile.Scale,
                    SpriteEffects.None,
                    (zend - entityPos.Y) / zextent);
            }
        }

        public void RenderTilesets(AABB region, float zend, float zextent)
        {
            foreach (var j in positionedTiles)
            {
                ref var tiles = ref positionedTiles.Get1(j);
                var tileset = tiles.Tileset;
                if (tileset == null)
                    continue;

                var tileSize = tileset.Size.ToVector2();

                var tpos = positionedTiles.Get2(j).Value * Constants.PIXELS_PER_METER;
                // TODO This should only be tileSize / 2????
                var tregion = ((region - tpos + tileSize) / tileSize).Round();

                var w = Math.Min(tiles.Idx.GetLength(0), tregion.X + tregion.Width);
                var h = Math.Min(tiles.Idx.GetLength(1), tregion.Y + tregion.Height);
                var d = tiles.Idx.GetLength(2);

                for (var x = Math.Max(0, tregion.X); x < w; x++)
                {
                    var posX = tpos.X + x * tileset.Size.X;
                    for (int y = Math.Max(0, tregion.Y); y < h; y++)
                    {
                        var posY = tpos.Y + y * tileset.Size.Y;
                        for (int z = 0; z < d; z++)
                        {
                            var tile = tiles.Idx[x, y, z];
                            var entry = tileset.Entries[tile.Entry];
                            var quad = entry.Quads[tile.Idx.X, tile.Idx.Y + entry.FrameTile];
                            var layer = 1f;

                            if (tile.Height > 0)
                                layer = (zend - ((tile.Height - 1) * tileSize.Y + posY)) / zextent;

                            batch.Draw(
                                entry.Texture,
                                new Vector2(posX, posY),
                                quad,
                                Color.White,
                                0,
                                tileSize,
                                Vector2.One,
                                SpriteEffects.None,
                                layer);
                        }
                    }
                }
            }
        }

        public void Run()
        {
            var viewport = game.GraphicsDevice.Viewport;
            var bounds = game.GraphicsDevice.Viewport.Bounds;
            var n = cameras.GetEntitiesCount();
            if (n == 0) {
                return;
            }
            var cols = (int)Ceiling(Sqrt(n));
            var rows = (int)Ceiling(n / cols);
            // TODO take into account lost decimals.
            var camSize = new Point(
                game.GraphicsDevice.Viewport.Width / cols,
                game.GraphicsDevice.Viewport.Height / rows);
            var halfCamSize = camSize.ToVector2() / 2;

            foreach (var i in cameras)
            {
                game.GraphicsDevice.Viewport = new Viewport(new Rectangle(
                    new Point(i % cols, (int)(i / cols)) * camSize,
                    camSize));

                // TODO LOL
                /*var test = new Texture2D(game.GraphicsDevice, 1, 1);
                test.SetData(new[] { Color.White });
                batch.Begin();
                batch.Draw(test, new Rectangle(Point.Zero, camSize), null, new Color(0, 0, i * 64));
                batch.End();*/

                var pos = (cameras.Get2(i).Value * Constants.PIXELS_PER_METER);
                var region = new AABB { Min = pos - halfCamSize, Max = pos + halfCamSize };

                batch.Begin(
                    SpriteSortMode.BackToFront,
                    BlendState.NonPremultiplied,
                    SamplerState.AnisotropicClamp,
                    DepthStencilState.Default,
                    RasterizerState.CullNone,
                    null,
                    Matrix.CreateTranslation(new Vector3(-region.Min, 0)));

                var zend = region.Max.Y + camSize.Y;
                var zextent = camSize.Y * 3;

                RenderSprites(region, zend, zextent);
                RenderTilesets(region, zend, zextent);

                batch.End();
            }

            game.GraphicsDevice.Viewport = viewport;
        }
    }
}
