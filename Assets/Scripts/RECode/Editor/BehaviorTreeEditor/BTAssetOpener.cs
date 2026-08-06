using RECode.REFramework;
using UnityEditor;
using UnityEditor.Callbacks;

namespace RECode.Editor.BTEditor
{
    /// <summary>
    /// 双击 BehaviorTreeAsset 时自动打开行为树编辑器窗口
    /// </summary>
    public class BTAssetOpener
    {
        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as BehaviorTreeAsset;
            if (asset == null) return false;

            BehaviorTreeEditorWindow.OpenAsset(asset);
            return true;
        }
    }
}