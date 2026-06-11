# PluginSystem AOT 和静态插件设计

版本：v0.1
状态：正式初版
适用范围：Native AOT、trimming、source generator、静态插件、资源包和运行时动态插件限制

## 1. 目标

AtomUI.City 必须把 AOT 友好作为框架级约束。插件系统尤其需要明确：运行时动态程序集加载和 Native AOT 天然冲突。

设计目标：

- CoreCLR 动态插件和 Native AOT 静态插件边界清楚。
- 运行时反射扫描不是默认机制。
- source generator 优先生成索引。
- 资源包可以在 AOT 场景下继续懒加载。
- 插件开发期能提前发现 AOT 不兼容能力。

## 2. 发布模式

| 模式 | 插件策略 |
|---|---|
| CoreCLR | 支持运行时安装、加载、卸载动态插件。 |
| Native AOT | 不支持默认运行时动态加载插件程序集。 |
| Native AOT + static plugins | 插件编译进 Host，运行时按清单启用或禁用。 |
| Native AOT + resource packs | 只动态加载本地化、图片、样式等资源包。 |
| Native AOT + external process | 通过进程外插件和 IPC 扩展。 |

## 3. 静态插件

静态插件指插件程序集在应用构建时已经被 Host 引用并编译进发布产物。

规则：

- 静态插件仍使用 PluginId、manifest、capability 和 Contribution Lease。
- 静态插件不能依赖 AssemblyLoadContext 卸载。
- 静态插件可以启用、停用和撤销 Contribution。
- 静态插件更新需要应用重新发布。

静态插件保留统一编程模型，但失去运行时程序集卸载能力。

## 4. 资源包

资源包可在 AOT 场景下保持动态加载。

资源包包括：

- `.locpack`。
- 图片。
- 样式。
- 非代码配置。
- 预生成 manifest。

规则：

- 资源包不能包含需要执行的托管代码。
- 资源包必须有 hash 和来源校验。
- 资源包可以按 culture 懒加载。
- 资源包卸载必须释放 UI 和 Localization 引用。

## 5. Source Generator 策略

插件系统优先使用 source generator 生成：

- 插件模块索引。
- 依赖图。
- 路由索引。
- View/ViewModel 映射。
- 权限索引。
- Data client 代理索引。
- 本地化 key 索引。
- AOT 兼容性报告。

运行时不应通过扫描所有类型发现插件能力。

## 6. Trimming 约束

插件代码需要避免依赖未声明反射。

规则：

- 需要反射访问的类型必须由 source generator 生成显式访问路径。
- 动态代理优先改为生成代码。
- Options binding 优先使用 source-generated binding。
- 序列化优先使用 source-generated context。
- 框架公共 API 必须尽量 trimming-safe。

## 7. 诊断

构建期应报告：

- 动态插件能力用于 Native AOT 发布。
- 插件使用未声明反射。
- 插件清单缺少 AOT 兼容声明。
- 插件依赖不支持 trimming。
- source generator 未生成必要索引。

运行期应报告：

- 当前发布模式不支持动态插件。
- 静态插件无法卸载程序集。
- 资源包加载失败。

## 8. 测试要求

必须覆盖：

- CoreCLR 动态插件加载。
- Native AOT 模式拒绝动态插件。
- 静态插件启用和停用。
- 静态插件撤销 Contribution。
- 资源包懒加载和释放。
- trimming 分析警告。
- source generator 缺失索引时构建失败。
