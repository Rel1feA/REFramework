using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    //设计成部分类，可以在创建新的节点的时候，顺便在构建器增加新的节点
    public partial class BehaviorTreeBuilder
    {
        private readonly Stack<BehaviorNode> nodeStack;
        private readonly BehaviorTree bhTree;
        public BehaviorTreeBuilder()
        {
            bhTree = new BehaviorTree(null);
            nodeStack = new Stack<BehaviorNode>();
        }

        private void AddBehavior(BehaviorNode behavior)
        {
            if(bhTree.HaveRoot)
            {
                nodeStack.Peek().AddChild(behavior);
            }
            else
            {
                bhTree.SetRoot(behavior);
            }
            if(behavior is CompositeNode||behavior is DecoratorNode)
            {
                nodeStack.Push(behavior);
            }
        }

        public void TreeTick()
        {
            bhTree.Tick();
        }

        public BehaviorTreeBuilder Back()
        {
            nodeStack.Pop();
            return this;
        }

        public BehaviorTree End()
        {
            nodeStack.Clear();
            return bhTree;
        }

        public BehaviorTreeBuilder Sequence()
        {
            SequenceNode node=new SequenceNode();
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder Selector()
        {
            SelectorNode node=new SelectorNode();
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder Parallel(ParallelNode.E_Policy success,ParallelNode.E_Policy failure)
        {
            ParallelNode node=new ParallelNode(success,failure);
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder Monitor(ParallelNode.E_Policy success, ParallelNode.E_Policy failure)
        {
            MonitorNode node=new MonitorNode(success,failure);
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder ActiveSelector()
        {
            ActiveSelector node=new ActiveSelector();
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder Repeat(int limit)
        {
            RepeatNode node=new RepeatNode(limit);
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder Inverter()
        {
            InverterNode node=new InverterNode();
            AddBehavior(node);
            return this;
        }
    }
}


