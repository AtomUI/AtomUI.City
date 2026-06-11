# Build Source Generation 集成设计

版本：v0.1
状态：正式初版
适用范围：Source Generator 接入、生成产物收敛、增量生成、输出路径、AOT 约束和测试

## 1. 目标

Build 负责把 `AtomUI.City.Generators` 接入应用和插件项目，并把生成产物收敛为可诊断、可测试、可打包的文件。

Source Generator 的框架级原则见：[Source Generator 设计规范](../../architecture/source-generation.md)。

## 2. 接入方式

应用开发者引用：

```text
AtomUI.City.Build
```

Build 通过 analyzer asset 引入：

```text
AtomUI.City.Generators
```

规则：

- 应用不需要直接引用多个 generator 包。
- 运行时包不依赖 Roslyn。
- generator/analyzer 只在编译期运行。

## 3. Generator 分类

第一版接入：

- Modularity generator。
- Routing generator。
- Presentation generator。
- Security generator。
- EventBus generator。
- Localization generator。
- Data generator。
- Plugin static generator。
- Diagnostics analyzer。

## 4. 输出产物

C# registrar：

```text
obj/.../generated/
  AtomUI.City.Generated.Modularity.g.cs
  AtomUI.City.Generated.Routing.g.cs
  AtomUI.City.Generated.Presentation.g.cs
```

Manifest：

```text
obj/AtomUI.City/manifests/
  modules.json
  routes.json
  presentation.json
  permissions.json
  events.json
  localization.json
  data.json
  plugins.json
```

Build 将最终快照复制到：

```text
output/artifacts/generated/
output/artifacts/manifests/
```

## 5. 增量生成

要求：

- 只使用 incremental generator。
- 输入未变化时输出不变化。
- 输出 hint name 稳定。
- 不读取不稳定环境信息。
- 不执行用户代码。
- 不访问网络。

## 6. Strict 模式

`AtomUICitySourceGenerationMode`：

| 模式 | 行为 |
|---|---|
| `Strict` | AOT-first，禁止默认动态发现，问题尽量报错。 |
| `Compatible` | 允许部分 opt-in 动态能力，输出 warning。 |
| `Off` | 关闭框架生成器，仅用于特殊调试。 |

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| generator 接入 | Build test | 引用 Build 后 generator 自动运行。 |
| generated path | Build test | 产物进入 expected obj/output 路径。 |
| incremental | Generator test | 输入不变不重新生成。 |
| strict mode | Analyzer/Build | 动态发现报错。 |
| manifest output | Generator/Build | JSON manifest 生成稳定。 |
