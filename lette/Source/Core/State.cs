using Leopotam.Ecs;

namespace Lette.Core
{
    public interface IState
    {
        bool CapturesUpdate => true;

        void Init(CossinLette game) { }
        void Update() { }
        void Draw() { }
        void Destroy() { }
    }

    public abstract class EcsState : IState
    {
        internal CossinLette? game;
        internal EcsWorld? world;
        internal EcsSystems? updateSystems;
        internal EcsSystems? drawSystems;
        internal EcsSystems? systems;

        public abstract void InitSystems(out EcsSystems update, out EcsSystems draw, out EcsSystems systems);

        public virtual void Init(CossinLette game)
        {
            this.game = game;
            world = new();

            InitSystems(out updateSystems, out drawSystems, out systems);

            drawSystems
                .Inject(game.Batch)
                .Inject(game.Fonts);

            systems
                .Add(updateSystems)
                .Add(drawSystems)
                .Inject(game)
                .Inject(game.Watcher)
                .Inject(game.Step);

            systems.Init();
        }

        public virtual void Update() => updateSystems?.Run();
        public virtual void Draw() => drawSystems?.Run();

        public virtual void Destroy()
        {
            systems?.Destroy();
            world?.Destroy();
        }
    }
}

/*public struct StateTransition
{
    public Color Color;
    public TimeSpan Out, In;
    public bool ShouldUpdate;
    public IState State;
}

public interface IState
{
    bool CapturesUpdate { get; }

    void Enter() {}
    void Exit() {}

    void Init() {}
    void Update() {}
    void Draw() {}
    void Destroy() {}
}

public class StateManager
{
    public Stack<IState> States = new Stack<IState>();
    public Queue<StateTransition> QueuedTransitions = new Queue<StateTransition>();

    public StateTransition? Transition;
    public TimeSpan Time;
    public bool HasSwitched;

    public void CheckTransition()
    {
        if (!HasSwitched && Time > Transition.Value.Out)
        {
            Time = TimeSpan.Zero;
            HasSwitched = true;
            States.Push(Transition.Value.State);
        }
        if (HasSwitched && Time > Transition.Value.In)
        {
            if (QueuedTransitions.Count > 0)
                StartTransition(QueuedTransitions.Dequeue());
            else
                Transition = null;
        }
    }

    public void StartTransition(StateTransition transition)
    {
        Transition = transition;
        Time = TimeSpan.Zero;
        HasSwitched = false;
        CheckTransition();
    }

    public void Push(StateTransition transition)
    {
        if (Transition.HasValue)
            QueuedTransitions.Enqueue(transition);
        else
            StartTransition(transition);
    }

    public void Initialize()
    {
    }

    public void Update(GameTime gameTime)
    {
        if (Transition.HasValue)
        {
            Time += gameTime.ElapsedGameTime;
            CheckTransition();
        }

        foreach (var state in States.TakeUntil(s => s.CapturesUpdate))
            state.Update();
    }

    public void Draw()
    {
        foreach (var state in States.Reverse())
            state.Draw();
    }
}*/
