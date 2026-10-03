using System;
using System.Collections.Generic;

namespace ScramblyFoxDefense.Core
{
    /// <summary>One handler per session state. Handlers subscribe in Enter and unsubscribe in Exit.</summary>
    public interface IGameState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    /// <summary>Owns the session flow. States request transitions through it, never call each other.</summary>
    public sealed class GameStateMachine
    {
        readonly Dictionary<Type, IGameState> _states = new Dictionary<Type, IGameState>();
        IGameState _current;

        public IGameState Current => _current;

        public void Register(IGameState state) => _states[state.GetType()] = state;

        public void Enter<T>() where T : IGameState
        {
            _current?.Exit();
            _current = _states[typeof(T)];
            _current.Enter();
        }

        public void Tick(float deltaTime) => _current?.Tick(deltaTime);

        public void Stop()
        {
            _current?.Exit();
            _current = null;
        }
    }
}
