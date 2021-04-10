using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Lette.Core
{
    public interface IState
    {
        bool CapturesUpdate { get; }

        void Init(CossinLette game) {}
        void Update() {}
        void Draw() {}
        void Destroy() {}
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
