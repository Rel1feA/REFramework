using UnityEngine;

namespace RECode.REFramework
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        [SerializeField] private BehaviorTreeAsset treeAsset;
        [SerializeField] private bool tickOnUpdate = true;

        public BehaviorTree Tree { get; private set; }
        public BehaviorTreeAsset Asset => treeAsset;

        // 用于编辑器调试的回写映射
        public System.Action OnTickComplete;

        private void Start()
        {
            if (treeAsset != null)
                Tree = new BehaviorTree(BTNodeFactory.Build(treeAsset.rootNode));
        }

        private void Update()
        {
            if (tickOnUpdate && Tree != null)
            {
                Tree.Tick();
                OnTickComplete?.Invoke();
            }
        }

        /// <summary>重新从 Asset 构建行为树</summary>
        public void RebuildTree()
        {
            if (treeAsset?.rootNode != null)
            {
                Tree = new BehaviorTree(BTNodeFactory.Build(treeAsset.rootNode));
            }
        }
    }
}