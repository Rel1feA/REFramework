using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RECode.REFramework
{
    //此类为动作节点，可自行发挥设计，具体实现由自己决定，以下只是范例
    public class ActionNode : BehaviorNode
    {
        private string actionName;
        public ActionNode(string eventName,Blackboard blackboard):base(blackboard)
        {
            this.actionName = eventName;
        }

        protected override E_BehaviorState OnUpdate()
        {
            UnityAction action=blackboard.GetValue<UnityAction>(actionName);
            if (action != null)
            {
                action.Invoke();
                return E_BehaviorState.Success;
            }
            else
            {
                return E_BehaviorState.Failure;
            }
        }
    }

    public class ConditionNode:BehaviorNode
    {
        private string boolValKey;
        public ConditionNode(string boolValKey,Blackboard blackboard):base (blackboard)
        {
            this.boolValKey = boolValKey;
        }

        protected override E_BehaviorState OnUpdate()
        {
            return blackboard.GetValue<bool>(boolValKey)?E_BehaviorState.Success:E_BehaviorState.Failure;
        }
    }

    public class DebugNode:BehaviorNode
    {
        private string word;
        public DebugNode(string word,Blackboard blackboard):base(blackboard)
        {
            this.word = word;
        }

        protected override E_BehaviorState OnUpdate()
        {
            Debug.Log(word);
            return E_BehaviorState.Success;
        }
    }


    //设计成部分类，可以在创建新的节点的时候，顺便在构建器增加新的节点
    public partial class BehaviorTreeBuilder
    {
        public BehaviorTreeBuilder ActionNode(string actionName)
        {
            ActionNode node= new ActionNode(actionName,bhTree.blackboard);  
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder ConditionNode(string boolValKey)
        {
            ConditionNode node = new ConditionNode(boolValKey,bhTree.blackboard);
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder DebugNode(string word)
        {
            DebugNode node = new DebugNode(word, bhTree.blackboard);
            AddBehavior(node);
            return this;
        }
    }
}


