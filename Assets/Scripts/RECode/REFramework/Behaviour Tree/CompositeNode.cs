using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public abstract class CompositeNode : BehaviorNode
    {
        protected LinkedList<BehaviorNode> children;

        public CompositeNode()
        {
            children = new LinkedList<BehaviorNode>();
        }

        public virtual void RemoveChild(BehaviorNode child)
        {
            children.Remove(child);
        }

        public void ClearChildren()
        {
            children.Clear();
        }

        public override void AddChild(BehaviorNode child)
        {
            children.AddLast(child);
        }
    }
}


