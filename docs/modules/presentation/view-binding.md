# AtomUI.City.Presentation View 绑定设计

版本：v0.1
状态：正式初版
适用范围：View 创建、DataContext、binding handle、View/ViewModel 生命周期和释放

## 1. 定位

View binding 把 ViewModel instance 安全绑定到 View，并把 View 侧资源挂入 ActivationScope。

```text
ViewModel instance
-> IViewLocator locate ViewDescriptor
-> IViewFactory create View on UI Thread
-> IViewBinder set DataContext
-> attach lifecycle adapter
-> register disposables into ActivationScope
-> return BoundViewHandle
```

## 2. ViewFactory

规则：

- View 创建必须在 UI Thread。
- View 可以从 Application 或 Plugin service context 创建。
- View 构造函数不应启动长期任务。
- View 不能持有插件服务到 Host 静态对象。
- 创建失败返回 Presentation commit failure。

Strict AOT 模式下，ViewFactory 应由 Source Generator 生成强类型工厂，避免反射构造。

## 3. Binding 规则

- ViewModel 不知道 View 类型。
- View 不负责导航决策。
- Binding 必须可释放。
- ViewDataContext 变化必须受控，不能被外部任意覆盖。
- View 和 ViewModel 生命周期不完全等同，但必须有关联释放策略。
- UI 事件订阅、binding disposable 和 visual adapter 默认挂 ActivationScope。

## 4. 失败处理

Presentation 应提供诊断：

- 找不到 View。
- 找到多个默认 View。
- View 创建失败。
- Binding 失败。
- 插件 View descriptor 已撤销。

Binding 失败时，Presentation 必须释放已创建 View 和 provisional ActivationScope，并让 Routing 保持旧 Outlet 内容。

## 5. 测试矩阵

| 功能点 | 测试类型 | 断言 |
|---|---|---|
| View 创建 | Unit | ViewFactory 在 fake UI dispatcher 上创建 View。 |
| DataContext 设置 | Unit | View 绑定到 ViewModel。 |
| Binding 释放 | Unit | ActivationScope 停止时释放 binding。 |
| View 创建失败 | Unit | commit failure，旧内容保留。 |
| 插件 View 泄漏 | Analyzer/Generator | 输出稳定诊断。 |
