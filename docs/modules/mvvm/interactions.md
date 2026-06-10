# AtomUI.City.Mvvm Interactions 设计

版本：v0.1
状态：初版草案
适用范围：ViewModel 到 Presentation 的交互请求、Interaction handler、结果模型、ActivationScope 绑定和测试支持。

## 1. 定位

Interaction 用来让 ViewModel 发起 UI 交互请求，同时避免 ViewModel 直接依赖窗口、Dialog、MessageBox 或 AtomUI 控件。

Interaction 是 ViewModel 和 Presentation 之间的受控桥接。

## 2. 非目标

Interaction 不负责：

- 具体 Dialog 控件实现。
- 窗口管理。
- 路由跳转。
- 通知系统的 UI 呈现。
- 文件系统权限实现。

这些由 Presentation、Routing 或应用层实现。

## 3. 模型

建议提供泛型 Interaction Request：

```text
ViewModel
-> Interaction<TRequest, TResult>
-> Presentation handler
-> Result back to ViewModel
```

Interaction 适合：

- 确认。
- 输入。
- 文件选择。
- 通知。
- 需要 UI 承接的用户交互。

## 4. Handler 生命周期

Interaction handler 必须绑定 ActivationScope。

```text
ActivationScope
-> Register interaction handler
-> Handle interaction requests
-> Dispose handler on deactivation
```

ViewModel 停用时，未完成 interaction 应取消或返回明确的 canceled result。

## 5. 结果模型

Interaction 结果必须区分：

| 结果 | 含义 |
|---|---|
| Completed | 交互完成并返回结果。 |
| Canceled | 用户或生命周期取消。 |
| Failed | handler 执行失败。 |
| NotHandled | 没有可用 handler。 |

Interaction handler 缺失不应该导致应用崩溃，但必须记录诊断。

## 6. 插件边界

插件 ViewModel 可以发起 Interaction。

插件 Interaction 必须满足：

- handler 绑定插件产生的 ActivationScope。
- 请求携带 PluginId、ModuleId、ContributionId。
- 插件停用时取消未完成请求。
- 插件不能直接持有 Host UI 对象。

## 7. Presentation 集成

Presentation 负责把 Interaction Request 映射到具体 UI。

Mvvm 只定义：

- request。
- result。
- handler contract。
- lifecycle binding。
- diagnostics。

Presentation 可以根据平台实现 Dialog、Toast、FilePicker、Window 等交互。

## 8. 错误策略

| 场景 | 默认处理 |
|---|---|
| handler 缺失 | 返回 NotHandled，记录诊断。 |
| handler 抛异常 | 返回 Failed，记录诊断。 |
| ActivationScope 停用 | 返回 Canceled。 |
| Plugin 停用 | 返回 Canceled。 |

## 9. AOT / Source Generator

Generator/Analyzer 可负责：

- 生成 interaction descriptor。
- 诊断 interaction id 重复。
- 诊断 request/result 类型不稳定。
- 诊断 handler 未绑定 ActivationScope。
- 生成 manifest，供 Presentation 和 Testing 使用。

## 10. 测试策略

Testing 包应支持：

- 捕获 Interaction request。
- 注入 fake handler。
- 断言 Completed/Canceled/Failed/NotHandled。
- 断言 ActivationScope 释放时取消未完成请求。
- 断言插件停用时取消请求。
- 断言诊断记录。
