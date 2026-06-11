# AtomUI.City.Localization Lookup and Fallback 设计

版本：v0.1
状态：正式初版
适用范围：资源查找、资源优先级、fallback culture、missing marker、格式化错误和撤销后 fallback。

## 1. 定位

Lookup and fallback 负责把 resource key 解析为当前 culture 下的显示资源。

查找必须可诊断、可预测、可测试。

## 2. 查找输入

查找输入包含：

- Resource key。
- Resource type。
- Current culture。
- Scope。
- ModuleId。
- PluginId。
- ContributionId。
- Format args。
- Fallback policy。

## 3. 查找顺序

```text
Current feature / plugin
-> owning module
-> application host
-> shared framework
-> fallback culture
-> invariant fallback
-> missing marker
```

同一层级内的优先级由 Contribution order 和 Host policy 决定。

## 4. Fallback Culture

Fallback chain 示例：

```text
zh-CN -> zh-Hans -> zh -> invariant
```

规则：

- fallback package 按需加载。
- fallback 命中必须记录诊断级别信息。
- critical resource fallback 失败可以导致 culture switch rollback。
- 非 critical resource fallback 失败返回 missing marker。

## 5. Missing Marker

开发模式默认：

```text
!Settings.Title!
```

发布模式默认：

- invariant fallback。
- key fallback。
- diagnostics record。

具体策略由 Host 配置。

## 6. 格式化

格式化资源必须使用当前 culture。

规则：

- 参数数量不匹配时返回 raw template 或 missing marker。
- 格式化异常记录 diagnostics。
- 日期、数字、货币使用 CurrentCulture。
- UI 文案资源使用 CurrentUICulture。

## 7. 资源撤销

插件资源撤销后：

```text
Revoke contribution
-> remove package store
-> invalidate lookup cache
-> fallback to owning module / host
-> notify Presentation refresh or clear UI
```

撤销后的资源不能继续被 Host cache 命中。

## 8. 测试策略

测试必须覆盖：

- 当前 scope 命中。
- module fallback。
- host fallback。
- culture fallback。
- invariant fallback。
- missing marker。
- 格式参数错误。
- 插件撤销后 fallback。
