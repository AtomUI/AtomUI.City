# AtomUI.City.Presentation Activation 集成设计

版本：v0.1
状态：正式初版
适用范围：Visual lifecycle、ActivationScope、attached/detached、close intent 和 ViewModel 激活边界

## 1. 定位

Mvvm Activation 是 ViewModel 生命周期。Presentation 负责把 visual lifecycle 接入 Activation。

Visual lifecycle 和 ViewModel active 状态不能混为一谈。

| 概念 | 来源 | 含义 |
|---|---|---|
| ActivationScope | Routing / Mvvm | ViewModel 逻辑上进入当前路由或激活上下文。 |
| VisualAttachmentState | Presentation / AtomUI/Avalonia | View 当前是否挂在 visual tree 或处于可见生命周期。 |

## 2. 导航提交阶段

```text
Prepare:
Create provisional RouteScope / ActivationScope
Create ViewModel
Resolve ViewDescriptor
Create View
Bind View and ViewModel

Commit:
Apply Outlet commit plan
Attach / replace / detach views
Update AtomUI/Avalonia VisualTree

Activate:
Mark ActivationScope running
Activate ViewModel
Attach visual lifecycle adapter
Update NavigationSnapshot / Journal
```

ActivationScope 在 binding 前可用，但只有 Outlet commit 成功后，ViewModel 才进入 active 状态。

## 3. VisualTree 反馈

VisualTree 变化必须通过 Presentation 归一化后反馈。Routing、Mvvm、Core 不直接订阅 AtomUI/Avalonia 原始 visual tree 事件。

反馈分为：

- Outlet commit 反馈。
- Visual lifecycle 反馈。
- Leave / close intent。
- Diagnostics。

## 4. Close Intent

关闭类事件必须表达为意图，而不是直接释放对象。

```text
Window Closing / View close gesture
-> Presentation captures close intent
-> Routing LeaveGuard / Mvvm Interaction confirmation
-> if allowed: deactivate route or window
-> Presentation detaches VisualTree
```

## 5. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| commit 后激活 | Unit | commit 成功后 ActivationScope running。 |
| commit 失败 | Unit | provisional Scope 被释放。 |
| attached feedback | Unit | visual state 反馈到 activation adapter。 |
| detached feedback | Unit | visual state 更新但不直接停用 ViewModel。 |
| close intent | Unit | 转为 leave request 或 interaction。 |
