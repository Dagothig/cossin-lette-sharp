using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Lette.Core;
using Lette.States;
using System.Linq;

namespace Lette
{
    public class CossinLette : Game
    {
        public Queue<IState> QueuedStates = new Queue<IState>();
        public Stack<IState> States = new Stack<IState>();

        public GraphicsDeviceManager Graphics;

        public CossinLette()
        {
            Content.RootDirectory = "Content";
            Graphics = new GraphicsDeviceManager(this);
            QueuedStates.Enqueue(new GameState(this));
        }

        protected override void Initialize()
        {
            Window.Title = "Cossin Lette";
            base.Initialize();
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
