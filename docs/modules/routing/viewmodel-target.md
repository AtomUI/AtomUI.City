# AtomUI.City.Routing ViewModel Target 设计

版本：v0.1
状态：正式初版
适用范围：Route 到 ViewModel Target 的映射、ViewModel 创建、参数和解析数据注入、Mvvm Activation、Presentation 边界。

## 1. 定位

Routing 负责把 Route 解析为 ViewModel Target。

它不负责 ViewLocator，不负责 View 创建，不操作 AtomUI/Avalonia 控件。

边界：

```text
Routing: Route -> ViewModel Target
Mvvm: ViewModel activation/deactivation
Presentation: ViewModel -> View
```

## 2. ViewModelTargetDescriptor

`ViewModelTargetDescriptor` 是 Source Generator 输出的运行时描述。

建议字段：

| 字段 | 职责 |
|---|---|
| `ViewModelType` | ViewModel 类型。 |
| `Factory` | 强类型工厂描述。 |
| `ServiceContext` | 服务来源。 |
| `ParameterBindings` | 路由参数绑定。 |
| `ResolvedDataBindings` | 解析数据绑定。 |
| `ActivationPolicy` | 激活策略。 |
| `Contribution` | 来源贡献。 |

运行时不通过命名约定推断 ViewModel。

## 3. 创建规则

ViewModel 创建发生在导航准备阶段。

```text
Resolve data
-> Create ViewModel
-> Initialize route context
-> Prepare activation
-> Commit
-> Activate ViewModel
```

创建失败时，候选 RouteScope 释放，当前页面保持不变。

## 4. 服务来源

ViewModel 从 RouteDescriptor 对应的 ServiceContext 创建。

| 来源 | 服务上下文 |
|---|---|
| 静态应用模块 | Application service context。 |
| 插件路由 | Plugin service context。 |
| RouteScope 服务 | Route service scope。 |
| ActivationScope 服务 | Activation service scope。 |

插件 ViewModel 不能从 Host Root ServiceProvider 任意解析未暴露服务。

## 5. 参数注入

路由参数通过强类型参数对象进入 ViewModel。

推荐方式：

- 构造函数接收参数对象。
- 初始化接口接收参数对象。
- RouteContext 暴露参数对象。

不推荐：

- 从字符串 path 手工解析。
- 从 Dictionary 取 object。
- 在 ViewModel 构造函数中访问全局 Router 解析参数。

## 6. Resolved Data 注入

Resolver 结果可以作为 ViewModel 初始化输入。

规则：

- 必需数据缺失时导航失败。
- 可选数据明确表达可空。
- 多个 Resolver 数据通过稳定 key 绑定。
- Source Generator 校验 key 和类型。

## 7. Activation 集成

Mvvm 负责 ViewModel Activation。Routing 负责把 Activation 放进导航事务中，确保 UI commit 成功后才把候选 ViewModel 标记为 active。

Routing 在准备阶段可以创建候选 ActivationScope，用于给 Presentation binding、UI 事件订阅和 Interaction handler 提供释放边界：

```text
Create provisional RouteScope
-> Create provisional ActivationScope
-> Create ViewModel
-> Presentation prepare binding
```

Routing 在提交成功后驱动：

```text
Mark ActivationScope running
-> Activate ViewModel
-> Bind State/EventBus/Interactions
```

如果 Presentation commit 失败，候选 ActivationScope 和 RouteScope 必须释放，ViewModel 不进入 active 状态。

离开路由时：

```text
Run Leave Guards
-> Stop accepting operations
-> Deactivate ViewModel
-> Dispose ActivationScope
-> Dispose RouteScope
```

ViewModel 构造函数只接收依赖和轻量数据，不启动长期任务。

## 8. Presentation 集成

Presentation 根据 ViewModel Target 或 ViewModel 实例定位 View。

Routing 给 Presentation 的信息：

- Navigation transaction id。
- Outlet commit plan。
- ViewModel instance。
- RouteContext。
- Reuse/KeepAlive 指令。

Presentation 返回：

- Commit success。
- Commit failed。
- View activation diagnostics。

Routing 不知道具体控件类型。

## 9. Reuse

复用发生在两个层面：

- 共同父路由保留已有 ViewModel。
- RouteReusePolicy 允许缓存已离开分支。

复用 ViewModel 时：

- 参数变化需要触发 RouteContext update。
- Resolver 数据变化需要触发 update。
- ActivationScope 是否重建由策略决定。
- 插件停用时必须强制释放。

## 10. 插件 ViewModel

插件 ViewModel 规则：

- 类型来自插件加载上下文。
- 服务来自插件 ServiceProvider。
- RouteScope 记录插件 Contribution。
- Host 不在静态字段保存插件 ViewModel 类型实例。
- 插件停用必须停用并释放相关 ViewModel。

跨插件边界传递给 Host 的类型必须位于共享 contract 程序集。

## 11. AOT 和 Source Generator

Source Generator 必须生成：

- ViewModelTargetDescriptor。
- 构造函数选择诊断。
- 参数绑定代码。
- Resolver 数据绑定代码。
- 必需服务诊断。
- 插件类型边界诊断。

Strict AOT 模式下应生成强类型 factory，避免运行时反射构造。

## 12. 错误策略

| 场景 | 默认处理 |
|---|---|
| ViewModel 创建失败 | Navigation failed，释放候选 RouteScope。 |
| 参数绑定失败 | Navigation failed。 |
| 必需 resolved data 缺失 | Navigation failed。 |
| Activation 失败 | 回滚或释放候选分支。 |
| Deactivation 失败 | 聚合诊断，继续释放。 |

## 13. 测试要求

测试必须覆盖：

- ViewModelTargetDescriptor 生成。
- 参数注入。
- resolved data 注入。
- 插件服务上下文创建 ViewModel。
- 创建失败回滚。
- Activation 失败释放 scope。
- Presentation commit failure 回滚。
- Reuse ViewModel 参数更新。
