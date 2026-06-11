# Build 应用发布设计

版本：v0.1
状态：正式初版
适用范围：应用 publish、静态插件、bundled plugin、资源包、Native AOT、发布 manifest 和发布诊断

## 1. 目标

Build 负责定义 AtomUI.City 应用发布输出。发布输出必须同时服务普通 CoreCLR 桌面部署、动态插件部署、静态插件部署和 Native AOT 部署。

## 2. 发布模式

| 模式 | 说明 |
|---|---|
| CoreCLR dynamic plugins | 应用支持运行时下载、安装、加载插件。 |
| CoreCLR bundled plugins | 应用随包携带内置插件。 |
| Native AOT static plugins | 插件编译进应用，运行时启用/停用 Contribution。 |
| Resource pack mode | 只动态加载资源、语言包、`.locpack`。 |

## 3. 发布流程

```text
Build application
-> Generate application manifest
-> Validate module manifests
-> Collect static plugins
-> Collect bundled plugins
-> Collect resources
-> Validate AOT compatibility
-> Publish app
-> Write publish layout
-> Write diagnostics
```

## 4. 输出布局

```text
output/publish/apps/<AppId>/<Configuration>/<TargetFramework>/
  app/
  atomui-city/
    application.manifest.json
    manifests/
    resources/
    bundled-plugins/
    static-plugins/
    diagnostics/
```

规则：

- `app/` 保存应用发布输出。
- `atomui-city/manifests` 保存框架 manifest。
- `bundled-plugins` 保存随应用发布的内置插件包或展开产物。
- `static-plugins` 保存静态插件 manifest。
- 发布目录不保存用户插件配置和状态。

## 5. Native AOT

Native AOT 发布规则：

- 默认不支持运行时动态加载插件程序集。
- 动态插件配置必须报错或明确降级。
- 静态插件必须在构建期进入 manifest。
- source generator 生成静态 registrar。
- 资源包仍可动态加载，但不能包含托管代码。

## 6. Application Manifest

应用 manifest 建议包含：

- AppId。
- framework version。
- plugin API version。
- enabled static modules。
- static plugin list。
- bundled plugin list。
- resource pack list。
- AOT mode。
- manifest hashes。

## 7. 测试矩阵

| 功能点 | 测试类型 | 必测场景 |
|---|---|---|
| publish layout | Build test | app、manifests、resources 路径正确。 |
| application manifest | Build test | AppId、plugin API、hash。 |
| bundled plugin | Build test | bundled plugin 进入发布输出。 |
| static plugin | Build test | static manifest 生成。 |
| Native AOT | Build test | dynamic plugin 被拒绝。 |
| resource pack | Build test | `.locpack` 进入 resources。 |
