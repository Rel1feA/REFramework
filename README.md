# REFramework 框架

一套轻量级 Unity 游戏开发框架，覆盖项目起步阶段最常用的基础能力：单例、事件中心、资源管理、对象池、场景加载、计时器、配置表（CSV）、有限状态机（FSM）、行为树、输入、音频、UI、背包等。

- 框架命名空间：`RECode.REFramework`
- 示例命名空间：`RECode.Controller2D`（2D 控制器示例）
- 编辑器命名空间：`RECode.Editor.BTEditor`（行为树编辑器）

> 本文档中的代码示例默认已在文件顶部 `using RECode.REFramework;`。

---

## 目录

- [环境与依赖](#环境与依赖)
- [目录结构](#目录结构)
- [快速开始](#快速开始)
- [单例系统](#单例系统)
- [事件中心](#事件中心)
- [MonoController](#monocontroller)
- [资源管理](#资源管理)
- [对象池](#对象池)
- [场景加载](#场景加载)
- [计时器](#计时器)
- [配置表 CSV](#配置表-csv)
- [有限状态机 FSM](#有限状态机-fsm)
- [Blackboard](#blackboard)
- [行为树](#行为树)
- [输入系统](#输入系统)
- [音频](#音频)
- [UI 框架](#ui-框架)
- [背包](#背包)
- [2D 控制器示例](#2d-控制器示例)
- [常见问题](#常见问题)

---

## 环境与依赖

| 项 | 版本 / 说明 |
| --- | --- |
| Unity | 2022.3.15f1c1（2022.3 LTS） |
| Input System | `com.unity.inputsystem` 1.7.0 |
| TextMeshPro | `com.unity.textmeshpro` 3.0.6 |
| UGUI | `com.unity.ugui` 1.0.0 |

> 输入模块使用新版 Input System（不是旧版 InputManager），使用前需在 Player Settings 中将 Active Input Handling 切到 Input System 或 Both。

---

## 目录结构

```
Assets/Scripts/RECode/
├── 2DController/            # 示例：2D 角色控制器（PlayerController）
├── Editor/                  # 编辑器工具（行为树编辑器、事件中心调试器）
└── REFramework/             # 框架核心
    ├── Audio/               # 音频管理
    ├── Behaviour Tree/      # 行为树运行时 + 可视化编辑器数据
    ├── Blackboard/          # 通用黑板
    ├── ConfigReader/        # CSV 配置表读取
    ├── Event/               # 事件中心
    ├── FSM/                 # 有限状态机
    ├── Input/               # Input System 封装
    ├── Inventory/           # 背包
    ├── Mono/                # MonoController（更新事件 / 协程入口）
    ├── ObjectPool/          # 对象池
    ├── Resources/           # Resources 资源加载封装
    ├── Scene/               # 场景加载
    ├── Singleton/           # 单例基类
    ├── Timer/               # 计时器
    └── UI/                  # UI 管理（UIManager / BasePanel）
```

---

## 快速开始

框架有两种单例基类，使用方式不同：

- **MonoSingleton<T>**：继承 `MonoBehaviour`，需要挂到场景里的某个 GameObject 上。首次 `Awake` 时 `DontDestroyOnLoad`，跨场景保留；重复创建会被销毁。
- **NormalSingleton<T>**：普通 C# 单例（`class, new()`），无需挂场景，首次访问 `Instance` 时自动创建。

```csharp
// NormalSingleton：直接使用
EventCenter.Instance.AddListener("OnReady", OnReady);
```

依赖 `MonoSingleton` 的模块（`MonoController`、`PoolManager`、`TimerManager`、`UIManager`、`InputManager`、`AudioManager`）需要在场景中挂载对应组件后才能使用；依赖 `NormalSingleton` 的模块（`EventCenter`、`ResourcesManager`、`LoadSceneManager`、`ConfigManager`、`InventoryManager`）可直接使用。

---

## 单例系统

### MonoSingleton<T>

```csharp
public class GameManager : MonoSingleton<GameManager>
{
    protected override void Awake()
    {
        base.Awake();
        // 初始化逻辑
    }
}

// 使用（前提：场景中已挂 GameManager）
GameManager.Instance;
```

### NormalSingleton<T>

```csharp
public class Settings : NormalSingleton<Settings> { }
// 使用
Settings.Instance;
```

---

## 事件中心

`EventCenter` 提供全局观察者模式，支持无参事件、带一个参数的事件、以及带返回值（`Func<T>`）的委托。

| 方法 | 说明 |
| --- | --- |
| `AddListener(string, UnityAction)` | 添加无参事件监听 |
| `AddListener<T>(string, UnityAction<T>)` | 添加带参事件监听 |
| `RemoveListener(string, UnityAction)` / `RemoveListener<T>(...)` | 移除监听 |
| `AddFuncListener<T>(string, Func<T>)` / `RemoveFuncListener<T>(...)` | 添加 / 移除委托 |
| `FuncTrigger<T>(string)` | 触发委托并返回 `T` |
| `EventTrigger(string)` / `EventTrigger<T>(string, T)` | 触发事件 |
| `Clear()` | 清空全部事件 |

```csharp
// 无参事件
EventCenter.Instance.AddListener("OnGameStart", OnGameStart);
EventCenter.Instance.EventTrigger("OnGameStart");
EventCenter.Instance.RemoveListener("OnGameStart", OnGameStart);

// 带参事件
EventCenter.Instance.AddListener<int>("OnScoreChanged", OnScoreChanged);
EventCenter.Instance.EventTrigger("OnScoreChanged", 100);

// 带返回值委托
EventCenter.Instance.AddFuncListener<float>("GetMoveSpeed", GetMoveSpeed);
float speed = EventCenter.Instance.FuncTrigger<float>("GetMoveSpeed");
```

> 同一事件名不能混用「无参 / 有参 / Func」三种类型，否则会报类型不匹配错误。事件名建议用常量统一管理。

---

## MonoController

常驻的 `MonoBehaviour`，给非 `MonoBehaviour` 类提供 Update 更新事件和协程入口。

```csharp
MonoController.Instance.AddUpdateListener(Tick);
MonoController.Instance.RemoveUpdateListener(Tick);

Coroutine c = MonoController.Instance.MonoStartCoroutine(MyCoroutine());
```

---

## 资源管理

`ResourcesManager` 封装 `Resources` 的同步 / 异步加载。**注意：加载 `GameObject` 时会自动 `Instantiate` 返回实例；加载其他资源返回资源本身。** 资源需放在 `Assets/Resources/` 下。

```csharp
// 同步加载（GameObject 自动实例化）
GameObject enemy = ResourcesManager.Instance.Load<GameObject>("Prefabs/Enemy");

// 异步加载
ResourcesManager.Instance.LoadAsync<GameObject>("Prefabs/Enemy", (go) => { /* ... */ });

// 卸载未使用资源
ResourcesManager.Instance.UnloadUnusedAssets();
```

---

## 对象池

`PoolManager` 基于 `Resources` 下的 `Prefabs/` 路径管理对象池。

```csharp
PoolManager.Instance.GetObj("Bullet", (bullet) =>
{
    bullet.transform.position = firePoint.position;
});

PoolManager.Instance.PushObj("Bullet", bullet);   // 回收
PoolManager.Instance.WarmPool("Bullet", 30);      // 预热
PoolManager.Instance.Clear();                     // 清空
```

> 约定：预制体放在 `Assets/Resources/Prefabs/` 下，`GetObj` / `WarmPool` 传的是相对该目录的名字（不带扩展名）。

---

## 场景加载

```csharp
LoadSceneManager.Instance.LoadActiveScene();        // 重载当前场景
LoadSceneManager.Instance.LoadNextScene();          // 下一个场景
LoadSceneManager.Instance.LoadPreviousScene();      // 上一个场景

// 异步加载（名字 / 索引）
LoadSceneManager.Instance.LoadSceneAsync("Battle",
    loading: (progress) => { /* 0~1 */ },
    completed: (op) => { /* 完成 */ });
```

---

## 计时器

```csharp
// 单次计时
TimerManager.Instance.AddTimer("Bomb", 3f, () => Explode());

// 循环计时（共执行 5 次）
TimerManager.Instance.AddTimer("Spawn", 1f, Spawn, true, 5);

// 暂停 / 恢复 / 重置 / 删除
TimerManager.Instance.PauseTimer("Bomb");
TimerManager.Instance.ResumeTimer("Bomb");
TimerManager.Instance.ResetTimer("Bomb");
TimerManager.Instance.RemoveTimer("Bomb");

// 追加 / 移除回调
TimerManager.Instance.AddActionToTimer("Bomb", () => PlaySound());
TimerManager.Instance.RemoveActionFromTimer("Bomb", PlaySound);

// 全部暂停 / 恢复 / 清除
TimerManager.Instance.PauseAllTimer();
TimerManager.Instance.ResumeAllTimer();
TimerManager.Instance.RemoveAllTimer();
```

---

## 配置表 CSV

`ConfigManager` 通过反射把 CSV 表映射为强类型数据类。CSV 放在 `Assets/StreamingAssets/Configs/` 下，第一行为表头（对应数据类的公共属性 / 字段名）。

```csharp
public class ItemConfig
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Price { get; set; }
}

List<ItemConfig> list = ConfigManager.Instance.LoadCsv<ItemConfig>("ItemConfig");
```

> - 空单元格会转换为该类型的默认值。
> - 数据按类型缓存，同一张表重复调用不会重复读文件。
> - 目前仅支持 CSV；**WPS 等软件打开文件时 Unity 可能无法读取**，请用 Excel 或纯文本编辑器保存。

---

## 有限状态机 FSM

```csharp
public class IdleState : State<string>
{
    public IdleState() : base("Idle") { }

    public override void EnterState() { /* 进入 */ }
    public override void ExitState() { /* 退出 */ }
    public override void FrameUpdate() { /* 每帧 */ }
    public override State<string> ChangeState() => 条件 ? new RunState() : null;
}

var sm = new StateMachine<string>();
sm.Init(new IdleState());

// 每帧调用
sm.CurrentState.FrameUpdate();
sm.CheckChangeState();
```

---

## Blackboard

```csharp
var bb = new Blackboard();
bb.SetValue("Health", 100);
int hp = bb.GetValue<int>("Health");       // 不存在或类型不符时返回 default
bb.TryGetValue<int>("Health", out int v);  // 安全读取
bb.HasKey("Health");
bb.RemoveKey("Health");
bb.Clear();
```

---

## 行为树

基于 GraphView 实现的可视化行为树编辑器，支持拖拽连线、SO 资产持久化。

### 快速上手（编辑器）

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

### 内置节点

| 类型 | 节点 | 说明 |
| --- | --- | --- |
| 组合节点 | Sequence | 顺序执行，任一子节点失败则失败 |
| 组合节点 | Selector | 选择执行，任一子节点成功则成功 |
| 组合节点 | ActiveSelector | 切换成功分支时中断旧分支 |
| 组合节点 | Parallel | 并行执行，按成功 / 失败策略判定 |
| 组合节点 | Monitor | 并行监视，条件 + 动作组合 |
| 装饰节点 | Inverter | 反转子节点结果 |
| 装饰节点 | Repeat | 重复执行 N 次 |
| 装饰节点 | Delay | 延迟后执行子节点 |
| 叶子节点 | Action | 从黑板取 `UnityAction` 并执行 |
| 叶子节点 | Condition | 读取黑板 bool 值作为条件 |
| 叶子节点 | Debug | 打印调试日志 |

### 代码流式构建（Builder）

```csharp
var tree = new BehaviorTreeBuilder()
    .Selector()
        .Sequence()
            .ConditionNode("CanAttack")
            .ActionNode("DoAttack")
        .Back()
        .ActionNode("Idle")
    .Back()
    .End();

// 使用前向黑板写入节点所需数据
tree.blackboard.SetValue("CanAttack", true);
tree.blackboard.SetValue("DoAttack", (UnityAction)Attack);
tree.blackboard.SetValue("Idle", (UnityAction)Idle);

// 每帧驱动
tree.Tick();
```

`BehaviorTreeBuilder` 方法：`Sequence()`、`Selector()`、`Parallel(success, failure)`、`Monitor(success, failure)`、`ActiveSelector()`、`Repeat(int)`、`Inverter()`、`DelayNode(float)`、`ActionNode(string)`、`ConditionNode(string)`、`DebugNode(string)`、`Back()`、`End()`、`TreeTick()`。

### 自定义节点

**新增节点** 涉及 6 个文件（以 `DelayNode` 为例）：

1. 写节点类，继承 `DecoratorNode` / `CompositeNode` / `BehaviorNode`，实现 `OnUpdate()`
2. `BTNodeData.cs` → `E_BTNodeType` 枚举加一项
3. `BTNodeFactory.cs` → `CreateNode()` 的 switch 加构造 case
4. `BTNodeView.cs` → `BuildContent()` 加 UI 分支 + `ApplyStyle()` 加颜色
5. `BehaviorTreeGraphView.cs` → 右键菜单加创建条目
6. `BTNodeSearchWindow.cs` → 搜索窗口加条目

**删除节点** 即反向操作：从以上 6 个文件移除对应枚举值和注册代码。

> 代码中已有 `TODO: 新增节点时在此...` 标记，打开 IDE 任务列表即可快速定位。

---

## 输入系统

基于新版 Input System 的封装，支持按 Action 名绑定回调、读取值、切换 Action Map、动态改键。

### 准备

1. 创建 `InputActionAsset`（含 `GamePlay`、`UI` 两个 Action Map，`GamePlay` 下有 `Move`、`Jump` 等 Action）。
2. 把该资产赋给场景中 `InputManager` 的 `inputActions` 字段。
3. 默认启用 `GamePlay` Map。

### 使用

```csharp
// 绑定回调
InputManager.Instance.BindAction("Jump", Jump, InteractionType.Performed);

// 读取值
Vector2 move = InputManager.Instance.GetAxis("Move");
bool down    = InputManager.Instance.GetKeyDown("Jump");
bool held    = InputManager.Instance.GetKey("Jump");
bool up      = InputManager.Instance.GetKeyUp("Jump");

// 切换 Action Map
InputManager.Instance.SwitchActionMap("UI");

// 动态改键
InputManager.Instance.StartRebind("Jump", 0, onComplete: (ok) => { /* 改键完成 */ });

// 获取绑定显示名（如设置界面显示按键）
string keyName = InputManager.Instance.GetBindingName("Jump");
```

---

## 音频

`AudioManager` 通过 Inspector 的 `sounds` 列表配置音频，区分 BGM / SE，支持 AudioMixer。

### 准备

1. 场景中挂 `AudioManager`，赋值 `mixer`。
2. 在 `sounds` 列表添加 `Sound`：赋值 `clip`、选择 `audioType`（BGM / SE）、设置 `volume` / `loop` / `playOnAwake`。

### 使用

```csharp
AudioManager.Instance.PlayAudio("BGM_Main");   // 播放 BGM（自动停掉上一个 BGM）
AudioManager.Instance.PlayAudio("SE_Click");   // 播放音效
AudioManager.Instance.StopAudio("SE_Click");
AudioManager.Instance.ChangeValue(0.5f, "MasterVolume"); // 需先在 AudioMixer 暴露参数
```

---

## UI 框架

`UIManager` 负责面板的加载、显示、隐藏与层级管理，`BasePanel` 提供子控件自动查找。

### 准备

需在 `Assets/Resources/UI/` 下准备以下预制体（内部层级为 `Bot / Mid / Top / System`）：

- `StaticCanvas`（静态 Canvas）
- `DynamicCanvas`（动态 Canvas）
- `EventSystem`

面板预制体放在 `Assets/Resources/UI/Panels/` 下，并挂一个继承自 `BasePanel` 的脚本。

### 使用

```csharp
// 显示面板
UIManager.Instance.ShowPanel<MainPanel>("MainPanel", E_UI_Canvas.Static, E_UI_Layer.Mid, (panel) =>
{
    // 面板创建完成后的回调
});

UIManager.Instance.HidePanel("MainPanel");           // 隐藏面板
UIManager.Instance.HideFinallyShowPanel();           // 按打开顺序关闭最后一个
MainPanel panel = UIManager.Instance.GetPanel<MainPanel>("MainPanel"); // 获取已显示面板
```

### BasePanel

`BasePanel` 会自动收集子节点上的 `Button` / `Image` / `Text` / `Slider`，通过控件名快速获取：

```csharp
public class MainPanel : BasePanel
{
    public override void ShowMe()
    {
        Button startBtn = GetControl<Button>("StartButton");
        startBtn.onClick.AddListener(OnStartClick);
    }

    private void OnStartClick() { /* ... */ }
}
```

也可以给控件绑定自定义事件（拖拽、长按等）：

```csharp
UIManager.AddCustomEventListener(control, EventTriggerType.PointerDown, OnPointerDown);
```

---

## 背包

`InventoryManager` 是一个基础的背包实现，支持物品增删、堆叠、容量控制。

```csharp
int added   = InventoryManager.Instance.AddItem(1001, 5);   // 返回实际添加数量
int removed = InventoryManager.Instance.RemoveItem(1001, 2); // 返回实际移除数量

InventoryManager.Instance.DebugInventory(); // 打印背包内容（调试用）
```

> 注意：`InitItemInfo()` / `UseItem()` / `UpdateInventoryUI()` 目前是空实现，需根据项目自定义数据来源（ScriptableObject 或配置表）和 UI 刷新逻辑。

---

## 2D 控制器示例

`RECode.Controller2D` 命名空间下提供了一个 2D 平台控制器示例，演示框架输入模块的用法。挂在带 `Rigidbody2D + Collider2D` 的物体上，配置 `speed` / `jumpForce` / `groundLayer`。

它会：
- 通过 `InputManager.GetAxis("Move")` 读取移动输入
- 通过 `BindAction("Jump", Jump, InteractionType.Performed)` 绑定跳跃
- 支持下落时加重力、地面检测（OverlapBox + Gizmos 可视化）

---

## 常见问题

### 打开 REDebugger 后报资源丢失

1. 确保已导入 TextMeshPro 包
2. Project 窗口中找到框架内任意 `.uss` 文件，右键 → **Reimport**

### 单例 `Instance` 为 null

- `MonoSingleton` 需要先在场景中挂载对应脚本并 `Awake` 过一次，否则 `Instance` 为空。
- `NormalSingleton` 首次访问 `Instance` 会自动创建，无需挂场景。

### 资源加载不到

- `ResourcesManager` / `PoolManager` 依赖 `Assets/Resources/` 目录。
- 对象池预制体需放在 `Assets/Resources/Prefabs/` 下。
- 配置表 CSV 需放在 `Assets/StreamingAssets/Configs/` 下。

### 输入没有响应

- 确认已安装 Input System，并在 Player Settings 中把 Active Input Handling 设为 Input System 或 Both。
- 确认 `InputManager` 的 `inputActions` 已赋值，且对应 Action Map / Action 名称正确。

### UI 面板无法显示

- 确认 `Assets/Resources/UI/` 下存在 `StaticCanvas`、`DynamicCanvas`、`EventSystem` 预制体，且内部层级为 `Bot / Mid / Top / System`。
