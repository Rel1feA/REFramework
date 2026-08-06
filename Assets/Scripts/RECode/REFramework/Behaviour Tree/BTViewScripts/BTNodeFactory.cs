using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public static class BTNodeFactory
    {
        /// <summary>guid → 运行时节点（编辑器调试用，Build 时自动填充）</summary>
        public static Dictionary<string, BehaviorNode> LastBuildMap { get; private set; }

        public static BehaviorNode Build(BTNodeData data)
        {
            LastBuildMap = new Dictionary<string, BehaviorNode>();
            return BuildInternal(data);
        }

        private static BehaviorNode BuildInternal(BTNodeData data)
        {
            if (data == null) return null;
            BehaviorNode node = CreateNode(data);
            LastBuildMap[data.guid] = node;
            foreach (var childData in data.children)
            {
                var child = BuildInternal(childData);
                if (child != null) node.AddChild(child);
            }
            return node;
        }

        private static BehaviorNode CreateNode(BTNodeData data)
        {
            //TODO:新增自定义节点类型时，注册工厂构造函数
            switch (data.nodeType)
            {
                case E_BTNodeType.Sequence:
                    return new SequenceNode();
                case E_BTNodeType.Selector:
                    return new SelectorNode();
                case E_BTNodeType.ActiveSelector:
                    return new ActiveSelector();
                case E_BTNodeType.Parallel:
                    return new ParallelNode(data.successPolicy, data.failurePolicy);
                case E_BTNodeType.Monitor:
                    return new MonitorNode(data.successPolicy,data.failurePolicy);
                case E_BTNodeType.Repeat:
                    return new RepeatNode(data.paramInt);
                case E_BTNodeType.Inverter:
                    return new InverterNode();
                case E_BTNodeType.Delay:
                    return new DelayNode(data.paramFloat);
                case E_BTNodeType.Action:
                    return new ActionNode(data.actionParamJson ?? string.Empty);
                default:
                    return null;
            }
        }
    }
}

