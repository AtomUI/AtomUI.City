# AtomUI.City.Localization Validation and Errors 设计

版本：v0.1
状态：正式初版
适用范围：Validation message、Data/Security error、MessageKey、MessageArgs、错误文本刷新和诊断。

## 1. 定位

Validation 和错误模块不应该返回固定显示文本。

它们应返回稳定 code、message key 和参数，由 Localization 在当前 culture 下渲染。

## 2. Message 模型

建议消息结构：

```text
ErrorCode
MessageKey
MessageArgs
Severity
Diagnostics
```

`MessageKey` 用于用户可见文本。`Diagnostics` 用于开发和日志，不直接展示给用户。

## 3. Validation

Validation message 必须支持 culture refresh。

规则：

- Validator 返回 MessageKey 和 MessageArgs。
- Presentation 负责展示本地化结果。
- Culture 切换后可见 validation message 刷新。
- 缺失 validation key 使用 missing marker。

## 4. Data / Security Error

DataError 和 Security authorization result 不直接包含最终显示文本。

示例：

```text
DataError.AuthorizationForbidden
MessageKey = "Errors.AuthorizationForbidden"
MessageArgs = [...]
```

文化切换后，错误提示可以重新渲染。

## 5. Interaction 错误

Dialog、Toast、Notification 应传 MessageKey。

已经显示的临时 UI 是否刷新，由 Presentation 策略决定：

- 长时间存在的 Dialog 应刷新。
- 短生命周期 Toast 可以不刷新，但后续新 Toast 使用新 culture。

## 6. 错误策略

| 场景 | 默认处理 |
|---|---|
| MessageKey 缺失 | missing marker + diagnostics。 |
| MessageArgs 格式错误 | raw template + diagnostics。 |
| culture switch 时错误 UI 已关闭 | 忽略刷新。 |
| plugin error key revoked | fallback 或 clear UI。 |

## 7. 测试策略

测试必须覆盖：

- validation message key。
- DataError message key。
- Security forbidden message。
- culture switch refresh。
- missing key marker。
- plugin key revoked。
