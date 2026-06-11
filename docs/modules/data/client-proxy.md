# AtomUI.City.Data Client Proxy 设计

版本：v0.1
状态：正式初版
适用范围：typed client、generated client、adapter client、client descriptor、Refit 可选适配和 AOT/source generator。

## 1. 定位

Client proxy 负责给应用提供可注入、可测试、可诊断的数据客户端入口。

Data Core 不强制应用使用某一种代理框架。第一版支持 typed client 和 generated client；Refit、RPC、本地服务等通过 adapter 包接入。

## 2. Client 类型

| 类型 | 说明 |
|---|---|
| Typed client | 推荐默认方式，符合 .NET DI 和测试习惯。 |
| Generated client | Source Generator 生成 descriptor 和调用代码。 |
| Adapter client | Refit、RPC、本地服务、插件服务等适配。 |

Data client 不应直接暴露裸 transport 给 ViewModel。

## 3. Client Descriptor

Client descriptor 应包含：

- ClientId。
- Transport kind。
- Operation descriptors。
- Auth metadata。
- Cache metadata。
- Resilience metadata。
- Serializer metadata。
- Connection lifetime metadata。
- Plugin contribution。

Descriptor 由显式注册或 Source Generator 生成。

## 4. Operation Descriptor

Operation descriptor 应包含：

- Operation name。
- Request/response 类型。
- Access mode：query、mutation、subscription、upload、download。
- Concurrency policy。
- Timeout / deadline。
- Retry policy。
- Cache policy。
- Auth policy。
- Diagnostics category。

## 5. Refit 适配

Refit 可以作为 `AtomUI.City.Data.Refit` 可选适配包。

规则：

- Refit 不进入 Data Core 唯一范式。
- Refit client 也必须进入 Data pipeline。
- Refit metadata 必须转换为 Data descriptor。
- 错误必须映射为 DataError。

## 6. 插件 client

插件 client 必须通过 Contribution 注册。

规则：

- 插件 client descriptor 必须可撤销。
- 插件停用后不能创建新 client。
- 插件私有 DTO 不能跨 Host 边界长期持有。
- 插件 client 的 operation 必须经过 capability 检查。

## 7. 测试策略

测试必须覆盖：

- typed client 创建。
- generated descriptor 注册。
- adapter client 进入 pipeline。
- 未声明 client 诊断。
- 插件 client 撤销。
