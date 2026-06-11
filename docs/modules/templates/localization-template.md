# 本地化模板设计

版本：v0.1
状态：正式初版
适用范围：语言资源目录、资源 key、懒加载语言包、assembly 语言包、`.locpack` 和本地化测试

## 1. 目标

本地化模板用于生成符合 Localization 模块约定的资源结构。

设计目标：

- 支持按当前 culture 懒加载语言包。
- 支持插件资源撤销。
- 支持 assembly 语言包和 `.locpack`。
- 默认生成资源测试。

## 2. 默认结构

```text
Localization/
  en-US/
    Resources.resx
  zh-CN/
    Resources.resx
```

插件模板可生成：

```text
atomui-city/locales/
  en-US/
  zh-CN/
```

## 3. Resource Key

规则：

- 展示文本使用 resource key。
- 插件 display name 和 description 使用 resource key。
- 路由 title 使用 resource key。
- validation error 使用 resource key。
- 不在模板中硬编码多语言展示文本到业务代码。

## 4. 懒加载

模板必须生成可被 Localization manifest 识别的结构。

规则：

- 启动期只加载 manifest。
- 当前 culture 的语言包按需加载。
- culture 切换时触发资源刷新。
- 插件卸载时资源可撤销。

## 5. AOT

Native AOT 模式下：

- 支持 `.locpack`。
- 不默认依赖动态 assembly loading。
- 生成的资源索引必须 AOT 友好。

## 6. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| 资源目录 | Unit/Build | culture 目录生成。 |
| resource key | Unit | key 存在，不硬编码展示文本。 |
| manifest | Build | localization manifest 生成。 |
| lazy load | Unit | 当前 culture 包加载。 |
| fallback | Unit | 缺失 key fallback。 |
| plugin unload | Plugin test | 资源撤销。 |
