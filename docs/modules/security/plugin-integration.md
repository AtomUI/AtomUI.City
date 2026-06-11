# AtomUI.City.Security Plugin Integration 设计

版本：v0.1
状态：正式初版
适用范围：插件权限贡献、Policy requirement、Capability、Host 授权、撤销、缓存失效和 contract 隔离。

## 1. 定位

Plugin integration 负责约束插件如何参与 Security。

插件可以声明权限和授权元数据，但不能成为全局权限解释者。Host Security 是唯一授权决策入口。

## 2. 插件可贡献内容

插件可以贡献：

- Permission descriptor。
- Policy requirement descriptor。
- Route auth metadata。
- Command auth metadata。
- Data client auth metadata。
- Capability request。

所有贡献都必须通过 Contribution Request 和 ContributionLease 进入 Security registry。

## 3. Capability

Capability 表达 Host 允许插件使用的框架能力。

示例：

| Capability | 说明 |
|---|---|
| ContributeRoutes | 允许贡献路由。 |
| ContributeCommands | 允许贡献命令。 |
| UseDataClient | 允许访问指定 Data client。 |
| SubscribeEvents | 允许订阅指定事件 contract。 |
| PublishEvents | 允许发布指定事件 contract。 |
| ContributePresentationResources | 允许贡献 View、Style、Icon 等资源。 |

Capability 不等同于业务权限。Capability 是 Host 对插件能力的授权，Permission 是用户或主体对业务能力的授权。

## 4. Host 授权

插件启用时：

```text
Plugin metadata
-> Capability request
-> Host policy check
-> grant / reject capabilities
-> accept Security contributions
```

规则：

- 未授予 capability 的插件贡献不得进入 registry。
- 插件不能扩大自己的 capability。
- Capability grant 必须可诊断。
- Capability 变化必须触发相关授权缓存失效。

## 5. Contract 隔离

跨插件边界的授权 contract 必须位于 Host 共享 contract 程序集。

禁止：

- Host policy 依赖插件私有 requirement 类型。
- Host 静态缓存持有插件私有授权对象。
- 插件直接修改 Host principal。
- 插件绕过 Security 自行决定全局权限。

## 6. 撤销和卸载

插件停用时：

```text
Stop new Security checks from plugin contributions
-> revoke route / command / data auth metadata
-> revoke permissions and policies
-> invalidate authorization cache
-> recompute active commands and guards
-> dispose contribution leases
```

如果插件还有活动 RouteScope、ActivationScope 或 OperationScope，Host 必须先关闭相关运行实例，再释放插件 Security contribution。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| 插件权限名冲突 | 拒绝该 contribution。 |
| 插件请求未授权 capability | 拒绝对应 contribution。 |
| 插件 requirement 类型泄漏 | 构建期或启用期诊断。 |
| 撤销失败 | 聚合错误，继续撤销其他 contribution。 |
| 卸载后仍有授权缓存引用 | 标记 UnloadPending，输出诊断。 |

## 8. 测试策略

测试必须覆盖：

- 插件权限贡献成功。
- 插件权限冲突被拒绝。
- 未授权 capability 被拒绝。
- 插件停用后权限和缓存撤销。
- Host 不持有插件私有类型引用。
