using RECode.REFramework;
using UnityEditor;
using UnityEngine;

namespace RECode.Editor.BTEditor
{
    /// <summary>
    /// 自定义 Inspector：阻止 Unity 默认 Inspector 用 ListView 渲染 allNodes，
    /// 从而消除 ListViewSerializedObjectBinding 的 SerializedObject 泄漏报错。
    /// 只显示基础信息和 "打开编辑器" 按钮。
    /// </summary>
    [CustomEditor(typeof(BehaviorTreeAsset))]
    public class BehaviorTreeAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var asset = (BehaviorTreeAsset)target;

            EditorGUILayout.LabelField("行为树资源", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // 描述
            EditorGUI.BeginChangeCheck();
            asset.description = EditorGUILayout.TextArea(asset.description, GUILayout.Height(60));
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(asset);

            EditorGUILayout.Space(8);

            // 节点统计
            EditorGUILayout.LabelField("节点总数", asset.allNodes?.Count.ToString() ?? "0");
            EditorGUILayout.LabelField("根节点", asset.rootNode != null
                ? $"{asset.rootNode.nodeType} - {asset.rootNode.nodeName}"
                : "无");

            EditorGUILayout.Space(12);

            // 打开编辑器按钮
            if (GUILayout.Button("打开行为树编辑器", GUILayout.Height(30)))
            {
                BehaviorTreeEditorWindow.OpenAsset(asset);
            }
        }
    }
}