# AtomUI.City.Security Command Integration 设计

版本：v0.1
状态：正式初版
适用范围：Command 授权元数据、`CanExecute` 联动、权限变化刷新、Presentation 显示策略和测试支持。

## 1. 定位

Command integration 负责把权限检查接入 MVVM Command 的可执行状态。

Mvvm 提供 Command 状态和刷新入口。Security 负责授权判断。Presentation 负责展示按钮、菜单、快捷键等 UI 状态。

## 2. Command Auth Metadata

Command 可以声明：

- 需要登录。
- 需要 permission。
- 需要 policy。
- 未授权时 disabled。
- 未授权时 hidden。
- 未授权提示 key。

Command metadata 可以来自 attribute、builder API 或 Source Generator manifest。

## 3. CanExecute 数据流

```text
Authentication / permission state changed
-> Security command authorization source invalidates
-> Command CanExecute recompute
-> Presentation refreshes enabled / visible state
```

Command 可执行状态可以同时受以下因素影响：

- Security 权限。
- Routing 当前状态。
- ViewModel active 状态。
- Validation 状态。
- Operation 正在执行状态。

Security 只提供授权维度，不覆盖其他维度。

## 4. 用户动作

执行命令前必须再次检查授权，不能只依赖 UI disabled 状态。

```text
UI invokes command
-> Command checks active / validation / operation state
-> Security authorization check
-> Execute or return authorization failure
```

这可以避免权限变化后 UI 尚未刷新时执行旧权限命令。

## 5. Presentation 表达

Presentation 可以根据授权结果展示：

- Disabled。
- Hidden。
- Tooltip。
- 权限不足提示。
- 登录提示。

Presentation 不读取权限存储，不解释 Policy，只消费 Command 状态和 Security 结果。

## 6. CompositeCommand

组合命令需要过滤当前 active 上下文中的可执行子命令。

规则：

- 子命令权限变化会触发组合命令状态刷新。
- 当前无授权子命令时组合命令不可执行。
- 插件子命令撤销后必须从组合命令中移除。
- 权限失败应进入 Command diagnostics。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| 授权未通过 | `CanExecute = false`，执行时返回 authorization failure。 |
| Policy 抛异常 | `CanExecute = false`，记录诊断。 |
| 插件 command 权限撤销 | Command contribution disabled 或 removed。 |
| 登录态未知 | 默认不可执行，除非 command 标记匿名可执行。 |

## 8. 测试策略

测试必须覆盖：

- 权限变化刷新 `CanExecute`。
- 登录态变化刷新 `CanExecute`。
- 执行前二次授权。
- CompositeCommand 子命令授权变化。
- 插件 command 撤销。
- Presentation 不直接参与授权判断。
