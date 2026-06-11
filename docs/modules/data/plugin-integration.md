# AtomUI.City.Data Plugin Integration 设计

版本：v0.1
状态：正式初版
适用范围：插件 Data client、capability、请求取消、连接停止、缓存撤销、contract 隔离和卸载诊断。

## 1. 定位

Plugin integration 负责约束插件如何贡献和使用 Data client。

插件可以贡献 HTTP/gRPC/SignalR client，但必须经过 Host 管理的 descriptor、capability、lifecycle 和 pipeline。

## 2. 插件可贡献内容

插件可以贡献：

- HTTP client descriptor。
- gRPC client descriptor。
- SignalR hub descriptor。
- Serializer metadata。
- Auth metadata。
- Cache metadata。
- Resilience metadata。
- Connection lifetime metadata。

## 3. Capability

插件使用 Data client 必须有 capability。

示例：

- UseHttpClient。
- UseGrpcClient。
- UseSignalRHub。
- UseDataClient。
- UseRealtimeConnection。

未授权 capability 的 client contribution 不得进入 registry。

## 4. 生命周期

插件停用时：

```text
Stop new plugin data operations
-> cancel running operations
-> stop streams and realtime connections
-> revoke client descriptors
-> invalidate plugin cache
-> clear callbacks
-> dispose contribution leases
```

## 5. Contract 隔离

跨插件边界 DTO、event、stream item contract 必须位于 Host 共享 contract 程序集。

禁止：

- Host 静态缓存插件私有 client 实例。
- Host 长期持有插件私有 callback。
- 插件读取 Host token store。
- 插件绕过 Data pipeline。
- 插件启动非受控 connection 或 background receive loop。

## 6. 长连接

插件 SignalR/gRPC streaming 连接必须绑定插件 owner。

插件停用必须：

- 停止新消息投递。
- 取消 stream。
- stop connection。
- 移除 handler。
- 确认没有插件类型引用残留。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| capability denied | contribution rejected。 |
| plugin client conflict | contribution rejected。 |
| plugin unload with active connection | cancel and stop；失败进入 UnloadPending。 |
| plugin cache revoke failed | 聚合错误，继续清理。 |

## 8. 测试策略

测试必须覆盖：

- 插件 HTTP/gRPC/SignalR client 注册。
- capability denied。
- 插件停用取消请求。
- 插件停用关闭 SignalR connection。
- 插件 cache revoke。
- Host 不持有插件私有类型。
