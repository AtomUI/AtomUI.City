# AtomUI.City.Routing Resolvers 设计

版本：v0.1
状态：正式初版
适用范围：路由数据解析、Resolver 生命周期、取消、错误、缓存、与 Data/State/Mvvm 的集成。

## 1. 定位

Resolver 在路由激活前准备页面进入所必需的数据。

Resolver 的目标是把页面进入前的必要数据准备放到 Routing 生命周期中，而不是散落在 ViewModel 构造函数里。

Resolver 不负责长期状态维护，不替代 Data 模块，不负责 UI loading 控件。

## 2. 解析数据边界

适合 Resolver 的数据：

- 页面必须参数校验。
- 首屏必须数据。
- 权限策略需要的上下文数据。
- ViewModel 初始化需要的只读输入。
- Deep Link 还原所需数据。

不适合 Resolver 的数据：

- 页面进入后可延迟加载的数据。
- 长期实时订阅。
- 大型缓存。
- UI 临时状态。
- 业务流程状态机。

这些应进入 Data、State、ViewModel Activation 或业务服务。

## 3. Resolver Contract

建议语义：

```text
IRouteResolver<TData>
ResolveAsync(RouteResolveContext context, CancellationToken cancellationToken)
```

`RouteResolveContext` 应包含：

- RouteId。
- RouteScope。
- 参数。
- 父路由解析数据。
- Service provider。
- Contribution 信息。
- Diagnostics。
- CancellationToken。

Resolver 返回 `ResolveResult<TData>`，不返回裸数据。

## 4. 结果模型

Resolver 结果：

| 结果 | 说明 |
|---|---|
| `Success(data)` | 解析成功。 |
| `NotFound` | 必需数据不存在。 |
| `Redirect(target)` | 跳转其他路由。 |
| `Cancelled` | 被取消。 |
| `Failed(error)` | 异常失败。 |

NotFound 可以由路由错误策略转换为错误路由或导航失败。

## 5. 执行顺序

默认顺序：

```text
Parent resolvers
-> Child resolvers
```

同一路由多个 Resolver 默认按声明顺序执行。只有显式声明互不依赖时，才允许并行。

原因：

- 诊断稳定。
- 数据依赖明确。
- 失败位置清楚。
- 避免不必要的并发复杂度。

## 6. 与 RouteScope

Resolver 运行在 provisional RouteScope 内。

规则：

- Resolver 可以解析 scoped 服务。
- Resolver 创建的可释放资源必须注册到 RouteScope。
- Resolver 失败时释放 provisional RouteScope。
- Resolver 不能把插件私有对象交给 Host 长期保存。
- Resolver 不能直接访问 View。

## 7. 与 Data 集成

Data 模块负责请求管线、认证注入、重试、缓存和错误标准化。

Resolver 可以调用 Data client，但不自己实现 Data 管线。

推荐关系：

```text
Resolver
-> Data client
-> Data pipeline
-> ResolveResult
-> RouteContext data
```

Data 请求必须接收 Resolver 的 CancellationToken。

## 8. 与 State 集成

Resolver 结果可以：

- 作为 RouteContext data 暴露给 ViewModel。
- 初始化 RouteScope 级 State。
- 写入应用级 State，但必须显式。

Resolver 不应隐式修改全局状态。需要写全局状态时，必须通过明确服务和诊断记录。

## 9. 注入到 ViewModel

ViewModel Target 可以声明需要的 resolved data。

规则：

- Resolver key 必须稳定。
- Source Generator 校验 key 与 ViewModel Target 绑定。
- 缺失必需 resolved data 时导航失败。
- 可选 resolved data 可以为 null 或默认值。

## 10. Loading 状态

Routing 可以暴露 `NavigationStatus` 表示正在 resolving。

Presentation 可据此展示导航级 loading。具体 loading UI 由 Presentation 和应用决定。

Resolver 不直接控制 UI loading。

## 11. 缓存策略

第一版默认不缓存 Resolver 结果。

可选策略：

- 当前 RouteScope 内复用。
- Journal 恢复时复用可序列化快照。
- 显式 RouteReusePolicy 缓存。

Resolver 缓存必须绑定 RouteScope、NavigationScope 或 Data cache，不能使用无边界静态缓存。

## 12. 插件 Resolver

插件 Resolver 运行在插件服务上下文中。

插件停用时：

- 阻止新 Resolver。
- 取消运行中 Resolver。
- 释放 provisional RouteScope。
- 不写入新的 NavigationSnapshot。

Resolver 返回的数据如果跨插件边界传递，类型必须位于 Host 共享 contract 程序集。

## 13. 错误策略

| 场景 | 默认处理 |
|---|---|
| 参数无效 | Navigation failed。 |
| Resolver Cancelled | Navigation cancelled。 |
| Resolver NotFound | 进入 not-found 策略。 |
| Resolver Redirect | 重启导航到新目标。 |
| Resolver Failed | Navigation failed 或错误路由。 |

错误策略必须释放已创建的 provisional scope。

## 14. 诊断

必须记录：

- Resolver 类型。
- RouteId。
- Contribution。
- 输入参数摘要。
- 耗时。
- 结果。
- Data request correlation id。
- 取消来源。
- 错误信息。

## 15. 测试要求

测试必须覆盖：

- 父子 Resolver 顺序。
- Resolver success data 注入。
- Resolver NotFound。
- Resolver Redirect。
- Resolver 取消。
- Resolver 失败释放 provisional scope。
- 插件停用取消 Resolver。
- resolved data 类型边界。
