# PluginSystem 设置和状态迁移设计

版本：v0.1
状态：正式初版
适用范围：插件配置、用户状态、版本升级、回滚、迁移声明和降级策略

## 1. 目标

插件升级不只替换程序集，还可能改变配置结构、用户状态和缓存格式。迁移设计必须避免新版本启用失败时破坏旧版本可用性。

设计目标：

- 插件配置按 PluginId 隔离。
- 设置 schema 版本可声明。
- 更新前保留旧版本状态。
- 迁移操作可诊断、可回滚或可中止。
- 第一版不承诺复杂自动降级迁移。

## 2. 数据分类

| 类型 | 说明 |
|---|---|
| Plugin configuration | 用户配置、管理员配置、默认配置。 |
| Plugin state | 页面状态、用户偏好、工作区状态。 |
| Plugin cache | 可重建缓存。 |
| Plugin secrets | token、密钥引用，必须交给安全存储。 |
| Plugin diagnostics | 运行诊断和错误记录。 |

安装目录不存放可变数据。

## 3. 配置隔离

配置路径按 PluginId 分区：

```text
plugins/config/<plugin-id>/
plugins/state/<plugin-id>/
plugins/cache/<plugin-id>/
```

规则：

- 插件不能默认写 Host 全局配置。
- 插件只能访问自己的配置 section。
- 访问 Host 配置必须通过授权 contract。
- 卸载插件默认不删除用户配置和状态。

## 4. Schema 声明

插件清单可以声明：

```json
{
  "settings": {
    "schemaVersion": "2.0",
    "defaultConfiguration": "manifests/settings.defaults.json",
    "schema": "manifests/settings.schema.json",
    "migration": {
      "from": "[1.0,2.0)",
      "to": "2.0",
      "mode": "Explicit"
    }
  }
}
```

规则：

- schema 版本随插件配置结构变化。
- 默认配置必须是包内只读资源。
- 用户配置写在用户数据目录。
- 迁移能力必须绑定插件生命周期。

## 5. 更新迁移流程

```text
Install new plugin version
-> Read old settings schema
-> Read new settings schema
-> Determine migration requirement
-> Backup current configuration/state
-> Run migration if required
-> Validate migrated configuration
-> Activate new version
```

规则：

- 迁移在新版本启用前完成。
- 迁移失败不能破坏旧版本配置。
- 新版本启用失败时应保留回滚所需旧状态。
- 缓存数据可以删除重建，不应阻止回滚。

## 6. 回滚策略

第一版回滚策略：

- 可以回滚 active plugin version。
- 可以恢复迁移前备份配置。
- 不承诺自动把新 schema 降级到旧 schema。
- 如果旧版本无法读取迁移后的配置，必须使用备份。
- 如果没有备份，插件进入 Disabled 并提示用户处理。

## 7. 插件卸载

卸载插件时：

- 默认删除 active 记录和安装目录。
- 默认保留配置、状态和用户数据。
- 用户显式选择清理数据时，才删除配置和状态。
- secrets 必须交给 Security 或平台安全存储删除。

## 8. 测试要求

必须覆盖：

- 配置隔离。
- schema 版本升级。
- 迁移成功。
- 迁移失败回滚。
- 新版本启用失败恢复旧配置。
- 卸载保留用户数据。
- 显式清理用户数据。
- 插件不能写 Host 全局配置。
