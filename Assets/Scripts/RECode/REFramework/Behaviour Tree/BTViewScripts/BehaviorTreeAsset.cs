using System.Collections.Generic;
using UnityEngine;

namespace RECode.REFramework
{
    [CreateAssetMenu(fileName = "NewBehaviorTree", menuName = "BehaviorTree/TreeAsset")]
    public class BehaviorTreeAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        [System.NonSerialized]
        public BTNodeData rootNode;

        public List<BTNodeData> allNodes = new List<BTNodeData>();

        [TextArea(2, 5)]
        public string description;

        public Dictionary<string, BTNodeData> NodeMap
        {
            get
            {
                if (_nodeMap == null || _nodeMap.Count != allNodes.Count)
                    RebuildNodeMap();
                return _nodeMap;
            }
        }
        private Dictionary<string, BTNodeData> _nodeMap;

        public void RebuildNodeMap()
        {
            _nodeMap = new Dictionary<string, BTNodeData>();
            foreach (var node in allNodes)
                if (!string.IsNullOrEmpty(node.guid))
                    _nodeMap[node.guid] = node;
        }

        public void AddNode(BTNodeData node)
        {
            allNodes.Add(node);
            _nodeMap?.Add(node.guid, node);
        }

        public void RemoveNode(BTNodeData node)
        {
            allNodes.Remove(node);
            _nodeMap?.Remove(node.guid);
        }

        private void OnValidate()
        {
            RebuildNodeMap();
        }

        // 不做任何事：parentGuid 由 GraphView 在编辑时直接维护
        public void OnBeforeSerialize() { }

        // 反序列化后：从 parentGuid 重建 children 树
        public void OnAfterDeserialize()
        {
            var guidToNode = new Dictionary<string, BTNodeData>();
            rootNode = null;

            foreach (var node in allNodes)
            {
                if (string.IsNullOrEmpty(node.guid)) continue;
                node.children.Clear();
                node.parent = null;
                guidToNode[node.guid] = node;
            }

            foreach (var node in allNodes)
            {
                if (string.IsNullOrEmpty(node.parentGuid))
                {
                    // 多个根节点时取第一个
                    if (rootNode == null) rootNode = node;
                }
                else if (guidToNode.TryGetValue(node.parentGuid, out var parentNode))
                {
                    parentNode.children.Add(node);
                    node.parent = parentNode;
                }
            }

            RebuildNodeMap();
        }

        public BehaviorTree GetTree()
        {
            if (rootNode == null) return null;
            var root = BTNodeFactory.Build(rootNode);
            return new BehaviorTree(root);
        }
    }
}