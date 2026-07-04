using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode
{
    namespace REFramework
    {
        public class State<T>
        {
            public virtual void EnterState(T type) { }

            public virtual void ExitState(T type) { }

            public virtual void FrameUpdate(T type) { }

            public virtual void PhysicsUpdate(T type) { }

            public virtual State<T> ChangeState(T type) {  return null; }
        }
    }
}


