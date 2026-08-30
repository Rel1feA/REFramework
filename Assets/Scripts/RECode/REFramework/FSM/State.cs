using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class State<T>
    {
        public T value;

        public State(T t)
        {
            value = t;
        }

        public virtual void EnterState() { }

        public virtual void ExitState() { }

        public virtual void FrameUpdate() { }

        public virtual void PhysicsUpdate() { }

        public virtual State<T> ChangeState() { return null; }
    }
}


