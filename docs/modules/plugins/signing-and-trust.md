# PluginSystem 签名和信任设计

版本：v0.1
状态：正式初版
适用范围：插件包来源、签名、hash、发布者、信任策略和审计记录

## 1. 目标

插件是进程内代码，安装前必须能确认来源和内容。签名和信任系统用于决定插件是否允许安装、启用和获得能力。

设计目标：

- 包来源可追踪。
- 内容完整性可验证。
- 签名策略可配置。
- 发布者身份可审计。
- 信任结果写入安装记录和锁定文件。

## 2. 信任输入

| 输入 | 说明 |
|---|---|
| Package source | 插件源、feed、本地文件路径。 |
| Package hash | `.nupkg` hash。 |
| Content hash | 解压后运行内容 hash。 |
| Signature | 包签名或独立签名。 |
| Certificate thumbprint | 证书指纹。 |
| Publisher id | 发布者身份。 |
| PluginId | 插件运行时身份。 |
| Capabilities | 请求能力范围。 |

## 3. 签名策略

Host 可以配置签名策略：

| 策略 | 说明 |
|---|---|
| Required | 无有效签名拒绝安装。 |
| Preferred | 无签名允许安装但降低信任等级或要求确认。 |
| Disabled | 不检查签名，只检查 hash 和来源策略。 |

默认建议由应用决定。企业应用通常应使用 `Required`。

## 4. 信任等级

建议信任等级：

| 等级 | 说明 |
|---|---|
| Trusted | 来源和签名均受信任。 |
| Verified | hash 匹配，但签名策略不要求或无发布者信任链。 |
| UserAccepted | 用户显式确认安装。 |
| Untrusted | 不允许启用。 |
| Blocked | 被 Host policy 阻止。 |

信任等级影响能力授权。高风险能力可以要求 `Trusted`。

## 5. 安装时校验

```text
Resolve package source
-> Verify package hash
-> Verify signature if required
-> Check publisher policy
-> Check PluginId policy
-> Compute trust result
-> Store trust result
```

规则：

- hash 不匹配必须拒绝安装。
- 签名无效按策略拒绝或降级。
- 来源被阻止时拒绝安装。
- 信任结果必须进入 `install.json`。

## 6. 启用时复核

启用插件前应复核：

- 安装记录存在。
- content hash 未变化。
- 清单 hash 未变化。
- 锁定文件来源和安装记录一致。
- 信任结果仍满足 Host policy。

如果安装后文件被修改，插件必须进入 Invalid 或 Disabled。

## 7. 审计记录

审计记录应包含：

- operation id。
- package source。
- PluginId。
- PackageId。
- Version。
- package hash。
- content hash。
- signature status。
- certificate thumbprint。
- trust level。
- user/admin decision。

## 8. 非目标

第一版不承诺：

- 进程内不可信代码沙箱。
- 插件市场服务端。
- 远程吊销检查。
- 自动证书生命周期管理。

这些能力可以在后续安全模块或企业策略模块中扩展。

## 9. 测试要求

必须覆盖：

- 有效签名。
- 缺失签名。
- 无效签名。
- hash 不匹配。
- 不可信来源。
- 用户确认安装。
- 信任等级影响能力授权。
- 安装后文件被篡改。
