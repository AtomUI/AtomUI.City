# AtomUI.City.Localization

版本：v0.1
状态：初版草案

## 职责

`AtomUI.City.Localization` 负责本地化资源、文化切换、文本刷新和模块化资源注册。

Localization 需要支持模块独立贡献资源，并让 UI 层在文化变化后能够刷新文本。

## 边界

Localization 负责：

- 资源注册。
- 资源查找。
- 当前文化状态。
- 文化切换。
- 文本刷新通知。
- 模块资源隔离。

Localization 不负责：

- 业务翻译内容。
- 在线翻译服务。
- UI 控件渲染。

## 后续拆分

- `resources.md`
- `culture-switching.md`
- `module-resources.md`
