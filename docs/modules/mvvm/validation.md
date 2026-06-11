# AtomUI.City.Mvvm Validation 设计

版本：v0.1
状态：正式初版
适用范围：ViewModel 验证、验证状态、Command 联动、Presentation 集成、错误策略和测试支持。

## 1. 定位

Validation 负责 ViewModel 层的输入验证和状态暴露。

验证失败不是异常。验证失败应作为状态暴露给 UI、Command、Diagnostics 和 Testing。

## 2. 底层依赖

第一版默认复用 `CommunityToolkit.Mvvm`：

```text
ObservableValidator
```

Mvvm 在其上补充：

- ValidationScope。
- 同步验证和异步验证结果归一。
- Command 与验证状态联动。
- Presentation 可观察验证结果。
- Diagnostics 记录验证失败来源。

## 3. ValidationScope

ValidationScope 表示一组验证状态的生命周期边界。

ValidationScope 可以绑定：

- ViewModel。
- ActivationScope。
- RouteScope。
- OperationScope。

ViewModel 停用时，临时验证状态应随 ActivationScope 释放。

## 4. 验证结果模型

验证结果应区分：

| 结果 | 含义 |
|---|---|
| Valid | 验证通过。 |
| Invalid | 验证失败。 |
| Pending | 异步验证中。 |
| Canceled | 验证被取消。 |
| Failed | 验证逻辑异常。 |

验证逻辑异常和验证失败必须区分。

## 5. Command 联动

Command 可执行状态可以依赖 Validation 状态。

规则：

- Invalid 时 command 默认不可执行。
- Pending 时 command 是否可执行由 command policy 决定。
- Validation Failed 进入 ErrorPolicy，但不杀死 ViewModel。
- Command 执行前可以触发一次验证。

## 6. Presentation 集成

Presentation 负责把验证状态展示为 UI。

Mvvm 只提供：

- 属性级错误。
- 对象级错误。
- 验证状态变化通知。
- 验证诊断。

UI 展示形式由 Presentation 和应用决定。

## 7. 插件边界

插件 ViewModel 的验证状态绑定插件 ActivationScope。

插件停用时：

- 取消 pending validation。
- 释放 ValidationScope。
- 清理 validation subscriptions。

插件验证失败不能影响 Host 全局验证状态。

## 8. AOT / Source Generator

Generator/Analyzer 可负责：

- 诊断 validation attribute 使用不兼容 AOT。
- 生成 validation descriptor。
- 生成属性验证 manifest。
- 诊断异步验证未接入 cancellation token。

## 9. 错误策略

| 场景 | 默认处理 |
|---|---|
| 验证失败 | 暴露 Invalid，不抛异常。 |
| 验证取消 | 暴露 Canceled。 |
| 验证逻辑异常 | 暴露 Failed，记录诊断。 |
| ViewModel 停用 | 取消 pending validation。 |

## 10. 测试策略

Testing 包应支持：

- 触发属性验证。
- 触发对象验证。
- 断言 Valid/Invalid/Pending/Canceled/Failed。
- 断言 Command 与验证状态联动。
- 断言 ActivationScope 释放时取消验证。
- 断言诊断记录。
