# PluginSystem 加载设计

版本：v0.1
状态：正式初版
适用范围：插件验证后加载、加载上下文创建、模块图、服务 Scope 和启用前准备

## 1. 目标

插件加载负责把已验证的插件版本放入运行时，但加载不等于启用。只有 Contribution 成功应用后，插件能力才对应用可见。

设计目标：

- 加载过程可取消、可诊断。
- 插件服务不污染 Host Root ServiceProvider。
- 插件加载上下文可卸载。
- 插件模块图在插件边界内构建。
- 加载失败可以释放已创建资源。

## 2. 前置条件

进入加载前必须完成：

- 插件发现。
- 清单 schema 校验。
- 包安装校验。
- 兼容性校验。
- 依赖解析。
- 能力授权初步评估。
- 锁定文件 active 版本解析。

未通过前置条件的插件不能进入加载。

## 3. 加载流程

```text
Create plugin lifecycle context
-> Create plugin load context
-> Resolve Host contracts
-> Resolve private dependencies
-> Load main plugin assembly
-> Load generated module manifest
-> Build plugin module graph
-> Create plugin service collection
-> Run plugin module service registration
-> Build plugin ServiceProvider
-> Initialize plugin modules
-> Mark Loaded or Initialized
```

规则：

- 加载阶段可以创建插件服务 Scope。
- 加载阶段不能把 Contribution 应用到 Host registry。
- 加载阶段不能允许新路由、命令或事件入口进入插件。
- 加载失败必须释放已创建的加载上下文和服务容器。

## 4. 服务注册

插件服务注册进入插件自己的服务集合。

规则：

- 插件不能修改 Host Root ServiceProvider。
- 插件可以解析 Host 显式暴露的 contract。
- Host 可以通过受控代理调用插件服务。
- 插件服务实例不能被 Host 静态持有。
- 插件服务容器释放后，Host 不得再访问插件服务实例。

## 5. 模块图

插件可以包含多个模块。

规则：

- 插件模块继承普通模块抽象。
- 插件模块依赖通过注解和 source generator 生成模块图。
- 插件模块图只在当前插件内有效。
- 插件模块可以依赖 Host 共享模块 contract，但不能修改 Host 静态模块图。
- 插件模块初始化失败时，当前插件加载失败。

## 6. 加载中间件

PluginLoad pipeline 可以包含：

- 来源策略检查。
- 诊断上下文增强。
- 性能计时。
- 依赖解析审计。
- AOT/trim 兼容检查。
- 应用自定义拦截。

中间件不能绕过核心加载前置条件。

## 7. 加载失败

加载失败处理：

```text
Stop loading
-> Dispose plugin ServiceProvider if created
-> Release module initialization resources
-> Request load context unload if created
-> Mark Disabled or Faulted
-> Record diagnostics
```

加载失败默认不影响主应用启动，除非插件被 Host 标记为必需插件。

## 8. 线程约束

插件加载应遵守 Core Threading：

- 不阻塞 UI Thread。
- 加载操作必须绑定 CancellationToken。
- 插件初始化不得启动非受控线程。
- 后台任务必须通过 Host 管理入口创建。
- 加载取消后不能继续应用 Contribution。

## 9. 测试要求

必须覆盖：

- 加载成功状态流转。
- 主程序集缺失。
- 私有依赖缺失。
- Host contract 版本不匹配。
- 插件模块图构建失败。
- 插件服务注册失败。
- 加载取消。
- 加载失败后服务容器释放。
- 加载失败后加载上下文可释放。
