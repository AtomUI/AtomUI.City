# AtomUI.City.State 线程与调度设计

版本：v0.1
状态：正式初版
适用范围：状态提交、通知调度、Core Threading 集成、多线程约束和 UI Dispatcher 边界

## 1. 定位

桌面软件是天然多线程环境。State 模块必须在内核线程模型下运行，不能让状态提交、订阅通知和 UI 更新互相污染。

线程模型见：[Core Threading 设计](../core/threading.md)。

## 2. 基本规则

State 必须满足：

- `SetValue` 和 `Update` 原子化。
- 状态提交和订阅通知分离。
- 不在状态锁内调用订阅者。
- 相同 state key 的变更通知保持顺序。
- 应用级共享状态绑定 ApplicationScope。
- 插件状态绑定插件生命周期或插件贡献 lease。
- 推荐状态值使用 immutable 或 replace-only 风格。

## 3. 提交流程

```text
SetValue / Update
-> acquire state mutation gate
-> compare value
-> commit value and version
-> create change record
-> release mutation gate
-> dispatch notifications
```

订阅者运行在锁外，避免死锁和重入污染。

## 4. 调度策略

State Core 不直接依赖 Avalonia Dispatcher。

调度策略：

| 策略 | 说明 |
|---|---|
| Immediate | 当前线程通知。 |
| Queued | 排队后统一通知。 |
| Dispatcher | 切到 UI dispatcher。 |
| Background | 后台调度。 |

Presentation 负责把 Dispatcher 接入 State。Core 只定义抽象。

## 5. UI 边界

State Core 不直接更新 UI。

```text
State change committed
-> DispatchPolicy.UiThread
-> Presentation dispatcher
-> ViewModel property change or binding refresh
-> AtomUI/Avalonia visual refresh
```

UI 订阅必须绑定 Scope。View detached 后，相关 UI 订阅应停止更新。

## 6. Late Result Suppression

OperationScope 取消后不应继续提交状态更新。

规则：

- Data 请求完成前如果 OperationScope 已取消，结果必须被忽略或记录为 late result。
- Command 取消后不提交成功状态。
- RouteScope 离开后，Resolver 的 late result 不应更新旧路由状态。

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| 原子提交 | Unit | 并发更新不破坏值和 Version。 |
| 锁外通知 | Unit | handler 重入不会死锁。 |
| 顺序通知 | Unit | 同一 key 通知顺序稳定。 |
| UI 调度 | Unit | fake dispatcher 收到 UI 订阅。 |
| Scope 停用 | Unit | 停用后不再投递 UI 更新。 |
| late result | Unit | Operation 取消后不提交状态。 |
