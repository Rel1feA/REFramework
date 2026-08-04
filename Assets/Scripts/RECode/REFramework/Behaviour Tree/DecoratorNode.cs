using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public abstract class DecoratorNode : BehaviorNode
    {
        protected BehaviorNode child;
        public override void AddChild(BehaviorNode child)
        {
            this.child = child;
        }
    }
}


