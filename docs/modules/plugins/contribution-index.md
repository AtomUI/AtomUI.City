# PluginSystem 贡献索引设计

版本：v0.1
状态：正式初版
适用范围：插件贡献清单索引、贡献清单文件、必填策略和构建期生成

## 1. 目标

贡献索引用于把插件清单和各模块贡献清单连接起来。它让 Host 在不执行插件代码的情况下知道插件准备贡献哪些能力。

设计目标：

- 贡献清单按模块拆分。
- Host 可以加载前校验必填贡献是否存在。
- 各模块只解析自己负责的贡献清单。
- Contribution 可以追踪到 Plugin、Module 和源文件。
- 支持 source generator 和 MSBuild 稳定生成。

## 2. 清单索引

`plugin.json` 中的 `contributions` 字段只保存索引：

```json
{
  "contributions": {
    "modules": {
      "path": "manifests/modules.json",
      "required": true
    },
    "routes": {
      "path": "manifests/routes.json",
      "required": false
    },
    "localization": {
      "path": "manifests/localization.json",
      "required": false
    }
  }
}
```

规则：

- `path` 必须是插件 `atomui-city` 目录内相对路径。
- `required` 为 true 且文件缺失时，插件验证失败。
- 未声明的贡献类型默认不存在。
- Host 不应为了发现贡献而执行插件代码。

## 3. 推荐贡献清单

| 清单 | 所属模块 | 内容 |
|---|---|---|
| `modules.json` | Core ModuleSystem | 插件模块和模块依赖。 |
| `routes.json` | Routing | 路由、导航元数据、ViewModel target。 |
| `permissions.json` | Security | 权限点、策略元数据。 |
| `presentation.json` | Presentation | View 映射、资源、菜单、工具栏入口。 |
| `commands.json` | Mvvm | 命令入口和动作元数据。 |
| `eventbus.json` | EventBus | handler、可发布事件、可订阅事件。 |
| `data.json` | Data | HTTP/gRPC/SignalR client 贡献。 |
| `localization.json` | Localization | 语言包、资源索引、fallback 信息。 |
| `settings.json` | Core Configuration | 设置 section、schema、设置页面入口。 |
| `diagnostics.json` | Diagnostics | 诊断 provider 元数据。 |

## 4. ContributionId

每个 Contribution 必须有稳定 Id。

推荐格式：

```text
<plugin-id>:<module-id>:<contribution-type>:<local-id>
```

规则：

- ContributionId 必须在插件内唯一。
- Host registry 可以要求在全局范围唯一。
- ContributionId 进入 lease、诊断和回滚记录。
- Source generator 应确保稳定生成。

## 5. 解析责任

PluginSystem 只负责：

- 读取索引。
- 校验文件存在。
- 记录 hash。
- 把清单交给对应模块。

具体语义由各模块解释：

- Routing 解释路由。
- Security 解释权限。
- Presentation 解释 View 和 UI 资源。
- Localization 解释语言包。
- Data 解释 client 和 connection。

PluginSystem 不解释业务语义。

## 6. 必填和可选

规则：

- `modules.json` 对包含模块的插件通常为必填。
- `routes.json`、`presentation.json` 等按插件能力决定是否必填。
- 可选清单缺失不应导致插件验证失败。
- 清单存在但内容无效时，由所属模块决定是否使插件启用失败。

## 7. Hash 和审计

每个贡献清单应记录：

- 路径。
- hash。
- 生成工具版本。
- 生成时间可作为非核心诊断信息。
- 所属插件和版本。

hash 写入安装记录或锁定文件，用于判断安装内容是否被篡改。

## 8. 测试要求

必须覆盖：

- required 清单缺失。
- optional 清单缺失。
- 清单路径越界。
- ContributionId 重复。
- 清单 hash 变化。
- 各模块只解析自己的清单。
- PluginSystem 不执行插件代码即可完成索引读取。
