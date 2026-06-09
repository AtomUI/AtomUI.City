# AtomUI.City.Templates

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Templates` 负责应用模板、模块模板、页面模板、插件模板和测试模板。

模板需要体现 AtomUI.City 的默认编程范式，而不是只创建空项目文件。

## 边界

Templates 负责：

- 应用模板。
- 模块模板。
- 页面模板。
- 插件模板。
- 测试模板。
- 模板变量。
- 模板输出结构。

Templates 不负责：

- CLI 参数解析。
- 构建任务实现。
- 模板发布流程。

## 后续拆分

- `app-template.md`
- `module-template.md`
- `page-template.md`
- `plugin-template.md`
- `test-template.md`
