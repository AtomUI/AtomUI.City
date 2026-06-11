# AtomUI.City.Templates

版本：v0.1
状态：正式初版

## 职责

`AtomUI.City.Templates` 负责应用模板、模块模板、页面模板、插件模板和测试模板。

模板需要体现 AtomUI.City 的默认编程范式，而不是只创建空项目文件。

Templates 生成的工程必须符合 Core、Lifecycle、Routing、MVVM、Presentation、PluginSystem、Build 和 Testing 的约束。

## 边界

Templates 负责：

- 应用模板。
- 模块模板。
- 页面模板。
- 插件模板。
- 测试模板。
- 本地化模板。
- 配置模板。
- 模板变量。
- 模板输出结构。
- 模板 smoke test 规范。

Templates 不负责：

- CLI 参数解析。
- 构建任务实现。
- 模板发布流程。
- 插件运行时安装。
- 真实部署。
- 业务样例功能。

## 详细设计

| 文档 | 内容 |
|---|---|
| [detailed-design.md](detailed-design.md) | Templates 总体架构、职责边界、模板类型、生成结构、测试门禁和完成标准。 |
| [application-template.md](application-template.md) | 应用模板、Host 启动、模块入口、配置、本地化、测试项目和 Build 接入。 |
| [module-template.md](module-template.md) | 模块类、模块依赖、服务注册、配置、Contribution、source generator 输入和模块测试。 |
| [page-template.md](page-template.md) | 页面路由、ViewModel Target、ViewModel、View、Outlet、Activation 和页面测试。 |
| [plugin-template.md](plugin-template.md) | 插件项目、PluginId、主程序集、模块、manifest、资源、打包配置和插件测试。 |
| [test-template.md](test-template.md) | 测试项目、功能点测试矩阵、TestHost、单元测试、集成测试和模板默认测试入口。 |
| [localization-template.md](localization-template.md) | 语言资源目录、资源 key、懒加载语言包、assembly 语言包、`.locpack` 和本地化测试。 |
| [configuration-template.md](configuration-template.md) | Options、配置 section、PreConfigure、配置验证、reloadable 配置和配置测试。 |
| [template-variables.md](template-variables.md) | 模板变量、命名规则、命名空间、路径规则、默认值和参数校验。 |
| [diagnostics-and-testing.md](diagnostics-and-testing.md) | 模板诊断、变量校验、模板生成测试、构建测试、smoke test 和功能点测试矩阵。 |
