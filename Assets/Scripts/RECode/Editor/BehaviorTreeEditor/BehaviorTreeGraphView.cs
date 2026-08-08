using RECode.REFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RECode.Editor.BTEditor
{
    /// <summary>
    /// 行为树 GraphView 画布，负责：
    /// 1. 节点拖拽创建与删除
    /// 2. 端口连接 / 断开 —— 自动同步数据层的父子关系
    /// 3. 右键上下文菜单（创建 / 删除 / 复制 / 折叠 / 布局）
    /// 4. 选择根节点
    /// </summary>
    public class BehaviorTreeGraphView : GraphView
    {
        // ── 对外引用 ──
        public BehaviorTreeAsset Asset { get; private set; }
        public BehaviorTreeEditorWindow EditorWindow { get; private set; }

        // ── 内部状态 ──
        private BTNodeSearchWindow _searchWindow;
        private Dictionary<string, BTNodeView> _nodeViewMap = new();
        private Vector2 _defaultNodeSize = new(180, 120);

        // ── 拖拽时鼠标位置缓存（用于右键菜单→创建节点定位） ──
        private Vector2 _lastContextMenuMousePos;

        // ==================== 构造 & 初始化 ====================

        public BehaviorTreeGraphView()
        {
            // ── 操作能力 ──
            this.AddManipulator(new ContentDragger());           // 空白区拖拽平移
            this.AddManipulator(new SelectionDragger());         // 选中节点拖拽移动
            this.AddManipulator(new RectangleSelector());        // 框选
            this.AddManipulator(new ClickSelector());            // 点击选中
            this.AddManipulator(new FreehandSelector());         // 自由框选

            // ── 缩放 ──
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            // 滚动缩放阈值
            this.AddManipulator(new ContentZoomer());

            // ── 网格背景 ──
            var gridBg = new GridBackground { name = "GridBackground" };
            Insert(0, gridBg);
            gridBg.StretchToParentSize();

            // ── 搜索窗口初始化 ──
            _searchWindow = ScriptableObject.CreateInstance<BTNodeSearchWindow>();
            _searchWindow.Initialize(this);
            nodeCreationRequest += OnNodeCreationRequest;

            // ── GraphView 变化回调（处理增删边/节点） ──
            graphViewChanged += OnGraphViewChanged;

            // ── 右键菜单 ──
            RegisterContextMenu();

            // ── 样式（可选 USS） ──
            //styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(
            //    "Assets/Scripts/RECode/Editor/BehaviourTreeEditor/BehaviorTreeGraphView.uss"));
        }

        /// <summary>
        /// 绑定 EditorWindow 引用和加载 Asset
        /// </summary>
        public void Initialize(BehaviorTreeEditorWindow editorWindow, BehaviorTreeAsset asset)
        {
            EditorWindow = editorWindow;
            LoadAsset(asset);
        }

        // ==================== 数据 ↔ 视图 ====================

        /// <summary>
        /// 从 BehaviorTreeAsset 加载整棵树到画布
        /// </summary>
        public void LoadAsset(BehaviorTreeAsset asset)
        {
            Asset = asset;
            ClearGraph();

            if (asset?.rootNode == null) return;

            // 递归创建节点视图
            BuildNodeViewRecursive(asset.rootNode, null);

            // 所有节点创建完毕后，统一建边
            foreach (var view in _nodeViewMap.Values)
            {
                ConnectChildEdges(view);
            }

            // 延迟一帧强制重绘，确保边的位置计算完成
            schedule.Execute(() =>
            {
                MarkDirtyRepaint();
            }).ExecuteLater(1);
            schedule.Execute(() =>
            {
                FrameAllPublic();
            }).ExecuteLater(2);
        }

        /// <summary>
        /// 清空画布
        /// </summary>
        public void ClearGraph()
        {
            foreach (var edge in edges.ToList()) RemoveElement(edge);
            foreach (var node in nodes.ToList()) RemoveElement(node);
            _nodeViewMap.Clear();
        }

        /// <summary>
        /// 递归从数据创建节点视图（不建边）
        /// </summary>
        private void BuildNodeViewRecursive(BTNodeData data, BTNodeView parentView)
        {
            var view = CreateNodeView(data);
            AddElement(view);
            _nodeViewMap[data.guid] = view;

            foreach (var child in data.children)
                BuildNodeViewRecursive(child, view);
        }

        /// <summary>
        /// 根据节点的 children 数据建立 Output→Input 连线
        /// </summary>
        private void ConnectChildEdges(BTNodeView parentView)
        {
            if (parentView.OutputPort == null) return;
            foreach (var childData in parentView.Data.children)
            {
                if (_nodeViewMap.TryGetValue(childData.guid, out var childView)
                    && childView.InputPort != null)
                {
                    var edge = parentView.OutputPort.ConnectTo(childView.InputPort);
                    AddElement(edge);
                }
            }
        }

        /// <summary>
        /// 自动排列所有节点：从上到下（深度），优先级从左到右（children 顺序）
        /// </summary>
        public void AutoLayout()
        {
            if (Asset?.rootNode == null) return;

            const float vSpacing = 220f;   // 父子节点垂直间距（深度方向）
            const float hSpacing = 200f;   // 相邻叶子水平间距（兄弟方向）

            float totalWidth = LayoutSubtreeVertical(Asset.rootNode, 0, vSpacing, hSpacing);
            // totalWidth 可用来居中整棵树，这里先不处理

            EditorUtility.SetDirty(Asset);
            MarkDirtyRepaint();

            schedule.Execute(() => FrameAllPublic()).ExecuteLater(1);
        }

        /// <summary>
        /// 递归布局：返回该子树所需的总宽度
        /// </summary>
        private float LayoutSubtreeVertical(BTNodeData node, int depth,
            float vSpacing, float hSpacing)
        {
            if (node == null) return 0;

            float y = depth * vSpacing;     // 越深越靠下

            if (node.children.Count == 0)
            {
                // 叶子节点：X 暂设 0，由父节点统一移动
                node.graphPosition = new Vector2(0, y);
                UpdateNodeViewPosition(node);
                return hSpacing;
            }

            // 1. 先递归排列所有子节点（后序遍历）
            float totalWidth = 0;
            for (int i = 0; i < node.children.Count; i++)
            {
                var child = node.children[i];
                float childWidth = LayoutSubtreeVertical(child, depth + 1, vSpacing, hSpacing);

                // 把整个子树向右移到正确列
                ShiftSubtreeX(child, totalWidth - child.graphPosition.x);
                totalWidth += childWidth;
            }

            // 2. 父节点居中于首尾子节点之间
            float xCenter = (node.children[0].graphPosition.x
                           + node.children[^1].graphPosition.x) / 2f;
            node.graphPosition = new Vector2(xCenter, y);
            UpdateNodeViewPosition(node);

            return totalWidth;
        }

        /// <summary>
        /// 将节点及其所有子节点沿 X 轴平移
        /// </summary>
        private void ShiftSubtreeX(BTNodeData node, float xOffset)
        {
            if (node == null) return;
            node.graphPosition = new Vector2(node.graphPosition.x + xOffset, node.graphPosition.y);
            UpdateNodeViewPosition(node);
            foreach (var child in node.children)
                ShiftSubtreeX(child, xOffset);
        }

        /// <summary>
        /// 更新节点视图位置（如果视图已存在）
        /// </summary>
        private void UpdateNodeViewPosition(BTNodeData data)
        {
            if (_nodeViewMap.TryGetValue(data.guid, out var view))
                view.SetPosition(new Rect(data.graphPosition, _defaultNodeSize));
        }

        /// <summary>
        /// 创建一个新的节点数据
        /// </summary>
        public BTNodeData CreateNodeData(E_BTNodeType type)
        {
            return new BTNodeData
            {
                nodeType = type,
                nodeName = GetDefaultNodeName(type),
                graphPosition = Vector2.zero
            };
        }

        /// <summary>
        /// 创建节点视图（仅视图，不保存到 Asset）
        /// </summary>
        public BTNodeView CreateNodeView(BTNodeData data)
        {
            var nodeView = new BTNodeView(data, this);
            nodeView.SetPosition(new Rect(data.graphPosition, _defaultNodeSize));
            return nodeView;
        }

        /// <summary>
        /// 创建节点数据 + 视图 + 注册到 Asset
        /// </summary>
        public BTNodeView CreateAndAddNodeView(BTNodeData data, BTNodeData parentData = null)
        {
            // 数据层
            Asset.AddNode(data);
            if (parentData != null)
            {
                // 挂到指定父节点下
                parentData.children.Add(data);
                data.parent = parentData;
                data.parentGuid = parentData.guid;
            }
            else if (Asset.rootNode != null)
            {
                // 无指定父节点但已有根 → 自动挂到根下，防止断开节点丢失
                Asset.rootNode.children.Add(data);
                data.parent = Asset.rootNode;
                data.parentGuid = Asset.rootNode.guid;
            }
            else
            {
                // 第一个节点 → 设为根
                Asset.rootNode = data;
            }

            // 视图层
            var view = CreateNodeView(data);
            AddElement(view);
            _nodeViewMap[data.guid] = view;

            // 如果指定了父节点，自动连线
            if (parentData != null && _nodeViewMap.TryGetValue(parentData.guid, out var parentView)
                && parentView.OutputPort != null && view.InputPort != null)
            {
                var edge = parentView.OutputPort.ConnectTo(view.InputPort);
                AddElement(edge);
            }

            EditorUtility.SetDirty(Asset);
            return view;
        }

        /// <summary>
        /// 根据 guid 查找节点视图
        /// </summary>
        public BTNodeView GetNodeViewByGuid(string guid)
            => _nodeViewMap.TryGetValue(guid, out var view) ? view : null;

        // ==================== 端口连接兼容性 ====================

        /// <summary>
        /// 判断两个端口是否能连线：
        /// - 方向相反（Output ↔ Input）
        /// - 不是同一个节点
        /// - 同一 Input 只能接一条边（Single capacity）
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList()
                .Where(targetPort =>
                    targetPort.direction != startPort.direction
                    && targetPort.node != startPort.node
                )
                .ToList();
        }

        // ==================== 连边 / 断边 / 删节点的数据同步 ====================

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // ── 元素被删除 ──
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    switch (element)
                    {
                        case Edge edge:
                            HandleEdgeRemoved(edge);
                            break;
                        case BTNodeView nodeView:
                            HandleNodeRemoved(nodeView);
                            break;
                    }
                }
            }

            // ── 新建边 ──
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    HandleEdgeCreated(edge);
                }
            }

            // ── 节点被移动 ──
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is BTNodeView nodeView)
                    {
                        nodeView.Data.graphPosition = nodeView.GetPosition().position;
                    }
                }
            }

            if (Asset != null)
                EditorUtility.SetDirty(Asset);

            return change;
        }

        /// <summary>
        /// 边断开 → 从父节点 children 中移除子节点
        /// </summary>
        private void HandleEdgeRemoved(Edge edge)
        {
            var parentView = edge.output?.node as BTNodeView;
            var childView = edge.input?.node as BTNodeView;
            if (parentView == null || childView == null) return;

            parentView.Data.children.Remove(childView.Data);
            childView.Data.parent = null;
            childView.Data.parentGuid = null;
        }

        /// <summary>
        /// 新建边 → 子节点注册到父节点 children，同时解除旧父节点关系
        /// </summary>
        private void HandleEdgeCreated(Edge edge)
        {
            var parentView = edge.output?.node as BTNodeView;
            var childView = edge.input?.node as BTNodeView;
            if (parentView == null || childView == null) return;

            // 解除旧的父子关系（一个子节点只能有一个父）
            if (childView.Data.parent != null)
            {
                childView.Data.parent.children.Remove(childView.Data);
            }

            // 建立新关系
            parentView.Data.children.Add(childView.Data);
            childView.Data.parent = parentView.Data;
            childView.Data.parentGuid = parentView.Data.guid;
        }

        /// <summary>
        /// 删除节点 → 从父节点 children 移除 + 从 Asset 移除 + 断开所有子节点关系
        /// </summary>
        private void HandleNodeRemoved(BTNodeView nodeView)
        {
            var data = nodeView.Data;

            // 1. 从父节点 children 移除
            data.parent?.children.Remove(data);

            // 2. 递归解除所有子节点的 parent 引用（视图边已由 GraphView 自动删除）
            foreach (var child in data.children)
            {
                child.parent = null;
                child.parentGuid = null;
            }


            // 如果删的是根节点，清掉引用
            if (Asset.rootNode == data)
                Asset.rootNode = null;

            // 3. 从 Asset 数据容器移除
            Asset.RemoveNode(data);

            // 4. 从映射表移除
            _nodeViewMap.Remove(data.guid);
        }

        // ==================== 右键上下文菜单 ====================

        private void RegisterContextMenu()
        {
            // GraphView 的右键菜单
            var graphMenuManipulator = new ContextualMenuManipulator(BuildContextMenu);
            this.AddManipulator(graphMenuManipulator);
        }

        /// <summary>
        /// 构建画布空白区的右键菜单
        /// </summary>
        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            // 记录鼠标位置，供创建节点使用
            _lastContextMenuMousePos = viewTransform.matrix.inverse.MultiplyPoint(
                evt.localMousePosition
            );

            var menu = evt.menu;

            //TODO:新增自定义节点类型时，在此更新空白点击菜单
            // ── 创建节点 ──
            menu.AppendAction("创建 / 序列 (Sequence)", _ => CreateNodeViaMenu(E_BTNodeType.Sequence));
            menu.AppendAction("创建 / 选择器 (Selector)", _ => CreateNodeViaMenu(E_BTNodeType.Selector));
            menu.AppendAction("创建 / 并行 (Parallel)", _ => CreateNodeViaMenu(E_BTNodeType.Parallel));
            menu.AppendAction("创建 / 取反 (Inverter)", _ => CreateNodeViaMenu(E_BTNodeType.Inverter));
            menu.AppendAction("创建 / 延时 (Delay)", _ => CreateNodeViaMenu(E_BTNodeType.Delay));
            menu.AppendAction("创建 / 重复 (Repeat)", _ => CreateNodeViaMenu(E_BTNodeType.Repeat));
            menu.AppendAction("创建 / 事件节点 (Action)", _ => CreateNodeViaMenu(E_BTNodeType.Action));
            menu.AppendAction("创建 / 条件节点 (Condition)", _ => CreateNodeViaMenu(E_BTNodeType.Condition));
            menu.AppendAction("创建 / 打印节点 (Debug)", _ => CreateNodeViaMenu(E_BTNodeType.Debug));
            menu.AppendSeparator("创建/");

            // ── 画布操作 ──
            menu.AppendAction("选择全部节点", _ => { ClearSelection(); graphElements.ForEach(e => AddToSelection(e)); });
            menu.AppendSeparator();
            menu.AppendAction("适应画布 (Frame All)", _ => FrameAllPublic());
        }

        private void CreateNodeViaMenu(E_BTNodeType type)
        {
            var data = CreateNodeData(type);
            data.graphPosition = _lastContextMenuMousePos;
            CreateAndAddNodeView(data);
        }

        // ==================== 右键创建节点（搜索窗口） ====================

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            if (Asset == null) return;
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        // ==================== 实用方法 ====================

        /// <summary>
        /// 获取默认节点名
        /// </summary>
        private string GetDefaultNodeName(E_BTNodeType type) => type switch
        {
            //TODO:新增自定义节点类型时，在此更新默认名
            E_BTNodeType.Sequence => "Sequence",
            E_BTNodeType.Selector => "Selector",
            E_BTNodeType.ActiveSelector => "ActiveSelector",
            E_BTNodeType.Parallel => "Parallel",
            E_BTNodeType.Monitor => "Monitor",
            E_BTNodeType.Inverter => "Inverter",
            E_BTNodeType.Repeat => "Repeat",
            E_BTNodeType.Action => "Action",
            E_BTNodeType.Delay=>"Delay",
            E_BTNodeType.Condition=>"Condition",
            E_BTNodeType.Debug=>"Debug",
            _ => "Unknown"
        };


        /// <summary>
        /// 暴露 GraphView 的 CalculateFrameTransform（用于工具栏按钮）
        /// </summary>
        public void FrameAllPublic()
        {
            var nodeViews = nodes.ToList().Cast<BTNodeView>().ToList();
            if (nodeViews.Count == 0) return;

            Rect bounds = new Rect(nodeViews[0].GetPosition().position, Vector2.zero);
            foreach (var nv in nodeViews)
            {
                var pos = nv.GetPosition();
                bounds = Rect.MinMaxRect(
                    Mathf.Min(bounds.xMin, pos.xMin),
                    Mathf.Min(bounds.yMin, pos.yMin),
                    Mathf.Max(bounds.xMax, pos.xMax),
                    Mathf.Max(bounds.yMax, pos.yMax)
                );
            }

            // 留一些边距
            bounds = new Rect(bounds.x - 80, bounds.y - 80, bounds.width + 160, bounds.height + 160);
            CalculateFrameTransformPublic(bounds, layout.width, layout.height, 30);
        }

        private void CalculateFrameTransformPublic(Rect rect, float viewWidth, float viewHeight, int frameBorder)
        {
            var frameTranslation = Vector3.zero;
            var frameScaling = Vector3.one;

            // 此处直接调用基类 protected 方法不可行，改用计算后直接设置
            float scale = Mathf.Min(viewWidth / rect.width, viewHeight / rect.height, 1f);
            float tx = -rect.x * scale + (viewWidth - rect.width * scale) * 0.5f;
            float ty = -rect.y * scale + (viewHeight - rect.height * scale) * 0.5f;

            viewTransform.scale = Vector3.one * scale;
            viewTransform.position = new Vector3(tx, ty, 0);
        }

        // ==================== 节点视图的右键菜单扩展 ====================

        /// <summary>
        /// 供 BTNodeView 调用，构建单个节点的右键菜单
        /// </summary>
        public void BuildNodeContextMenu(ContextualMenuPopulateEvent evt, BTNodeView nodeView)
        {
            var menu = evt.menu;

            //TODO:新增自定义节点类型时，在此更新右键菜单
            // 仅对可连接子节点的节点显示
            if (nodeView.OutputPort != null)
            {
                menu.AppendAction("添加子节点 / 序列", _ => CreateChildNode(E_BTNodeType.Sequence, nodeView));
                menu.AppendAction("添加子节点 / 选择器", _ => CreateChildNode(E_BTNodeType.Selector, nodeView));
                menu.AppendAction("添加子节点 / 事件节点", _ => CreateChildNode(E_BTNodeType.Action, nodeView));
                menu.AppendAction("添加子节点 / 条件节点", _ => CreateChildNode(E_BTNodeType.Condition, nodeView));
                menu.AppendAction("添加子节点 / 取反", _ => CreateChildNode(E_BTNodeType.Inverter, nodeView));
                menu.AppendAction("添加子节点 / 重复", _ => CreateChildNode(E_BTNodeType.Repeat, nodeView));
                menu.AppendAction("添加子节点 / 延时", _ => CreateChildNode(E_BTNodeType.Delay, nodeView));
                menu.AppendAction("添加子节点 / 打印节点", _ => CreateChildNode(E_BTNodeType.Debug, nodeView));
                menu.AppendSeparator("添加子节点/");
            }

            menu.AppendAction("复制节点", _ => DuplicateNode(nodeView));
            menu.AppendAction("设为根节点", _ => SetAsRoot(nodeView), nodeView.Data.guid != Asset.rootNode?.guid
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled);
            menu.AppendSeparator();

            menu.AppendAction("删除节点", _ =>
            {
                // 先去掉与它关联的所有边
                var edgesToRemove = edges.ToList()
                    .Where(e => (e.output?.node == nodeView || e.input?.node == nodeView))
                    .ToList();
                foreach (var e in edgesToRemove) RemoveElement(e);
                RemoveElement(nodeView);
            });
        }

        private void CreateChildNode(E_BTNodeType type, BTNodeView parentView)
        {
            var data = CreateNodeData(type);
            // 放在父节点右下方
            data.graphPosition = parentView.Data.graphPosition + new Vector2(0, 150);
            CreateAndAddNodeView(data, parentView.Data);
        }

        private void DuplicateNode(BTNodeView sourceView)
        {
            var newData = sourceView.Data.CloneWithoutRunTime();
            newData.graphPosition += new Vector2(50, 50);
            // 注意：clone 出来的 children 是空的，不复制子树
            CreateAndAddNodeView(newData, sourceView.Data.parent);
        }

        private void SetAsRoot(BTNodeView nodeView)
        {
            var oldRootData = Asset.rootNode;
            var newRootData = nodeView.Data;

            // 1. 从旧父节点的 children 列表中移除新根节点
            if (newRootData.parent != null)
            {
                newRootData.parent.children.Remove(newRootData);

                // 删掉旧父节点 → 新根节点 之间的边
                var edgesToRemove = edges.ToList()
                    .Where(e => e.input?.node == nodeView)
                    .ToList();
                foreach (var e in edgesToRemove)
                    RemoveElement(e);

                newRootData.parent = null;
                newRootData.parentGuid = null;
            }

            // 2. 设为新根
            Asset.rootNode = newRootData;

            // 3. 刷新旧根节点的样式（去掉高亮 + 显示输入端口）
            if (oldRootData != null && _nodeViewMap.TryGetValue(oldRootData.guid, out var oldView))
            {
                oldView.UpdateInputPortVisibility();
            }

            // 4. 刷新新根节点的样式（加高亮 + 隐藏输入端口）
            nodeView.UpdateInputPortVisibility();

            EditorUtility.SetDirty(Asset);
        }
    }
}