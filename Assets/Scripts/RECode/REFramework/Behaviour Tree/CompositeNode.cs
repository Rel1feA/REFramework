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
    public class SequenceNode : CompositeNode
    {
        protected LinkedListNode<BehaviorNode> currentChild;
        protected override void OnInitialize()
        {
            currentChild = children.First;
        }
        protected override E_BehaviorState OnUpdate()
        {
            while (true)
            {
                var s = currentChild.Value.Tick();
                if (s != E_BehaviorState.Success) return s;
                currentChild = currentChild.Next;
                if (currentChild == null) return E_BehaviorState.Success;
            }
        }
    }
    public class SelectorNode : SequenceNode
    {
        protected override E_BehaviorState OnUpdate()
        {
            while (true)
            {
                var s = currentChild.Value.Tick();
                if (s != E_BehaviorState.Failure) return s;
                currentChild = currentChild.Next;
                if (currentChild == null) return E_BehaviorState.Failure;
            }
        }
    }
    public class ParallelNode : CompositeNode
    {
        public enum E_Policy
        {
            RequireOne, RequireAll
        }

        protected E_Policy mSuccessPolicy;//成功的标准
        protected E_Policy mFailurePolicy;//失败的标准

        public ParallelNode(E_Policy mSuccessPolicy, E_Policy mFailurePolicy)
        {
            this.mSuccessPolicy = mSuccessPolicy;
            this.mFailurePolicy = mFailurePolicy;
        }

        protected override E_BehaviorState OnUpdate()
        {
            int successCount = 0, failureCount = 0;
            var b = children.First;
            var size = children.Count;
            for (int i = 0; i < size; i++)
            {
                var node = b.Value;
                if (!node.IsTerminated) node.Tick();
                b = b.Next;
                if (node.IsSuccess)
                {
                    successCount++;
                    if (mSuccessPolicy == E_Policy.RequireOne)
                        return E_BehaviorState.Success;
                }
                if (node.IsFailure)
                {
                    failureCount++;
                    if (mFailurePolicy == E_Policy.RequireOne)
                        return E_BehaviorState.Failure;
                }
            }
            if (mFailurePolicy == E_Policy.RequireAll && failureCount == size)
                return E_BehaviorState.Failure;
            if (mSuccessPolicy == E_Policy.RequireAll && successCount == size)
                return E_BehaviorState.Success;
            return E_BehaviorState.Running;
        }

        protected override void OnTerminate()
        {
            foreach (var node in children)
            {
                if (node.IsRunning) node.Abort();
            }
        }
    }
    public class MonitorNode : ParallelNode
    {
        public MonitorNode(E_Policy mSuccessPolicy, E_Policy mFailurePolicy) : base(mSuccessPolicy, mFailurePolicy)
        {

        }

        public void AddCondition(BehaviorNode condition)
        {
            children.AddFirst(condition);
        }
        public void AddAction(BehaviorNode action)
        {
            children.AddLast(action);
        }
    }
    public class ActiveSelector : SelectorNode
    {
        protected override E_BehaviorState OnUpdate()
        {
            var prev = currentChild;
            base.OnInitialize();
            var res = base.OnUpdate();
            if (prev != null && currentChild != prev)
            {
                prev.Value.Abort();
            }
            return res;
        }
    }
}


