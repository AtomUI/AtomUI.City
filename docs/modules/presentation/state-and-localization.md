# AtomUI.City.Presentation State 与 Localization 集成设计

版本：v0.1
状态：正式初版
适用范围：State UI 更新、culture refresh、binding refresh、路由标题和错误文本刷新

## 1. State UI 更新

State Core 不直接依赖 UI。Presentation 负责 UI 线程安全更新。

规则：

- UI 订阅使用 `DispatchPolicy.UiThread`。
- State 到 UI 的订阅必须绑定 Scope。
- View detached 后停止 UI 更新。
- 插件 View 相关订阅随插件停用释放。
- UI 更新异常进入 Presentation diagnostics。

Presentation 不保存应用状态；状态仍归 State 模块管理。

## 2. Localization UI 刷新

Presentation 负责把 Localization 的文化变化反映到 UI。

职责：

- 注册本地化 binding adapter。
- 当前文化变化后刷新文本。
- 路由标题、命令文本、验证消息、错误消息刷新。
- 插件本地化资源撤销后更新或清理对应 UI。

Localization 负责资源查找和文化状态，Presentation 负责 UI 展示刷新。

## 3. UI 线程规则

Culture refresh、ResourceDictionary 更新和 binding refresh 必须发生在 UI Thread。

```text
Culture state changed
-> Localization resolves resources
-> Presentation dispatcher
-> refresh bindings/resources
-> visual tree updated
```

## 4. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| State UI 订阅 | Unit | 通过 fake UI dispatcher 更新。 |
| View detached | Unit | detached 后停止 UI 更新。 |
| culture refresh | Unit | 文本 binding 被刷新。 |
| route title refresh | Unit | 路由标题随 culture 更新。 |
| 插件资源撤销 | Unit | 插件文本资源撤销后 UI 清理或 fallback。 |
