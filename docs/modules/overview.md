# 模块文档

本目录维护 AtomUI.City 各框架包的模块级设计。每个模块目录至少包含 `overview.md`，后续按实现复杂度继续拆分细节文档。

## 模块开工规则

所有模块必须有完善的文档后，才能开始写代码。

模块文档至少需要说明：

- 职责和非职责。
- 依赖和禁止依赖。
- 生命周期接入点。
- 核心概念和公共抽象。
- 扩展点。
- AOT/trimming/source generator 策略。
- 错误处理策略。
- 测试策略。
- 与其他模块的集成关系。

复杂模块不能只保留 `overview.md`，必须继续拆分细节文档。

完整规则见：[文档先行治理规范](../engineering/documentation-governance.md)。

## 模块索引

| 模块 | 文档 |
|---|---|
| Core | [core/overview.md](core/overview.md) |
| Mvvm | [mvvm/overview.md](mvvm/overview.md) |
| State | [state/overview.md](state/overview.md) |
| Routing | [routing/overview.md](routing/overview.md) |
| Data | [data/overview.md](data/overview.md) |
| Security | [security/overview.md](security/overview.md) |
| EventBus | [eventbus/overview.md](eventbus/overview.md) |
| Localization | [localization/overview.md](localization/overview.md) |
| Presentation | [presentation/overview.md](presentation/overview.md) |
| PluginSystem | [plugins/overview.md](plugins/overview.md) |
| Build | [build/overview.md](build/overview.md) |
| Cli | [cli/overview.md](cli/overview.md) |
| Templates | [templates/overview.md](templates/overview.md) |
| Testing | [testing/overview.md](testing/overview.md) |

模块文档需要保持业务无关，只描述框架职责、边界、依赖、生命周期和扩展点。
