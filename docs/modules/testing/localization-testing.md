# Localization 测试设计

版本：v0.1
状态：正式初版
适用范围：culture state、资源查找、fallback、懒加载、语言包 assembly、locpack、UI refresh 和插件本地化

## 1. 目标

Localization 测试必须证明语言切换、资源查找、懒加载和 UI refresh 可预测、可撤销、可诊断。

## 2. LocalizationTestKit

Testing 提供：

- fake culture state provider。
- fake language package provider。
- fake assembly package provider。
- fake locpack provider。
- resource lookup recorder。
- fallback assertion。
- culture switch driver。
- UI refresh fake bridge。
- resource leak assertion helper。

## 3. 单元测试范围

必须覆盖：

- current culture 读取。
- culture switch。
- resource lookup。
- missing key。
- fallback chain。
- culture package lazy load。
- language package assembly load。
- locpack load。
- resource unload。
- diagnostics。

## 4. UI refresh 测试

Fake bridge 覆盖：

- culture change 通知。
- ResourceDictionary 更新请求。
- ViewModel 错误文本刷新。
- command text refresh。
- route metadata refresh。

真实 ResourceDictionary 行为放平台集成测试。

## 5. 插件本地化测试

必须覆盖：

- 插件语言包注册。
- 插件语言包懒加载。
- 插件停用后资源撤销。
- 插件卸载后 language package unload。
- 插件缺失资源 fallback。

## 6. 测试要求

Localization Core 测试不依赖真实 AtomUI/Avalonia。
