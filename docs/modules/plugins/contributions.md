# PluginSystem 贡献模型设计

版本：v0.1
状态：正式初版
适用范围：插件可贡献能力、贡献申请、Contribution Lease、撤销规则和推荐规范

## 1. 目标

插件贡献模型决定插件能给应用增加什么能力，以及这些能力如何被安全撤销。

核心原则：

- 插件不能直接永久修改 Host。
- 插件不能直接修改 Host Root ServiceProvider。
- 插件贡献必须可追踪来源。
- 插件贡献必须可撤销。
- 插件贡献必须记录对应 Plugin 和 Module。
- 插件卸载前必须撤销所有贡献。

架构级范围见：[插件系统架构规范](../../architecture/plugin-system.md)。

## 2. 可贡献能力

第一版推荐允许插件贡献：

| 能力 | 推荐级别 | 说明 |
|---|---:|---|
| Module | 推荐 | 插件功能组织入口，可以携带服务、配置、资源和初始化逻辑。 |
| Services | 推荐 | 插件私有服务或受控 contract 服务。 |
| Routes | 推荐 | 页面路由、子路由、导航元数据、route guard。 |
| ViewModel | 推荐 | 插件页面或组件自己的 ViewModel。 |
| Views / Presentation Resources | 推荐 | View、样式、图标、菜单项、工具栏入口。 |
| Commands / Actions | 推荐 | 菜单动作、工具栏动作、快捷入口。 |
| Permissions | 推荐 | 权限点声明，交由 Security 统一解释。 |
| Localization | 推荐 | 本地化资源和文化切换刷新入口。 |
| EventBus handlers | 推荐 | 类型事件处理器或通道订阅。 |
| Data clients | 可选推荐 | API client、本地服务 client、插件数据 client。 |
| Background tasks | 谨慎 | 必须绑定插件生命周期并支持取消。 |
| Settings pages | 推荐 | 插件配置页面和配置模型。 |
| Diagnostics providers | 可选 | 插件诊断信息提供者。 |

禁止或不推荐：

| 能力 | 结论 | 原因 |
|---|---|---|
| Host Root ServiceProvider 修改 | 禁止 | 破坏隔离和卸载。 |
| 生命周期内核替换 | 禁止 | 只能通过 middleware 扩展生命周期。 |
| 全局静态状态 | 禁止 | 会阻止卸载并制造隐式耦合。 |
| 非受控线程 | 禁止 | 无法统一取消和诊断。 |
| 绕过 Security 的权限逻辑 | 禁止 | 权限必须由 Host 统一解释。 |
| 绕过 Data 管线的数据访问 | 不推荐 | 会绕过认证、错误处理、resilience 和诊断。 |
| 进程内不可信代码沙箱 | 禁止承诺 | 加载上下文不是安全边界。 |

## 3. Contribution Request

插件不直接写 registry，而是提交 Contribution Request。

```text
Plugin module
-> ContributionRequest
-> Host validation
-> Target registry
-> ContributionLease
```

Contribution Request 应包含：

- 插件 Id。
- 插件版本。
- 贡献的 Plugin。
- 贡献的 Module。
- 贡献类型。
- 贡献标识。
- 目标 registry。
- 依赖能力。
- 权限声明。
- 撤销策略。
- 诊断元数据。

Host 可以拒绝 request，例如能力被禁用、权限不足、版本不兼容、目标 registry 不支持动态注册或贡献标识冲突。

## 4. Contribution Lease

Contribution Lease 是插件贡献进入 Host 后的可撤销句柄。

Lease 必须包含：

- Lease Id。
- 贡献的 Plugin。
- 贡献的 Module。
- 贡献类型。
- 目标 registry。
- 注册结果。
- 撤销动作。
- 当前状态。
- 创建时间和撤销时间。
- 诊断信息。

Lease 状态建议：

```text
Created
-> Active
-> Revoking
-> Revoked
```

错误状态：

```text
Failed
RevokeFailed
```

Lease 撤销必须幂等。重复撤销不能导致二次释放异常。

## 5. Registry 要求

任何接收插件贡献的 registry 都必须支持：

- 动态注册。
- 动态撤销。
- 贡献归属追踪。
- 冲突检测。
- 启用/禁用。
- 诊断输出。
- 撤销失败报告。

如果某个 registry 无法撤销，它只能接收启动期静态模块贡献，不能接收运行时插件贡献。

## 6. 撤销顺序

插件停用或卸载时，贡献撤销必须在停止新入口和取消操作之后执行。

推荐顺序：

```text
Stop accepting new plugin entry
-> Deactivate plugin routes and view models
-> Cancel plugin operations
-> Revoke contribution leases in reverse order
-> Dispose plugin subscriptions and resources
-> Dispose plugin service scope
```

按反向顺序撤销可以降低依赖倒挂风险。例如菜单入口依赖命令，命令依赖服务，服务依赖插件 ServiceScope；撤销时应先撤销菜单入口，再撤销命令，再释放服务。

## 7. 贡献冲突

Host 必须处理插件贡献冲突。

常见冲突：

- 路由路径重复。
- 权限点重复但语义不同。
- 命令 Id 重复。
- 本地化 key 重复。
- ViewModel Target 或 View/ViewModel 绑定重复。
- Data client 名称重复。

默认策略：

- 相同插件内重复贡献视为插件错误。
- 不同插件贡献同一 Id 时默认拒绝后加载插件。
- Host 可以提供显式 override 策略，但必须记录诊断。
- 冲突不能通过注册顺序静默覆盖。

## 8. 能力声明

插件应在元数据中声明需要的能力。

推荐能力：

- routes
- presentation
- commands
- permissions
- localization
- eventbus
- data
- backgroundTasks
- diagnostics

Host 在加载前可以根据能力声明决定是否允许插件加载。能力声明不等同于安全沙箱，但可以作为兼容性、策略和诊断依据。

## 9. 推荐规范

插件开发推荐：

- 所有贡献集中声明，避免运行中散落注册。
- 所有长期订阅绑定插件生命周期或相关运行 Scope。
- 所有后台任务支持取消。
- 所有 UI 入口支持禁用和撤销。
- 所有 route guard、command、data client 使用 Host 提供的上下文。
- 不保存 Host、Scope、ServiceProvider、ViewModel 或插件类型实例到静态字段。
- 不把插件内部类型泄漏为 Host 长期持有的对象。

## 10. 测试要求

Testing 包后续应支持：

- 构造插件贡献 request。
- 断言 request 被接受或拒绝。
- 断言 lease 创建。
- 断言 lease 反向撤销顺序。
- 模拟 registry 撤销失败。
- 断言插件卸载前无剩余 active lease。
- 断言贡献冲突诊断。
