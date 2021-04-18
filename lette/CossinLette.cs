using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Lette.Core;
using Lette.States;
using System.Linq;
using System.IO;
using Lette.Resources;
using System;
using FontStashSharp;
using System.Text.Json;

namespace Lette
{
    public class CossinLette : Game
    {
        public Queue<IState> QueuedStates = new Queue<IState>();
        public Stack<IState> States = new Stack<IState>();

        public GraphicsDeviceManager Graphics;
        public SpriteBatch? Batch;
        public FileSystemWatcher? Watcher;
        public FontSystem? Fonts;
        public TimeSpan Step = TimeSpan.FromSeconds(1) / 60;
        public Init? Init;

        public CossinLette()
        {
            Graphics = new GraphicsDeviceManager(this);
            QueuedStates.Enqueue(new GameState());
        }

        protected override void Initialize()
        {
            Window.Title = "Cossin Lette";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            Batch = new(GraphicsDevice);
            Watcher = new()
            {
                Path = "Content",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            Fonts = FontSystemFactory.CreateStroked(GraphicsDevice, 1);
            Fonts.AddFont(File.ReadAllBytes("Content/fonts/Montserrat-Medium.ttf"));

            Init = JsonSerializer.Deserialize<Init>(File.ReadAllText($"Content/init.json"), JsonSerialization.GetOptions());

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            Batch?.Dispose();
            Watcher?.Dispose();

            base.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            foreach (var state in QueuedStates)
            {
                state.Init(this);
                States.Push(state);
            }
            QueuedStates.Clear();

            foreach (var state in States.TakeUntil(s => s.CapturesUpdate))
                state.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            foreach (var state in States.Reverse())
                state.Draw();

            base.Draw(gameTime);
        }
    }
}
