using RECode.REFramework;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace RECode.Editor.BTEditor
{
    /// <summary>
    /// 行为树单个节点的图形视图
    /// - 输入/输出端口用于连线
    /// - 状态指示器（运行时着色）
    /// - 右键上下文菜单委托给 GraphView
    /// </summary>
    public class BTNodeView : Node
    {
        public BTNodeData Data { get; private set; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }

        private BehaviorTreeGraphView _graphView;
        private Label _stateLabel;
        private TextField _nameField;
        private Label _priorityLabel;

        // ── 节点颜色 ──
        private static readonly Color ColorComposite = new(0.25f, 0.45f, 0.75f);
        private static readonly Color ColorDecorator = new(0.85f, 0.55f, 0.15f);
        private static readonly Color ColorLeaf = new(0.25f, 0.70f, 0.35f);

        public BTNodeView(BTNodeData data, BehaviorTreeGraphView graphView)
        {
            Data = data;
            _graphView = graphView;

            // ŸŸ 基础属性 ŸŸ
            title = data.nodeName;

            // ŸŸ 端口 ŸŸ
            BuildPorts();

            // ŸŸ 标题栏：名称编辑 + 状态点 ŸŸ
            BuildTitleBar();

            // ŸŸ 节点内容区 ŸŸ
            BuildContent();

            // ŸŸ 样式 ŸŸ
            ApplyStyle();

            // ŸŸ 右键菜单（委托给 GraphView） ŸŸ
            RegisterNodeContextMenu();


            // ŸŸ 初始为无效状态 ŸŸ
            UpdateState(E_BehaviorState.Invalid);
        }

        // ==================== 构建 UI ====================

        private void BuildPorts()
        {
            // ŸŸ 输入端口 ŸŸ 所有节点都有（便于被其他节点连接），默认隐藏（根节点不显示）
            InputPort = InstantiatePort(Orientation.Vertical, Direction.Input,
                Port.Capacity.Single, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            // ŸŸ 输出端口 ŸŸ 非叶子节点有
            if (!IsLeafNode())
            {
                OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output,
                    Port.Capacity.Multi, typeof(bool));
                OutputPort.portName = "Out";
                outputContainer.Add(OutputPort);
            }
        }

        private void BuildTitleBar()
        {
            // ŸŸ 可编辑的节点名称 ŸŸ
            _nameField = new TextField
            {
                value = Data.nodeName,
                isDelayed = true,
                style = { flexGrow = 1, minWidth = 80 }
            };
            _nameField.RegisterValueChangedCallback(evt =>
            {
                Data.nodeName = evt.newValue;
                title = evt.newValue;
                EditorUtility.SetDirty(_graphView.Asset);
            });

            // ŸŸ 状态指示灯 ŸŸ
            _stateLabel = new Label("●")
            {
                style =
                {
                    fontSize = 16,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginLeft = 8,
                    marginRight = 4
                }
            };

            // ŸŸ 替换默认 title 布局 ŸŸ
            titleContainer.Clear();
            titleContainer.Add(_stateLabel);
            titleContainer.Add(_nameField);

            // ŸŸ 根节点特殊样式（后续通过 class 设置） ŸŸ

            // ── 优先级数字（右上角，UE 风格） ──
            _priorityLabel = new Label
            {
                style =
                {
                    fontSize = 11,
                    color = new Color(0.9f, 0.9f, 0.9f),
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginLeft = 6,
                    marginRight = 4,
                    backgroundColor = new Color(0f, 0f, 0f, 0.35f),
                    paddingLeft = 4,
                    paddingRight = 4,
                    borderTopLeftRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomRightRadius = 4,
                }
            };
            titleContainer.Add(_priorityLabel);
        }

        private void BuildContent()
        {
            // ŸŸ 类型标签 ŸŸ
            var typeLabel = new Label(Data.nodeType.ToString())
            {
                style =
                {
                    fontSize = 10,
                    color = new Color(0.6f, 0.6f, 0.6f),
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginTop = 4,
                    marginBottom = 4,
                    marginLeft = 8
                }
            };
            mainContainer.Add(typeLabel);

            //TODO:新增自定义节点类型时，若有特定参数再次加判断
            switch (Data.nodeType)
            {
                case E_BTNodeType.Parallel:
                case E_BTNodeType.Monitor:
                    BuildParallelParams();
                    break;
                case E_BTNodeType.Repeat:
                    BuildRepeatParams();
                    break;
                case E_BTNodeType.Delay:
                    BuildDelayParams();
                    break;
                case E_BTNodeType.Action:
                case E_BTNodeType.Condition:
                case E_BTNodeType.Debug:
                    BuildActionParams();
                    break;
            }
        }

        //TODO:新增自定义节点类型时，如有需要自定义节点样式

        /// <summary>
        /// 并行/监控节点：成功策略 + 失败策略下拉
        /// </summary>
        private void BuildParallelParams()
        {
            mainContainer.Add(new Label("并行策略")
            {
                style = { fontSize = 10, color = Color.gray, marginLeft = 8, marginTop = 4 }
            });

            var successDrop = new EnumField("成功条件", Data.successPolicy);
            successDrop.RegisterValueChangedCallback(evt =>
            {
                Data.successPolicy = (ParallelNode.E_Policy)evt.newValue;
                EditorUtility.SetDirty(_graphView.Asset);
            });
            successDrop.style.marginLeft = 8;
            mainContainer.Add(successDrop);

            var failureDrop = new EnumField("失败条件", Data.failurePolicy);
            failureDrop.RegisterValueChangedCallback(evt =>
            {
                Data.failurePolicy = (ParallelNode.E_Policy)evt.newValue;
                EditorUtility.SetDirty(_graphView.Asset);
            });
            failureDrop.style.marginLeft = 8;
            mainContainer.Add(failureDrop);
        }

        /// <summary>
        /// 重复节点：重复次数
        /// </summary>
        private void BuildRepeatParams()
        {
            mainContainer.Add(new Label("重复次数")
            {
                style = { fontSize = 10, color = Color.gray, marginLeft = 8, marginTop = 4 }
            });

            var repeatField = new IntegerField { value = Data.paramInt };
            repeatField.RegisterValueChangedCallback(evt =>
            {
                Data.paramInt = Mathf.Max(1, evt.newValue);
                EditorUtility.SetDirty(_graphView.Asset);
            });
            repeatField.style.marginLeft = 8;
            mainContainer.Add(repeatField);
        }

        /// <summary>
        /// Action 节点参数：事件名称
        /// </summary>
        private void BuildActionParams()
        {
            var eventField = new TextField("String变量")
            {
                value = Data.actionParamJson ?? "",
                style = { marginLeft = 8, marginRight = 8, marginTop = 4, marginBottom = 4 }
            };
            eventField.RegisterValueChangedCallback(evt =>
            {
                Data.actionParamJson = evt.newValue;
                EditorUtility.SetDirty(_graphView.Asset);
            });
            mainContainer.Add(eventField);
        }

        private void BuildDelayParams()
        {
            var delayField = new FloatField("延时(秒)")
            {
                value=Data.paramFloat,
                style = { marginLeft = 8, marginRight = 8, marginTop = 4 }
            };
            delayField.RegisterValueChangedCallback(evt =>
            {
                Data.paramFloat =Mathf.Max(0,evt.newValue);
                EditorUtility.SetDirty(_graphView.Asset);
            });
            mainContainer.Add(delayField);
        }

        // ==================== 样式 ====================

        private void ApplyStyle()
        {
            Color color = Data.nodeType switch
            {
                //TODO:新增自定义节点类型时，选择节点颜色
                E_BTNodeType.Action  or E_BTNodeType.Monitor or E_BTNodeType.Condition or E_BTNodeType.Debug
                    => ColorLeaf,
                E_BTNodeType.Inverter or E_BTNodeType.Repeat or E_BTNodeType.Delay
                    => ColorDecorator,
                _ => ColorComposite
            };

            titleContainer.style.backgroundColor = color;
            titleContainer.style.color = Color.white;
            titleContainer.style.paddingLeft = 6;
            titleContainer.style.paddingRight = 6;
            titleContainer.style.paddingBottom = 2;
            titleContainer.style.paddingTop = 2;
            titleContainer.style.borderTopLeftRadius = 6;
            titleContainer.style.borderTopRightRadius = 6;

            // 圆角
            mainContainer.style.borderBottomLeftRadius = 6;
            mainContainer.style.borderBottomRightRadius = 6;
            mainContainer.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f);

            // 最小尺寸
            style.minWidth = 140;

            inputContainer.style.flexDirection = FlexDirection.Column;
            outputContainer.style.flexDirection = FlexDirection.Column;

            // 输入端口是否可见：根节点隐藏
            UpdateInputPortVisibility();
        }

        /// <summary>
        /// 根据是否是根节点决定是否显示输入端口
        /// </summary>
        public void UpdateInputPortVisibility()
        {
            bool isRoot = _graphView.Asset?.rootNode?.guid == Data.guid;

            if (InputPort != null)
            {
                InputPort.style.display = isRoot ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (isRoot)
            {
                // 根节点醒目标识
                AddToClassList("root-node");

                // 金色边框
                style.borderLeftWidth = 3;
                style.borderRightWidth = 3;
                style.borderTopWidth = 3;
                style.borderBottomWidth = 3;
                style.borderLeftColor = new Color(1f, 0.75f, 0.1f);   // 金色
                style.borderRightColor = new Color(1f, 0.75f, 0.1f);
                style.borderTopColor = new Color(1f, 0.75f, 0.1f);
                style.borderBottomColor = new Color(1f, 0.75f, 0.1f);

                // 标题栏加一个 ⭐ 图标
                if (!titleContainer.Children().Any(c => c is Label l && l.text == "Root"))
                {
                    titleContainer.Insert(0, new Label("Root")
                    {
                        style =
                {
                    fontSize = 14,
                    marginRight = 4,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
                    });
                }
            }
            else
            {
                RemoveFromClassList("root-node");

                // 普通节点恢复默认边框
                style.borderLeftWidth = 0;
                style.borderRightWidth = 0;
                style.borderTopWidth = 0;
                style.borderBottomWidth = 0;

                // 移除 ⭐（如果有的话）
                var star = titleContainer.Children().FirstOrDefault(c => c is Label l && l.text == "Root");
                star?.RemoveFromHierarchy();
            }
        }

        // ==================== 右键菜单 ====================

        private void RegisterNodeContextMenu()
        {
            var menuManipulator = new ContextualMenuManipulator(evt =>
            {
                _graphView.BuildNodeContextMenu(evt, this);
            });
            this.AddManipulator(menuManipulator);
        }

        // ==================== 运行时状态 ====================

        /// <summary>
        /// 更新节点运行时的状态显示
        /// </summary>
        public void UpdateState(E_BehaviorState state)
        {
            var (symbol, color) = state switch
            {
                E_BehaviorState.Running => (">", new Color(1f, 0.85f, 0.1f)),   // 黄色三角
                E_BehaviorState.Success => ("O", new Color(0.1f, 0.85f, 0.3f)), // 亮绿圆
                E_BehaviorState.Failure => ("X", new Color(0.9f, 0.2f, 0.15f)), // 亮红叉
                E_BehaviorState.Aborted => ("!", new Color(0.95f, 0.5f, 0.15f)),// 橙色叹号
                _ => ("-", new Color(0.5f, 0.5f, 0.5f))   // 灰色横线
            };

            _stateLabel.text = symbol;
            _stateLabel.style.color = color;

            // 运行时标题栏背景变色
            titleContainer.style.backgroundColor = state switch
            {
                E_BehaviorState.Running => new Color(0.35f, 0.30f, 0.05f),
                E_BehaviorState.Success => new Color(0.05f, 0.30f, 0.10f),
                E_BehaviorState.Failure => new Color(0.35f, 0.08f, 0.05f),
                E_BehaviorState.Aborted => new Color(0.35f, 0.18f, 0.05f),
                _ => GetOriginalColor()
            };
        }

        /// <summary>刷新右上角优先级数字（全局层序遍历序号）</summary>
        public void RefreshPriority(Dictionary<string, int> order)
        {
            if (_priorityLabel == null) return;

            if (order.TryGetValue(Data.guid, out int idx))
            {
                _priorityLabel.style.display = DisplayStyle.Flex;
                _priorityLabel.text = idx.ToString();
            }
            else
            {
                _priorityLabel.style.display = DisplayStyle.None;
            }
        }

        private Color GetOriginalColor() => Data.nodeType switch
        {
            //TODO:新增自定义节点类型时，在此更新一下原始颜色
            E_BTNodeType.Action or E_BTNodeType.Monitor or E_BTNodeType.Condition or E_BTNodeType.Debug=> ColorLeaf,
            E_BTNodeType.Inverter or E_BTNodeType.Repeat or E_BTNodeType.Delay => ColorDecorator,
            _ => ColorComposite
        };

        // ==================== 辅助 ====================

        private bool IsLeafNode()
        {
            return Data.nodeType is E_BTNodeType.Action or E_BTNodeType.Condition;
        }
    }
}