using RECode.REFramework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RECode.Editor.BTEditor
{
    public class BehaviorTreeEditorWindow : EditorWindow
    {
        private BehaviorTreeGraphView _graphView;
        private BehaviorTreeAsset _currentAsset;
        private bool _guiBuilt;

        [MenuItem("Window/REEditor/BehaviorTree Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<BehaviorTreeEditorWindow>("行为树编辑器");
            window.minSize = new Vector2(800, 500);
            window.Show();
        }

        public static void OpenAsset(BehaviorTreeAsset asset)
        {
            var window = GetWindow<BehaviorTreeEditorWindow>("行为树编辑器");
            window.LoadAsset(asset);
            window.Show();
        }

        private void CreateGUI()
        {
            BuildGUI();
            _guiBuilt = true;
        }

        private void BuildGUI()
        {
            rootVisualElement.Unbind();
            rootVisualElement.Clear();

            // 根容器改为竖向弹性布局
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            // ── 工具栏（固定高度） ──
            var toolbar = new Toolbar();
            toolbar.style.height = 24;
            toolbar.style.flexShrink = 0;

            var saveBtn = new Button(() =>
            {
                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(_currentAsset);
            })
            { text = "保存" };
            toolbar.Add(saveBtn);

            var reloadBtn = new Button(() =>
            {
                if (_currentAsset != null)
                    _graphView?.LoadAsset(_currentAsset);
            })
            { text = "刷新" };
            toolbar.Add(reloadBtn);

            var frameBtn = new Button(() => _graphView?.FrameAllPublic())
            { text = "适应画布" };
            toolbar.Add(frameBtn);

            var autoLayoutBtn = new Button(() => _graphView?.AutoLayout())
            { text = "自动排列" };
            toolbar.Add(autoLayoutBtn);

            rootVisualElement.Add(toolbar);

            // ── GraphView 画布（占剩余空间） ──
            _graphView = new BehaviorTreeGraphView();
            _graphView.style.flexGrow = 1;   // 替代 StretchToParentSize
            rootVisualElement.Add(_graphView);

            if (_currentAsset != null)
                _graphView.Initialize(this, _currentAsset);
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            rootVisualElement?.Unbind();
            rootVisualElement?.Clear();
            _guiBuilt = false;
        }

        private void OnEnable()
        {
            // 窗口从隐藏恢复时，重建 GUI
            if (!_guiBuilt)
                BuildGUI();
            EditorApplication.update += OnEditorUpdate;
        }

        /// <summary>
        /// 运行时轮询：通过反射读取节点状态，刷新 GraphView 显示
        /// </summary>
        private void OnEditorUpdate()
        {
            if (!Application.isPlaying || _graphView?.Asset == null) return;
            if (BTNodeFactory.LastBuildMap == null) return;

            // 反射获取 BehaviorNode.state 字段
            var stateField = typeof(BehaviorNode).GetField("state",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (stateField == null) return;

            foreach (var kv in BTNodeFactory.LastBuildMap)
            {
                var nodeView = _graphView.GetNodeViewByGuid(kv.Key);
                if (nodeView != null)
                    nodeView.UpdateState((E_BehaviorState)stateField.GetValue(kv.Value));
            }
        }

        public void LoadAsset(BehaviorTreeAsset asset)
        {
            _currentAsset = asset;

            // 兜底：如果 GraphView 还没有创建（比如时序问题），先建 GUI
            if (_graphView == null)
                BuildGUI();

            _graphView?.Initialize(this, asset);
            titleContent = new GUIContent($"行为树 - {asset.name}");
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is BehaviorTreeAsset asset)
                LoadAsset(asset);
        }
    }
}