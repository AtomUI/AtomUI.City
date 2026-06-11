# AtomUI.City 编程范式

版本：v0.1
状态：正式初版
适用范围：应用开发者面对 AtomUI.City 时的默认组织方式和约束

## 1. 定位

AtomUI.City 是应用框架，不只是工具库。它会提供一套默认编程范式，并要求应用按这套范式组织模块、路由、ViewModel、状态、数据请求、权限和生命周期。

默认路径：

```text
创建模块
-> 注册服务和配置
-> 声明路由和权限
-> 编写 ViewModel
-> 使用 State / Data / EventBus / Security
-> 由 Lifecycle 管理激活、订阅和释放
-> 由 Presentation 接入 AtomUI/Avalonia
```

## 2. 核心约定

应用开发者需要接受以下约定：

- 功能通过模块组织。
- 页面进入通过路由组织。
- UI 交互逻辑通过 ViewModel 承载。
- 可持续状态进入 State 体系。
- 长期订阅绑定 Activation Scope。
- 跨模块通信通过 EventBus。
- 权限进入 Security。
- 数据请求进入 Data 管线。
- UI 集成通过 Presentation。

## 3. ViewModel 约定

ViewModel 是 UI 交互和 UI 状态组织的主要承载点。

ViewModel 不应在构造阶段建立长期订阅。状态订阅、EventBus 订阅、Interaction Handler、Reaction 和可释放资源应绑定到 Activation Scope。

命令使用 .NET MVVM 生态已有名称，例如：

- `IRelayCommand`
- `IAsyncRelayCommand`
- `RelayCommand`
- `AsyncRelayCommand`

框架不为命令类型额外添加 City 前缀。

## 4. 状态约定

State 体系需要提供：

- 当前值语义。
- 可写状态。
- 计算状态。
- Reaction。
- 状态快照。
- 状态作用域。

`IObservable<T>` 可以作为适配能力存在，但不是 State 的主公共 API。

## 5. 扩展约定

框架允许应用引入自己的领域层、应用服务层、DDD、CQRS、工作台模型或其他业务组织方式。

这些结构属于应用内部架构，不是 AtomUI.City v1 默认强加的框架职责。

## 6. AOT 与生成式约定

AtomUI.City 的默认编程范式必须对 AOT、trimming、启动性能和包体积友好。

应用开发者应优先使用显式注册和强类型 API，例如显式注册模块、路由、ViewModel Target、View/ViewModel 绑定、权限、本地化资源和插件能力。框架应优先通过 source generator、analyzer、构建期 manifest 和模板生成来减少运行时反射。

运行时程序集扫描、命名约定反射发现、动态代理、表达式树编译和动态插件加载都不应成为默认路径。确实需要这些能力时，必须作为 opt-in 能力暴露，并在文档、Build 或 Analyzer 中给出 AOT/trimming 兼容性诊断。
