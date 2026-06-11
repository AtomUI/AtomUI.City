# PluginSystem 安全设计

版本：v0.1
状态：正式初版
适用范围：插件来源、签名、hash、能力授权、加载边界和不可信代码约束

## 1. 目标

插件系统必须把安全边界说清楚。插件是运行在应用进程内的扩展代码，不能把进程内加载机制当作安全沙箱。

安全设计目标：

- 插件来源可追踪。
- 插件包内容可验证。
- 插件能力必须授权后才能生效。
- 插件不能绕过 Host contract 修改全局运行时。
- 插件错误默认隔离在插件边界内。
- 运行时加载隔离不被误认为安全隔离。

包签名、来源、hash 和信任等级的细化规则见：[签名和信任设计](signing-and-trust.md)。
能力授权的细化规则见：[能力授权设计](capabilities.md)。

## 2. 信任模型

插件信任由 Host policy 决定。

信任输入：

| 输入 | 用途 |
|---|---|
| Package source | 判断插件来自哪个仓库、文件或内部源。 |
| Package hash | 确认下载内容未变化。 |
| Signature | 验证发布者身份和包完整性。 |
| Publisher | 审计和策略匹配。 |
| PluginId | 授权、配置隔离、状态隔离。 |
| Capabilities | 判断插件请求的扩展范围。 |

规则：

- 未知来源插件可以被拒绝或要求用户确认。
- hash 不匹配必须拒绝安装。
- 签名策略由应用决定，可以分为 required、preferred、disabled。
- 发布者信息不能单独作为信任依据。
- 信任结果必须写入安装记录和锁定文件。

## 3. AssemblyLoadContext 不是安全边界

可卸载加载上下文只解决依赖隔离和卸载问题。

它不能防止插件：

- 执行任意托管代码。
- 访问进程内公开对象。
- 使用文件系统或网络 API。
- 消耗 CPU 和内存。
- 调用允许访问的 native API。

因此第一版不承诺进程内不可信插件沙箱。

需要运行不可信代码时，应使用进程隔离、受限 IPC、操作系统权限或专门沙箱机制，这不属于第一版 PluginSystem 的默认能力。

## 4. 能力授权

插件声明 requested capabilities，Host policy 产生 granted capabilities。

流程：

```text
Read manifest requested capabilities
-> Evaluate package trust
-> Evaluate application policy
-> Evaluate user consent if required
-> Produce granted capabilities
-> Store granted capabilities
-> Validate contributions against granted capabilities
```

规则：

- 插件声明能力不等于获得能力。
- 每个 Contribution 必须落在 granted capabilities 范围内。
- 能力拒绝必须进入诊断。
- 高风险能力可以要求用户确认或管理员策略。

示例能力：

| 能力 | 风险 |
|---|---|
| `routes` | 增加应用入口。 |
| `presentation.resources` | 影响 UI 呈现。 |
| `eventbus.subscribe` | 接收系统事件。 |
| `eventbus.publish` | 影响其他模块。 |
| `data.http` | 访问远程服务。 |
| `data.signalr` | 建立长期连接。 |
| `background.tasks` | 长期占用资源。 |
| `localization` | 影响展示文本。 |
| `settings` | 增加配置入口。 |

## 5. Contract 边界

插件只能通过 Host contract 接触框架能力。

规则：

- 插件不能获得 Host Root ServiceProvider。
- 插件不能直接写 Host Registry。
- 插件不能持有 Host 内部可变对象。
- 插件不能把插件私有类型泄漏为 Host 长期持有对象。
- 跨插件边界的事件、DTO、消息和 contract 必须位于 Host 共享 contract 程序集。

Host 共享 contract 程序集必须由默认加载上下文加载，插件只引用该 contract，不携带私有副本。

## 6. 文件和目录安全

插件安装目录必须和包缓存目录分离。

规则：

- 包缓存中的 `.nupkg` 不能直接作为运行时加载来源。
- `staging` 内容不能被加载。
- `installed` 中已完成安装的版本目录视为不可变。
- 插件运行时不能写自己的安装目录。
- 插件用户配置和状态应写入专门的 plugin data 目录，不写安装目录。
- 清理目录必须确认插件未加载且没有 `UnloadPending`。

## 7. Native 资产

插件携带 native/RID 资产时必须显式声明。

规则：

- native 资产必须进入清单和安装记录。
- native 资产参与 content hash。
- native 文件被系统锁定时，更新进入 pending。
- native 资产不能从未验证目录加载。
- AOT 场景下动态 native 插件加载需要应用显式支持。

## 8. 运行时隔离

运行时隔离依赖以下机制共同完成：

- 独立插件 ServiceScope。
- 可卸载加载上下文。
- Contribution Lease。
- Host contract。
- 生命周期取消。
- EventBus 订阅自动释放。
- Operation 绑定插件生命周期。
- 诊断追踪。

这些机制用于工程隔离和可靠卸载，不用于承诺安全沙箱。

## 9. 安全失败策略

| 失败 | 策略 |
|---|---|
| 来源不可信 | 拒绝安装或要求用户确认。 |
| hash 不匹配 | 拒绝安装。 |
| 签名无效 | 按 Host policy 拒绝或禁用。 |
| 能力未授权 | 拒绝对应 Contribution。 |
| contract 泄漏 | 拒绝启用或进入 Faulted。 |
| 插件运行期安全错误 | 停用插件并记录诊断。 |

安全失败不能静默忽略。

## 10. 诊断和测试

必须覆盖：

- 未知来源安装。
- hash 不匹配。
- 签名缺失或无效。
- 能力拒绝。
- 未授权 Contribution。
- 插件尝试访问未暴露 Host 服务。
- 插件私有类型泄漏到 Host registry。
- `AssemblyLoadContext` 卸载后无插件对象残留。
- native 文件锁定导致 pending update。

诊断信息必须包含 source、hash、signature status、trust result、PluginId、capability、ContributionId 和处理策略。
