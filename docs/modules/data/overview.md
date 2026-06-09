# AtomUI.City.Data

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Data` 负责数据请求、客户端代理、请求管线、缓存、错误模型、取消、重试和认证集成。

Data 的目标是让应用数据访问具备统一入口、统一错误处理和统一生命周期。

## 边界

Data 可以依赖：

- Microsoft.Extensions.Http
- Polly
- Security 抽象
- State 抽象

Data 不负责：

- 领域模型设计。
- 仓储模式。
- 应用服务分层。
- UI 状态展示。

## 后续拆分

- `request-pipeline.md`
- `client-proxy.md`
- `caching.md`
- `error-model.md`
- `resilience.md`
