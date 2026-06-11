# Build Manifest 生成设计

版本：v0.1
状态：正式初版
适用范围：模块、路由、权限、Presentation、Data、Localization、Plugin 和应用 manifest 的生成、校验和输出

## 1. 目标

Manifest 是 AtomUI.City 在构建期沉淀框架元数据的核心产物。Build 负责汇总 source generator、MSBuild items 和资源索引，生成稳定 manifest。

## 2. Manifest 类型

| Manifest | 文件 | 来源 |
|---|---|---|
| Module | `modules.json` | Modularity generator。 |
| Routing | `routes.json` | Routing generator。 |
| Permission | `permissions.json` | Security generator。 |
| Presentation | `presentation.json` | Presentation generator。 |
| Data | `data.json` | Data generator。 |
| Localization | `localization.json` | Localization generator。 |
| EventBus | `events.json` | EventBus generator。 |
| Plugin | `plugin.json` | MSBuild task 汇总。 |
| Contribution index | `plugin.manifest.json` | MSBuild task 汇总。 |
| Application | `application.manifest.json` | Build task 汇总。 |

## 3. 生成流程

```text
Source generators emit intermediate manifests
-> MSBuild task collects intermediate manifests
-> Normalize paths and ids
-> Validate schema
-> Sort deterministic
-> Compute hashes
-> Write final manifests
-> Copy manifest snapshots to output/artifacts/manifests
```

## 4. 中间产物

中间 manifest 可以位于：

```text
obj/AtomUI.City/manifests/
```

最终快照进入：

```text
output/artifacts/manifests/
```

规则：

- 中间 manifest 服务 Build task。
- 最终 manifest 服务打包、诊断、测试和 CLI。
- 插件包内 manifest 必须来自最终校验后的产物。

## 5. 稳定性规则

Manifest 必须 deterministic：

- 字段顺序稳定。
- 数组顺序稳定。
- 路径分隔符稳定。
- 不包含绝对临时路径。
- 不包含构建时间戳作为核心 hash 输入。
- hash 计算规则稳定。

## 6. 校验规则

必须校验：

- schema version。
- required fields。
- duplicate ids。
- route conflict。
- permission conflict。
- View/ViewModel mapping conflict。
- Data client duplicate。
- localization culture format。
- plugin capability 超出声明。
- contribution manifest 缺失。

校验失败时构建失败，除非对应规则明确允许 warning。

## 7. AOT 关系

Manifest 生成必须支持 AOT-first：

- 不通过运行时反射扫描生成 manifest。
- 不执行用户代码。
- 动态发现必须 opt-in，并产生 AOT 诊断。
- Native AOT 发布使用静态 plugin/application manifest。

## 8. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| manifest 收集 | Build test | 多 generator 产物汇总。 |
| schema 校验 | Unit/Build | 缺少必填字段失败。 |
| deterministic order | Unit | 输入顺序变化输出稳定。 |
| hash | Unit | 内容变化 hash 变化。 |
| plugin manifest | Build | `plugin.json` 和 contribution index 正确生成。 |
| output copy | Build | 快照进入 `output/artifacts/manifests`。 |
