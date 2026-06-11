# AtomUI.City.Data Routing Integration 设计

版本：v0.1
状态：正式初版
适用范围：Resolver 调用 Data、导航取消、ResolveResult 映射、预取、缓存和诊断。

## 1. 定位

Routing Resolver 可以调用 Data client，为页面进入准备首屏必需数据。

Resolver 不实现自己的请求管线，不直接访问 transport，不隐式写全局 State。

## 2. Resolver 链路

```text
Resolver
-> Data client
-> Data pipeline
-> DataResult
-> ResolveResult
-> RouteContext data
```

Data 请求必须接收 Resolver 的 `CancellationToken`。

## 3. 错误映射

| DataResult | ResolveResult |
|---|---|
| Success | Success(data)。 |
| Cancelled | Cancelled。 |
| NotFound | NotFound。 |
| AuthenticationRequired | Redirect / Challenge，按 route policy。 |
| AuthorizationForbidden | Failed 或 forbidden route，按 route policy。 |
| Timeout / NetworkUnavailable | Failed 或 retry route，按 Host 策略。 |

Routing 决定导航结果，Data 只提供标准错误。

## 4. 预取和缓存

Resolver 可以使用 Data cache。

规则：

- Resolver cache 必须绑定 RouteScope、NavigationScope 或 Data cache。
- 不使用无边界静态缓存。
- Journal 恢复时只能复用可序列化快照。
- Principal change 必须让受保护数据缓存失效。

## 5. 取消

导航取消时：

- Resolver cancellation token 取消。
- Data OperationScope 取消。
- transport 请求取消。
- 返回 ResolveResult Cancelled。
- 不提交 State。

## 6. 插件 Resolver

插件 Resolver 调用插件 Data client 时：

- client 来自插件 service context。
- operation 绑定插件 contribution。
- 插件停用取消请求。
- DTO 跨边界必须位于 Host 共享 contract 程序集。

## 7. 测试策略

测试必须覆盖：

- Resolver Data success。
- Data NotFound 映射。
- Data auth failure 映射。
- 导航取消取消 Data 请求。
- Resolver cache。
- 插件停用取消 Resolver Data 请求。
