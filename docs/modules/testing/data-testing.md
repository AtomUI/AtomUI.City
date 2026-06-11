# Data 测试设计

版本：v0.1
状态：正式初版
适用范围：HTTP、gRPC、SignalR、request pipeline、streaming、realtime、缓存、认证、并发、取消和插件连接

## 1. 目标

Data 测试必须证明多种数据访问方式在统一管线下可测试、可取消、可诊断。

## 2. DataTransportFakes

Testing 提供：

- fake HTTP transport。
- fake gRPC transport。
- fake SignalR transport。
- fake streaming response。
- fake realtime connection。
- fake access token provider。
- fake cache。
- fake resilience policy。
- request recorder。

## 3. 单元测试范围

必须覆盖：

- request pipeline handler 顺序。
- HTTP success/failure。
- gRPC success/failure。
- SignalR connect/reconnect/stop。
- streaming read。
- realtime message。
- cancellation。
- timeout。
- retry。
- cache hit/miss。
- cache invalidation。
- auth token injection。
- auth refresh failure。
- error mapping。
- diagnostics。

## 4. 多线程和异步

必须覆盖：

- 不阻塞 UI thread。
- callback 不直接更新 UI。
- late result suppression。
- concurrent request。
- connection lifecycle。
- backpressure。
- large payload progress。

## 5. 插件测试

必须覆盖：

- 插件 Data client contribution。
- 插件 SignalR connection 停止。
- 插件卸载取消 active request。
- 插件卸载停止 stream。
- 插件卸载清理 cache scope。

## 6. 集成测试范围

Framework integration test 覆盖：

```text
Data
-> Security token
-> Request pipeline
-> Fake transport
-> State update or EventBus publish
```

真实远程服务不属于默认集成测试。
