# PluginSystem 诊断和测试设计

版本：v0.1
状态：正式初版
适用范围：插件诊断事件、错误码、测试工具、状态机测试和卸载验证

## 1. 目标

插件系统必须能解释插件为什么没有安装、为什么不能加载、为什么不能启用、为什么不能卸载。

设计目标：

- 每个阶段有稳定诊断事件。
- 常见失败有稳定错误码。
- 诊断带 PluginId、Version、operation id 和阶段。
- 测试工具可以驱动插件生命周期。
- 卸载测试能发现引用泄漏。

## 2. 诊断阶段

| 阶段 | 说明 |
|---|---|
| Discover | 扫描插件位置和读取锁定文件。 |
| Install | 下载、hash、解压、staging、安装。 |
| Verify | 清单、兼容性、能力、签名、依赖。 |
| Load | 加载上下文、程序集、模块图、服务容器。 |
| Activate | Contribution 校验和 Lease 创建。 |
| Deactivate | 停止入口、取消 Operation、撤销 Lease。 |
| Unload | 释放资源、卸载加载上下文。 |
| Update | 版本切换、pending、回滚。 |

## 3. 诊断上下文

每条诊断至少包含：

- PluginId。
- PackageId。
- Version。
- PluginProfile。
- operation id。
- phase。
- source path。
- install path。
- contribution id，如果适用。
- exception，如果适用。
- policy result，如果适用。

## 4. 错误码

建议错误码：

| Code | 含义 |
|---|---|
| `AUCPLG0001` | 缺少 PluginId。 |
| `AUCPLG0002` | 多主程序集。 |
| `AUCPLG0003` | PluginId 与安装记录不一致。 |
| `AUCPLG0101` | Host 版本不兼容。 |
| `AUCPLG0102` | 插件 API 版本不兼容。 |
| `AUCPLG0201` | 能力被拒绝。 |
| `AUCPLG0202` | Contribution 超出授权能力。 |
| `AUCPLG0301` | required contribution manifest 缺失。 |
| `AUCPLG0401` | 插件私有类型泄漏到 Host contract。 |
| `AUCPLG0501` | 依赖插件缺失。 |
| `AUCPLG0502` | 依赖版本范围不满足。 |
| `AUCPLG0601` | 包 hash 不匹配。 |
| `AUCPLG0602` | 签名无效。 |
| `AUCPLG0701` | 插件加载失败。 |
| `AUCPLG0801` | 插件卸载进入 UnloadPending。 |
| `AUCPLG0901` | 更新失败并回滚。 |

## 5. 测试工具

Testing 包应提供：

- Plugin test host。
- Fake plugin package builder。
- Fake plugin source。
- Plugin lifecycle driver。
- Fake Host contract registry。
- Fake Contribution registry。
- Load context unload assertion helper。
- Pending update simulator。
- Capability policy test helper。

测试工具不应要求真实 NuGet feed。

## 6. 状态机测试

必须覆盖状态：

```text
Discovered
Verified
Loaded
Initialized
ContributionsApplied
Active
Deactivating
Inactive
Unloading
Unloaded
Invalid
Disabled
Faulted
UnloadPending
```

状态机测试必须断言非法状态转换被拒绝。

## 7. 卸载验证

卸载测试必须验证：

- Lease 全部撤销。
- Operation 全部取消。
- EventBus subscription 全部释放。
- UI 引用全部释放。
- ServiceProvider 已释放。
- AssemblyLoadContext 可以被 GC。
- `UnloadPending` 有明确残留诊断。

## 8. 安装和更新测试

必须覆盖：

- staging 安装成功。
- staging 安装失败清理。
- package cache 损坏。
- hash 不匹配。
- 更新成功。
- 更新进入 pending。
- 回滚成功。
- 回滚失败进入 Disabled。

## 9. 文档完成标准

PluginSystem 的任何行为变更必须同步更新：

- 对应模块设计文档。
- 诊断代码表。
- checklist。
- 测试场景说明。

没有对应文档的行为不应进入实现。
