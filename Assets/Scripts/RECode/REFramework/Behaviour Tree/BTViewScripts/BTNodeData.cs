using System;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    public enum E_BTNodeType
    {
        //TODO:新增自定义节点类型时，在此加一个枚举类型
        Sequence, Selector, ActiveSelector,Parallel, Monitor, 
        Inverter, Repeat,Delay,
        Action
    }

    [Serializable]
    public class BTNodeData
    {
        public string guid = Guid.NewGuid().ToString();
        public string nodeName;
        public E_BTNodeType nodeType;
        public Vector2 graphPosition;

        // ── 序列化用：父子关系靠 guid 关联，不嵌套 ──
        public string parentGuid;

        // ── 运行时用：反序列化后重建，不参与序列化 ──
        [NonSerialized]
        public List<BTNodeData> children = new List<BTNodeData>();

        [NonSerialized]
        public BTNodeData parent;

        // ── 节点参数 ──
        public ParallelNode.E_Policy successPolicy = ParallelNode.E_Policy.RequireOne;
        public ParallelNode.E_Policy failurePolicy = ParallelNode.E_Policy.RequireOne;
        public int paramInt = 1;
        public float paramFloat = 0f;
        public string actionTypeName;
        public string actionParamJson;

        public BTNodeData CloneWithoutRunTime()
        {
            var clone = (BTNodeData)MemberwiseClone();
            clone.guid = Guid.NewGuid().ToString();
            clone.children = new List<BTNodeData>();
            clone.parent = null;
            clone.parentGuid = null;
            return clone;
        }
    }
}