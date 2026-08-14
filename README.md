# REFRamework 框架

## 行为树

基于 GraphView 实现的可视化行为树编辑器，支持拖拽连线、SO 资产持久化。

### 快速上手

1. Project 窗口右键 → **Create** → **BehaviorTree** → **TreeAsset**
2. 双击 `.asset` 打开编辑器，画布空白处右键创建节点
3. 从父节点 **Out** 端口拖线到子节点 **In** 端口建立连接
4. `Ctrl+S` 保存，关闭后重新打开自动恢复

### 运行时调用

```csharp
public class EnemyAI : MonoBehaviour
{
    public BehaviorTreeAsset treeAsset;
    private BehaviorTree _tree;

    private void Start()
    {
        _tree = treeAsset.GetTree();
    }

    private void Update()
    {
        _tree?.Tick();  // 每帧驱动
    }
}
```

### 自定义节点

**新增节点** 涉及 6 个文件（以 `DelayNode` 为例）：

1. 写节点类，继承 `DecoratorNode` / `CompositeNode` / `BehaviorNode`，实现 `OnUpdate()`
2. `BTNodeData.cs` → `E_BTNodeType` 枚举加一项
3. `BTNodeFactory.cs` → `CreateNode()` 的 switch 加构造 case
4. `BTNodeView.cs` → `BuildContent()` 加 UI 分支 + `ApplyStyle()` 加颜色
5. `BehaviorTreeGraphView.cs` → 右键菜单加创建条目
6. `BTNodeSearchWindow.cs` → 搜索窗口加条目

**删除节点** 即反向操作：从以上 6 个文件移除对应枚举值和注册代码。

> 代码中已有 `TODO: 新增节点时在此...` 标记，打开 VS 任务列表即可快速定位。

---

## 配置表

目前仅支持 CSV 格式，且 WPS 等软件打开文件时 Unity 无法读取。

---

## 常见问题

### 打开 REDebugger 后报资源丢失

1. 确保已导入 TextMeshPro 包
2. Project 窗口中找到框架内任意 `.uss` 文件，右键 → **Reimport**
