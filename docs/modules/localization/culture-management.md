# AtomUI.City.Localization Culture Management 设计

版本：v0.1
状态：正式初版
适用范围：当前文化状态、用户偏好、系统文化、事务式文化切换、失败回滚和通知。

## 1. 定位

Culture management 负责决定当前应用使用哪个 culture，以及文化切换如何安全完成。

桌面应用必须支持运行时切换文化，不能要求重启应用。

## 2. Culture 来源

文化来源优先级：

```text
Explicit user selection
-> persisted user preference
-> application default
-> system UI culture
-> invariant culture
```

用户选择应保存到用户配置。系统文化变化是否自动跟随由 Host policy 决定。

## 3. Culture State

Culture state 应包含：

- Current culture。
- Current UI culture。
- Fallback culture chain。
- Revision。
- Source。
- Loaded package set。
- Diagnostics。

Revision 用于让 binding、cache 和 localizer 判断是否需要刷新。

## 4. 事务式切换

文化切换流程：

```text
SetCultureAsync
-> calculate active package set
-> load target culture packages
-> load required fallback packages
-> validate critical resources
-> prepare Presentation resource swap
-> commit culture state
-> apply AtomUI/Avalonia resources on UI Thread
-> notify subscribers
```

失败回滚：

```text
Load or apply failed
-> keep previous culture state
-> dispose partially loaded packages
-> restore previous Presentation resources
-> emit diagnostics
```

## 5. 并发策略

同一时间只能有一个 culture switch。

规则：

- 新切换请求可以排队或取消旧请求，策略由 Host 配置。
- 已进入 commit 阶段后不允许抢占。
- 文化切换取消不是 fatal error。
- 文化切换必须绑定 ApplicationScope。

## 6. 线程模型

资源加载可以在后台进行。

AtomUI/Avalonia resource swap 和 binding refresh 必须在 UI Thread。

Localization Core 不依赖 Avalonia；Presentation 提供 bridge。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| culture 不支持 | 拒绝切换，保留旧 culture。 |
| package 加载失败 | rollback。 |
| fallback 加载失败 | rollback 或 missing marker，按 criticality。 |
| UI apply 失败 | rollback Presentation resources。 |
| 并发切换冲突 | queue 或 cancel previous。 |

## 8. 测试策略

测试必须覆盖：

- 默认 culture 选择。
- 用户偏好覆盖系统 culture。
- 成功切换。
- package 加载失败 rollback。
- UI apply 失败 rollback。
- 并发切换。
- revision 递增。
