using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class SelectorNode :SequenceNode
    {
        protected override E_BehaviorState OnUpdate()
        {
            while (true)
            {
                var s = currentChild.Value.Tick();
                if (s != E_BehaviorState.Failure) return s;
                currentChild =currentChild.Next;
                if (currentChild == null) return E_BehaviorState.Failure;
            }
        }
    }
}

