# AtomUI.City 生命周期

版本：v0.1
状态：初版草案
适用范围：Application、Module、Route、ViewModel、State、EventBus、Command 生命周期设计

## 1. 目标

生命周期是 AtomUI.City 的核心设计重心。框架必须明确管理创建、初始化、激活、停用、释放、错误处理和取消流程，避免订阅泄漏、重复初始化和无边界状态变更。

## 2. 生命周期层级

| 层级 | 生命周期关注点 |
|---|---|
| Application | 创建、配置、初始化、启动、挂起、恢复、关闭。 |
| Module | 发现、配置、初始化、启用、停止。 |
| Route | 匹配、守卫、解析、进入、离开、释放。 |
| ViewModel | 创建、激活、停用、释放。 |
| StateScope | 创建、订阅、Reaction、快照、清理。 |
| EventBus Subscription | 订阅、调度、错误处理、释放。 |
| Command | 可执行、执行中、完成、失败、取消。 |

## 3. 模块生命周期

模块生命周期：

```text
Discover -> PreConfigure -> ConfigureServices -> Configure -> Initialize -> Started -> Stopping -> Stopped
```

模块系统需要保证：

- 模块依赖顺序稳定。
- 重复模块可诊断。
- 缺失依赖可诊断。
- 初始化失败可定位。
- 模块资源注册可追踪。

## 4. Activation Scope

Activation Scope 的职责：

- 收集可释放资源。
- 关联取消令牌。
- 管理 ViewModel 激活期间的订阅。
- 在 ViewModel 停用时统一释放。
- 避免重复订阅和内存泄漏。

ViewModel 的长期订阅必须进入 Activation Scope，不应直接挂在构造函数中。

## 5. 错误和取消

生命周期相关错误需要进入统一错误处理管线。

取消语义需要覆盖：

- 应用关闭。
- 模块停止。
- 路由离开。
- ViewModel 停用。
- Command 取消。
- Data 请求取消。
- State Reaction 释放。
