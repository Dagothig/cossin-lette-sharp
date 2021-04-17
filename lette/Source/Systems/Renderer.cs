using System;
using Leopotam.Ecs;
using Lette.Components;
using Lette.Core;
using Lette.Resources;
using Lette.Lib.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Aether = tainicom.Aether.Physics2D;
using static System.MathF;

namespace Lette.Systems
{
    public class Renderer : IEcsRunSystem
    {
        SpriteBatch? batch = null;
        CossinLette? game = null;
        EcsFilter<Pos, Sprite, AABB>? sprites = null;
        EcsFilter<Tiles, Pos>? tileses = null;
        EcsFilter<Camera, Pos>? cameras = null;
        GenArr<Tileset>? tilesets= null;
        GenArr<Sheet>? sheets = null;
        Aether.Dynamics.World? world = null;
        DebugView? debugView = null;

        public void RenderSprites(AABB region, float zend, float zextent)
        {
            if (sprites != null && batch != null) foreach (var i in sprites)
            {
                ref var aabb = ref sprites.Get3(i);
                if (!region.Overlaps(aabb))
                    continue;

                var pos = sprites.Get1(i).Value * Constants.PIXELS_PER_METER;
                ref var sprite = ref sprites.Get2(i);

                // TODO AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAaa

                var sheet = sheets?[sprite.SheetIdx];
                if (sheet == null)
                    continue;
                var entry = sheet.Entries[sprite.Entry % sheet.Entries.Length];
                if (entry == null || entry.Texture == null)
                    continue;
                var strip = entry.Strips[sprite.Strip % entry.Strips.Length];
                var tile = strip?.Tiles[sprite.Tile % strip.Tiles.Length];
                if (tile == null)
                    continue;

                batch.Draw(
                    entry.Texture,
                    pos,
                    tile.Quad,
                    Color.White,
                    0,
                    entry.Decal,
                    tile.Scale,
                    SpriteEffects.None,
                    (zend - pos.Y) / zextent);
            }
        }

        public void RenderTilesets(AABB region, float zend, float zextent)
        {
            if (tileses != null && batch != null) foreach (var j in tileses)
            {
                ref var tiles = ref tileses.Get1(j);
                var tileset = tilesets?[tiles.TilesetIdx];
                if (tileset == null || tileset.Entries == null)
                    continue;

                var tileSize = tileset.Size.ToVector2();

                var tpos = tileses.Get2(j).Value * Constants.PIXELS_PER_METER;
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
            if (game == null || cameras == null || batch == null)
                return;
            var viewport = game.GraphicsDevice.Viewport;
            var bounds = game.GraphicsDevice.Viewport.Bounds;
            var n = cameras.GetEntitiesCount();
            if (n == 0) {
                return;
            }
            var cols = (int)Ceiling(Sqrt(n));
            var rows = (int)Ceiling(n / (float)cols);
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

                var pos = (cameras.Get2(i).Value * Constants.PIXELS_PER_METER);
                var region = new AABB { Min = pos - halfCamSize, Max = pos + halfCamSize };
                var transform = Matrix.CreateTranslation(new Vector3(-region.Min, 0));

                batch.Begin(
                    SpriteSortMode.BackToFront,
                    BlendState.NonPremultiplied,
                    SamplerState.AnisotropicClamp,
                    DepthStencilState.Default,
                    RasterizerState.CullNone,
                    null,
                    transform);

                var zend = region.Max.Y + camSize.Y;
                var zextent = camSize.Y * 3;

                RenderSprites(region, zend, zextent);
                RenderTilesets(region, zend, zextent);

                batch.End();

                debugView?.RenderDebugData(
                    Matrix.CreateOrthographicOffCenter(
                        0, game.GraphicsDevice.Viewport.Width,
                        game.GraphicsDevice.Viewport.Height, 0,
                        0, 1),
                    Matrix.CreateScale(Constants.PIXELS_PER_METER) * transform);
            }

            game.GraphicsDevice.Viewport = viewport;
        }
    }
}
