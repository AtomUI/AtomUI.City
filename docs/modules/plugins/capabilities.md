# PluginSystem 能力授权设计

版本：v0.1
状态：正式初版
适用范围：插件能力声明、授权、能力范围、Contribution 校验和诊断

## 1. 目标

能力系统用于约束插件可以向 Host 增加什么能力。它不是安全沙箱，但它是 Host 策略、用户授权和诊断的基础。

设计目标：

- 插件在清单中声明 requested capabilities。
- Host policy 产生 granted capabilities。
- Contribution 必须在授权能力范围内。
- 能力范围可表达路由、数据客户端、事件、后台任务等边界。
- 能力授权结果可审计、可撤销。

## 2. requested 和 granted

插件清单声明请求能力：

```json
{
  "capabilities": [
    {
      "name": "routes",
      "scope": ["/sales/**"]
    },
    {
      "name": "data.http",
      "clients": ["SalesApi"]
    }
  ]
}
```

Host 评估后生成授权能力：

```json
{
  "grantedCapabilities": [
    {
      "name": "routes",
      "scope": ["/sales/**"]
    }
  ]
}
```

规则：

- requested capability 不等于 granted capability。
- 未授权能力不能产生 Contribution。
- 授权结果必须写入锁定文件和安装记录。
- 能力变更可能要求插件重新启用。

## 3. 能力目录

第一版建议能力：

| 能力 | 范围 |
|---|---|
| `modules` | 插件模块声明。 |
| `services` | 插件私有服务和受控 contract 服务。 |
| `routes` | 路由 path pattern。 |
| `presentation.views` | View/ViewModel 映射。 |
| `presentation.resources` | 样式、图标、菜单、工具栏资源。 |
| `commands` | 命令和动作入口。 |
| `permissions` | 权限点声明。 |
| `localization` | 本地化资源。 |
| `eventbus.subscribe` | 可订阅事件 contract。 |
| `eventbus.publish` | 可发布事件 contract。 |
| `data.http` | HTTP client 名称。 |
| `data.grpc` | gRPC client 名称。 |
| `data.signalr` | SignalR connection 名称。 |
| `background.tasks` | 后台任务类型或数量限制。 |
| `settings` | 设置页面和配置 section。 |
| `diagnostics` | 诊断 provider。 |

## 4. 范围表达

能力范围必须尽可能具体。

示例：

```json
{
  "name": "eventbus.subscribe",
  "contracts": [
    "Company.Contracts.SalesOrderChanged"
  ]
}
```

```json
{
  "name": "routes",
  "scope": [
    "/sales/**",
    "/reports/sales"
  ]
}
```

规则：

- 路由能力不能使用全局通配，除非 Host 显式授权。
- Data 能力必须声明 client 名称。
- EventBus 能力必须声明共享 contract。
- 后台任务能力必须绑定生命周期和取消策略。

## 5. 授权流程

```text
Read requested capabilities
-> Check package trust
-> Check Host policy
-> Check user/admin consent
-> Produce granted capabilities
-> Validate contribution manifests
-> Apply contributions with lease
```

规则：

- 能力授权发生在 Contribution 应用前。
- 授权失败不一定阻止插件安装。
- 必需能力被拒绝时，插件不能启用。
- 可选能力被拒绝时，插件可以降级启用。

## 6. Contribution 校验

每个 Contribution 必须校验来源和能力。

校验输入：

- PluginId。
- ModuleId。
- ContributionId。
- Contribution type。
- requested capabilities。
- granted capabilities。
- Host registry policy。

校验失败时，不创建 lease。

## 7. 能力撤销

能力撤销流程：

```text
Update granted capabilities
-> Stop new plugin entry
-> Revoke affected contribution leases
-> Cancel affected operations
-> Mark plugin degraded or inactive
```

规则：

- 能力撤销不能留下已生效 Contribution。
- 正在运行的 Operation 必须收到取消。
- UI 入口应禁用或移除。
- 诊断必须记录撤销原因。

## 8. 测试要求

必须覆盖：

- 未授权能力被拒绝。
- 必需能力被拒绝导致插件不能启用。
- 可选能力被拒绝后降级启用。
- Contribution 超出 routes 范围。
- EventBus 使用未声明 contract。
- Data client 未授权。
- 能力撤销后 lease 被撤销。
