using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lette.Core;
using Lette.Components;
using Lette.Systems;
using Lette.Resources;
using Leopotam.Ecs;

namespace Lette
{
    public class GameState
    {
        public EcsWorld World;
        public EcsSystems UpdateSystems;
        public EcsSystems DrawSystems;
        public EcsSystems Systems;

        public void Init()
        {
            World = new EcsWorld();

            UpdateSystems = new EcsSystems(World)
                .Add(new Sheets());

            DrawSystems = new EcsSystems(World);

            Systems
                .Add(UpdateSystems)
                .Add(DrawSystems);

            Systems.Init();
        }

        public void Update()
        {
            UpdateSystems.Run();
        }
        public void Draw()
        {
            DrawSystems.Run();
        }

        public void Destroy()
        {
            Systems.Destroy();
            World.Destroy();
        }
    }

    public class CossinLette : Game
    {
        public static Sheet Cossin = new Sheet
        {
            Entries = new SheetEntry[]
            {
                new SheetEntry
                {
                    Src = "cossin-neutre"
                },
                new SheetEntry
                {

                }
            }
        };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D test;
        SpriteFont font;

        public CossinLette()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            test = Content.Load<Texture2D>("img/cossin/cossin-marche");
            font = Content.Load<SpriteFont>("fonts/Montserrat-Medium");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(test, Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "Woo", Vector2.One * 48, Color.Black);

            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
