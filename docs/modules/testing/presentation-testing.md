# Presentation 测试设计

版本：v0.1
状态：正式初版
适用范围：Presentation fake runtime、ViewLocator、ViewFactory、Outlet commit、UI Dispatcher、visual lifecycle 和 AtomUI/Avalonia 平台集成

## 1. 目标

Presentation 测试分为 fake runtime 测试和真实平台集成测试。默认测试不启动真实 AtomUI/Avalonia UI。

## 2. PresentationTestRuntime

Fake runtime 提供：

- fake ViewLocator。
- fake ViewFactory。
- fake route outlet。
- fake visual tree。
- fake UI dispatcher。
- fake interaction handler。
- fake resource dictionary。
- visual lifecycle feedback driver。

## 3. 单元测试范围

必须覆盖：

- ViewModel 到 View 映射。
- ViewLocator success/failure。
- ViewFactory create/dispose。
- Outlet commit。
- Commit failure rollback。
- UI dispatcher 投递。
- Visual attached/detached。
- Loaded/unloaded feedback。
- Resource lease revoke。
- 插件 View/Resource 撤销。

## 4. MVVM 集成

必须覆盖：

- ViewModel 创建后未 commit 不 active。
- commit 成功后 activation。
- visual detached 后 deactivation。
- Interaction handler 绑定和释放。
- Binding error diagnostics。

## 5. 平台集成测试

真实 AtomUI/Avalonia 测试只覆盖 fake runtime 无法证明的行为：

- 真实 UI Dispatcher。
- 真实 ViewLocator。
- 真实 binding。
- 真实 Outlet commit 到 visual tree。
- ResourceDictionary 刷新。
- Window lifecycle。
- visual attach/detach。

平台集成测试必须独立分类，不作为普通单元测试替代。

## 6. 测试要求

Presentation 的每个 public contract 和每个运行时桥接点必须有单元测试。真实 UI 行为使用平台集成测试补充。
