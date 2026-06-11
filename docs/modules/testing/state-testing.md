# State 测试设计

版本：v0.1
状态：正式初版
适用范围：应用状态、作用域状态、订阅、computed、snapshot、dispatcher、插件状态和线程安全

## 1. 目标

State 测试必须证明状态读写、通知、快照和释放在多线程桌面环境中可控。

## 2. StateTestStore

Testing 提供：

- fake application state。
- scoped state host。
- state change recorder。
- computed invalidation assertion。
- snapshot assertion。
- subscription leak assertion。
- dispatcher target assertion。

## 3. 单元测试范围

必须覆盖：

- state get/set。
- 相等值不通知。
- change version。
- subscription notification。
- subscription disposal。
- computed cache。
- computed invalidation。
- snapshot save。
- snapshot restore。
- invalid mutation。
- diagnostics。

## 4. 线程测试

必须覆盖：

- background mutation。
- UI dispatch notification。
- concurrent mutation policy。
- late notification suppression。
- Scope stop 后 notification 不执行。

## 5. 插件状态测试

必须覆盖：

- 插件状态隔离。
- 插件停用取消 subscription。
- 插件卸载释放 state reaction。
- 插件状态 snapshot。
- 插件配置和状态不写安装目录。

## 6. 集成测试范围

Framework integration test 覆盖：

```text
State
-> ViewModel reaction
-> Fake dispatcher
-> Lifecycle scope
```

真实 UI 绑定刷新由 Presentation 平台集成测试覆盖。
