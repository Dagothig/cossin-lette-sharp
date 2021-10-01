using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FontStashSharp;
using Lette.Core;
using Lette.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        public CossinLette()
        {
            Graphics = new GraphicsDeviceManager(this);
            QueuedStates.Enqueue(new EditorState());
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

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            Batch?.Dispose();
            Watcher?.Dispose();
            foreach (var state in States)
                state.Destroy();

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
