using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public static class BTNodeFactory
    {
        public static Dictionary<string, BehaviorNode> LastBuildMap { get; private set; }

        public static BehaviorNode Build(BTNodeData data,Blackboard blackboard)
        {
            LastBuildMap = new Dictionary<string, BehaviorNode>();
            return BuildInternal(data, blackboard);
        }

        private static BehaviorNode BuildInternal(BTNodeData data,Blackboard blackboard)
        {
            if (data == null) return null;
            BehaviorNode node = CreateNode(data, blackboard);
            LastBuildMap[data.guid] = node;
            foreach (var childData in data.children)
            {
                var child = BuildInternal(childData, blackboard);
                if (child != null) node.AddChild(child);
            }
            return node;
        }

        private static BehaviorNode CreateNode(BTNodeData data,Blackboard blackboard)
        {
            //TODO:新增自定义节点类型时，注册工厂构造函数
            switch (data.nodeType)
            {
                case E_BTNodeType.Sequence:
                    return new SequenceNode(blackboard);
                case E_BTNodeType.Selector:
                    return new SelectorNode(blackboard);
                case E_BTNodeType.ActiveSelector:
                    return new ActiveSelector(blackboard);
                case E_BTNodeType.Parallel:
                    return new ParallelNode(data.successPolicy, data.failurePolicy,blackboard);
                case E_BTNodeType.Monitor:
                    return new MonitorNode(data.successPolicy,data.failurePolicy, blackboard);
                case E_BTNodeType.Repeat:
                    return new RepeatNode(data.paramInt, blackboard);
                case E_BTNodeType.Inverter:
                    return new InverterNode(blackboard);
                case E_BTNodeType.Delay:
                    return new DelayNode(data.paramFloat, blackboard);
                case E_BTNodeType.Action:
                    return new ActionNode(data.actionParamJson ?? string.Empty,blackboard);
                case E_BTNodeType.Condition:
                    return new ConditionNode(data.actionParamJson ?? string.Empty,blackboard);
                case E_BTNodeType.Debug:
                    return new DebugNode(data.actionParamJson ?? string.Empty, blackboard);
                default:
                    return null;
            }
        }
    }
}

