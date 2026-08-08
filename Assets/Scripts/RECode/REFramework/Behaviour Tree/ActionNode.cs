using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    //此类为动作节点，可自行发挥设计，具体实现由自己决定，以下只是范例
    public class ActionNode : BehaviorNode
    {
        private string eventName;
        public ActionNode(string eventName)
        {
            this.eventName = eventName;
        }

        protected override E_BehaviorState OnUpdate()
        {
            EventCenter.Instance.EventTrigger(eventName);
            return E_BehaviorState.Success;
        }
    }

    public class ConditionNode:BehaviorNode
    {
        private string funcName;
        public ConditionNode(string funcName)
        {
            this.funcName = funcName;
        }

        protected override E_BehaviorState OnUpdate()
        {
            return EventCenter.Instance.FuncTrigger<bool>(funcName)?E_BehaviorState.Success:E_BehaviorState.Failure;
        }
    }

    public class DebugNode:BehaviorNode
    {
        private string word;
        public DebugNode(string word)
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
            ActionNode node= new ActionNode(actionName);  
            AddBehavior(node);
            return this;
        }

        public BehaviorTreeBuilder ConditionNode(string funcName)
        {
            ConditionNode node = new ConditionNode(funcName);
            AddBehavior(node);
            return this;
        }
    }
}


