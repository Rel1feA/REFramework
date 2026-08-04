using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public class SequenceNode : CompositeNode
    {
        protected LinkedListNode<BehaviorNode> currentChild;
        protected override void OnInitialize()
        {
            currentChild = children.First;
        }
        protected override E_BehaviorState OnUpdate()
        {
            while(true)
            {
                var s = currentChild.Value.Tick();
                if (s != E_BehaviorState.Success) return s;
                currentChild = currentChild.Next;
                if (currentChild == null) return E_BehaviorState.Success;
            }
        }
    }
}

