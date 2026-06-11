# AtomUI.City.Localization Source Generation 设计

版本：v0.1
状态：正式初版
适用范围：Resource manifest、language package descriptor、强类型 key、accessor、AOT、Analyzer 和构建期诊断。

## 1. 定位

Localization 是 source-generator-first 模块。

运行时默认不扫描程序集找资源，也不靠反射发现资源 key。Source Generator 生成 manifest、descriptor 和强类型访问入口。

## 2. 生成内容

Generator 负责：

- Resource manifest。
- Language package descriptor。
- Supported culture manifest。
- Fallback culture manifest。
- Strongly typed accessor。
- Key constants。
- Module resource descriptor。
- Plugin resource descriptor。
- AtomUI resource bridge descriptor。
- Locpack manifest。

## 3. 强类型 Accessor

强类型 accessor 生成在模块或插件主 assembly 中，不生成在语言包 assembly 中。

原因：

- 语言包 assembly 应保持 resource-only。
- 插件语言包卸载不能影响主插件 API。
- Host 不应持有语言包 assembly 类型。

## 4. Analyzer 诊断

必须诊断：

- 重复 key。
- 未声明 key 引用。
- fallback 不完整。
- invariant 缺失。
- culture package 缺失。
- 格式化参数数量不匹配。
- 插件资源覆盖 Host key。
- 插件资源类型泄漏。
- 运行时反射式资源扫描。

## 5. AOT

Native AOT 模式：

- 禁止依赖动态 assembly loading。
- 使用 file-based locpack。
- manifest 和 accessor 仍由 generator 生成。
- 运行时消费强类型 descriptor。

## 6. Build 集成

Build 模块后续负责：

- 生成 language package assembly。
- 生成 locpack。
- 输出 manifest。
- 校验资源完整性。
- 复制 culture package 到 output。

Localization 文档只定义 contract。

## 7. 测试策略

测试必须覆盖：

- manifest 生成。
- accessor 生成。
- duplicate key 诊断。
- missing key 诊断。
- fallback incomplete 诊断。
- locpack manifest。
- plugin resource leakage 诊断。
