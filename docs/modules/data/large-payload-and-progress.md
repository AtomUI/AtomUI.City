# AtomUI.City.Data Large Payload and Progress 设计

版本：v0.1
状态：正式初版
适用范围：上传、下载、大载荷、进度、range、临时文件、节流、取消和内存约束。

## 1. 定位

桌面应用经常处理大文件、内网资源、报表、导入导出和长时间下载。Data 必须支持大载荷和进度，而不是把所有内容一次性读入内存。

## 2. Upload / Download

Data operation 可以声明：

- Upload。
- Download。
- Download to stream。
- Download to temporary file。
- Range request。
- Resumable transfer。

## 3. 进度模型

进度事件应包含：

- OperationId。
- Bytes transferred。
- Total bytes if known。
- Percent if computable。
- Speed estimate。
- Stage。
- CancellationToken。

进度通知必须节流。

## 4. 内存约束

规则：

- 大文件默认流式处理。
- 不默认把完整 payload 放入内存。
- 临时文件必须绑定 OperationScope。
- Operation 取消时清理临时文件。
- 插件上传下载不能把 Host stream 长期保存。

## 5. UI 线程

进度回调不能直接更新 UI。

```text
Transport progress
-> Data progress dispatcher
-> throttled progress state
-> Presentation binding / State subscription
```

## 6. 错误策略

| 场景 | 默认处理 |
|---|---|
| 用户取消 | Cancelled，清理临时文件。 |
| disk full | LocalStorageError。 |
| network lost | NetworkUnavailable。 |
| range unsupported | Fallback 或 Failed，按 policy。 |
| progress handler failed | 记录诊断，不杀死 transport。 |

## 7. 测试策略

测试必须覆盖：

- upload progress。
- download progress。
- cancellation cleanup。
- range request。
- large payload streaming。
- progress throttle。
- disk error。
