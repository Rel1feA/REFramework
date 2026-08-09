using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public abstract class DecoratorNode : BehaviorNode
    {


        protected BehaviorNode child;

        protected DecoratorNode(Blackboard blackboard) : base(blackboard)
        {
        }

        public override void AddChild(BehaviorNode child)
        {
            this.child = child;
        }
    }
    public class InverterNode : DecoratorNode
    {
        public InverterNode(Blackboard blackboard) : base(blackboard)
        {
        }

        protected override E_BehaviorState OnUpdate()
        {
            child.Tick();
            if (child.IsFailure) return E_BehaviorState.Success;
            if (child.IsSuccess) return E_BehaviorState.Failure;
            return E_BehaviorState.Running;
        }
    }
    public class RepeatNode : DecoratorNode
    {
        private int counter;
        private int limit;
        public RepeatNode(int limit,Blackboard blackboard):base(blackboard)
        {
            this.limit = limit;
        }

        protected override void OnInitialize()
        {
            counter = 0;
        }

        protected override E_BehaviorState OnUpdate()
        {
            while (true)
            {
                child.Tick();
                if (child.IsRunning)
                    return E_BehaviorState.Running;
                if (child.IsFailure)
                    return E_BehaviorState.Failure;
                if (++counter >= limit)
                    return E_BehaviorState.Success;
            }
        }
    }
    public class DelayNode:DecoratorNode
    {
        private float delaySeconds;
        private float _elapsed;

        public DelayNode(float delaySeconds,Blackboard blackboard):base (blackboard)
        {
            this.delaySeconds = delaySeconds;
        }

        protected override void OnInitialize()
        {
            _elapsed = 0;
        }

        protected override E_BehaviorState OnUpdate()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed < delaySeconds)
                return E_BehaviorState.Running;
            return child.Tick();
        }
    }

    public partial class BehaviorTreeBuilder
    {
        public BehaviorTreeBuilder DelayNode(float delaySecond)
        {
            DelayNode node = new DelayNode(delaySecond, bhTree.blackboard);
            AddBehavior(node);
            return this;
        }
    }
}


