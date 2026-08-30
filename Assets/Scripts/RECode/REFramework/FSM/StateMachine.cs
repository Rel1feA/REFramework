using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class StateMachine<T>
    {
        private State<T> currentState;

        public State<T> CurrentState { get { return currentState; } }

        public void Init(State<T> state)
        {
            currentState = state;
            currentState.EnterState();
        }

        public void CheckChangeState()
        {
            State<T> state = currentState.ChangeState();
            if (state == null) return;
            currentState.ExitState();
            currentState = state;
            currentState.EnterState();
        }
    }
}


