using RECode.REFramework;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RECode.Editor.BTEditor
{
    /// <summary>
    /// 行为树节点搜索窗口：右键搜索 / 分类筛选 / 创建任意节点类型
    /// </summary>
    public class BTNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private BehaviorTreeGraphView _graphView;
        private Texture2D _indentIcon;

        public void Initialize(BehaviorTreeGraphView graphView)
        {
            _graphView = graphView;
            // 1x1透明纹理用作缩进占位，避免 ArgumentNullException
            _indentIcon = new Texture2D(1, 1);
            _indentIcon.SetPixel(0, 0, Color.clear);
            _indentIcon.Apply();
        }

        /// <summary>
        /// ISearchWindowProvider 入口：构建搜索树
        /// </summary>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("创建行为树节点"), 0)
            };

            //TODO:新增自定义节点类型时，在此更新搜索窗口
            // ── 组合节点 ──
            entries.Add(new SearchTreeGroupEntry(new GUIContent("组合节点 (Composite)"), 1));
            entries.Add(MakeEntry("序列 Sequence", E_BTNodeType.Sequence, 2));
            entries.Add(MakeEntry("选择器 Selector", E_BTNodeType.Selector, 2));
            entries.Add(MakeEntry("主动选择器 ActiveSelector", E_BTNodeType.ActiveSelector, 2));
            entries.Add(MakeEntry("并行 Parallel", E_BTNodeType.Parallel, 2));
            entries.Add(MakeEntry("监控器 Monitor", E_BTNodeType.Monitor, 2));

            // ── 装饰节点 ──
            entries.Add(new SearchTreeGroupEntry(new GUIContent("装饰节点 (Decorator)"), 1));
            entries.Add(MakeEntry("取反 Inverter", E_BTNodeType.Inverter, 2));
            entries.Add(MakeEntry("重复 Repeat", E_BTNodeType.Repeat, 2));
            entries.Add(MakeEntry("延时 Delay", E_BTNodeType.Delay, 2));

            // ── 叶子节点 ──
            entries.Add(new SearchTreeGroupEntry(new GUIContent("叶子节点 (Leaf)"), 1));
            entries.Add(MakeEntry("动作节点 Action", E_BTNodeType.Action, 2));

            return entries;
        }

        /// <summary>
        /// 选中条目后的回调：在画布上创建节点并选中
        /// </summary>
        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is not E_BTNodeType nodeType) return false;

            // 屏幕坐标 → 画布坐标
            var worldMousePos = _graphView.EditorWindow.rootVisualElement.ChangeCoordinatesTo(
                _graphView.EditorWindow.rootVisualElement.parent,
                context.screenMousePosition - _graphView.EditorWindow.position.position
            );
            var graphMousePos = _graphView.contentViewContainer.WorldToLocal(worldMousePos);

            // 创建数据 + 视图
            var nodeData = _graphView.CreateNodeData(nodeType);
            nodeData.graphPosition = graphMousePos;
            _graphView.CreateAndAddNodeView(nodeData);

            return true;
        }

        private SearchTreeEntry MakeEntry(string label, E_BTNodeType type, int level)
        {
            return new SearchTreeEntry(new GUIContent(label, _indentIcon))
            {
                level = level,
                userData = type
            };
        }
    }
}
