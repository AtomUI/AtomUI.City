# AtomUI.City.Security Permissions 设计

版本：v0.1
状态：正式初版
适用范围：权限点声明、命名、权限贡献、插件权限撤销、本地化、AOT/source generator 和诊断。

## 1. 定位

Permission 是框架可识别的稳定权限点。

Permission 不是业务角色，不是用户权限表，也不是 UI 菜单项。它只表达一个可被授权系统检查的能力标识。

## 2. Permission Descriptor

Permission descriptor 建议包含：

| 字段 | 说明 |
|---|---|
| Name | 稳定权限名。 |
| DisplayNameKey | 本地化显示文本 key。 |
| DescriptionKey | 本地化描述 key。 |
| Category | 权限分类。 |
| Contribution | 来源模块或插件。 |
| DefaultPolicy | 默认授权策略引用。 |
| IsHostOnly | 是否仅 Host 可授予。 |

权限名必须稳定，不应使用运行时随机值。

## 3. 命名规则

建议使用小写点分命名：

```text
settings.read
settings.write
project.build
plugin.sales.export
```

规则：

- Host 内置权限不能被插件覆盖。
- 插件权限建议带插件或模块命名空间前缀。
- 权限名大小写敏感策略在实现前统一确定，文档默认按大小写敏感处理。
- 权限名一旦发布，应避免破坏性重命名。

## 4. 权限贡献

模块和插件通过 Contribution 提交权限声明：

```text
Module / Plugin
-> PermissionContribution
-> SecurityContributionRegistry
-> PermissionManifest
```

规则：

- 插件不能直接写权限 registry。
- 插件权限必须有 ContributionLease。
- 插件停用时撤销对应权限和 policy metadata。
- 已撤销权限不能再被 Route、Command 或 Data 使用。
- 活动授权缓存必须按 contribution revision 失效。

## 5. 本地化

Permission descriptor 不直接存储显示文本，只存本地化 key。

Localization 负责资源查找和文化切换。Security 只保存 key 和 metadata。

## 6. Source Generator

Security generator 负责：

- 生成 permission manifest。
- 生成 permission descriptor 注册代码。
- 诊断重复权限名。
- 诊断未声明权限引用。
- 诊断插件覆盖 Host 权限。
- 诊断权限名不符合规范。

运行时默认不扫描程序集找权限。

## 7. 错误策略

| 场景 | 默认处理 |
|---|---|
| 重复权限名 | 构建期诊断。 |
| 未声明权限引用 | 构建期诊断；动态场景运行时 Failed。 |
| 插件权限撤销失败 | 聚合错误，继续撤销其他权限。 |
| 权限本地化缺失 | 使用权限名 fallback，并记录诊断。 |

## 8. 测试策略

测试必须覆盖：

- Host 权限注册。
- 插件权限注册和撤销。
- 重复权限诊断。
- 未声明权限引用诊断。
- contribution revision 变化后缓存失效。
