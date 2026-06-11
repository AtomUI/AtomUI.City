# AtomUI.City.Presentation UI Runtime 设计

版本：v0.1
状态：正式初版
适用范围：UI runtime ready/stopping、PresentationScope、WindowScope 和 AtomUI/Avalonia runtime bridge

## 1. 定位

Presentation 负责把 AtomUI.City Host 连接到 AtomUI/Avalonia UI runtime。

Core 不依赖 Avalonia。Presentation 是 Core 和 Avalonia 之间的适配层。

## 2. 启动链路

```text
ApplicationHost
-> StartPresentation
-> Initialize AtomUI/Avalonia runtime
-> Register IUiDispatcher
-> Create PresentationScope
-> Open initial WindowScope
-> Routing navigates initial route
```

## 3. 职责

Presentation runtime 负责：

- 初始化 Avalonia application bridge。
- 接入 AtomUI 资源和主题。
- 注册 Core `IUiDispatcher`。
- 报告 UI runtime ready。
- 创建 PresentationScope。
- 创建 WindowScope。
- 在 UI runtime 停止时拒绝新的 UI 投递。
- 输出 UI runtime 诊断。

## 4. PresentationScope

PresentationScope 是 UI runtime 的生命周期边界。

规则：

- PresentationScope 由 Host 生命周期管理。
- WindowScope 必须是 PresentationScope 的子 Scope。
- UI runtime stopping 后，不能创建新的 WindowScope。
- PresentationScope 停止时释放所有窗口、Outlet、View、binding、Interaction handler 和 UI 订阅。

## 5. WindowScope

每个窗口有独立 WindowScope。

规则：

- 一个 WindowScope 下可以有一个或多个 NavigationScope。
- 窗口关闭先请求 Routing/Mvvm leave confirmation。
- 关闭确认通过 Mvvm Interaction 或 Leave Guard 处理。
- WindowScope 停止时释放窗口内所有 UI contribution。

Presentation 不定义业务窗口模型，只提供窗口生命周期桥接。

## 6. 错误策略

| 场景 | 默认处理 |
|---|---|
| UI runtime 启动失败 | Application fatal。 |
| UI runtime 未 ready | 按 Host 策略等待或返回明确错误。 |
| UI runtime stopping 后新投递 | 拒绝并记录诊断。 |
| WindowScope 释放失败 | 聚合错误，继续释放其他窗口。 |

## 7. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| runtime ready | Unit | ready 后 dispatcher 可用。 |
| runtime stopping | Unit | 停止后拒绝新 UI 投递。 |
| PresentationScope stop | Unit | 子 WindowScope 被释放。 |
| Window close intent | Unit | 转成 leave request 或 interaction。 |
| 启动失败 | Unit | 返回 fatal 诊断。 |
