[网络命令行工具-ken](https://kentxxq.com/posts/%E7%AC%94%E8%AE%B0/%E7%BD%91%E7%BB%9C%E5%91%BD%E4%BB%A4%E8%A1%8C%E5%B7%A5%E5%85%B7-ken/)

## Native AOT 体积分析

以下数据用于评估 Native AOT 改造和功能删减，不代表当前项目已经启用 Native AOT。

### 测量口径

- 环境：Windows x64、.NET SDK 10.0.302、.NET 10.0.10 Runtime Pack。
- 发布参数：`dotnet publish src/Cli/Cli.csproj -c Release -r win-x64 -p:PublishAot=true`。
- 使用 MiB（`1 MiB = 1,048,576 bytes`）。
- “运行体积”统计发布目录中运行所需文件，排除 `.pdb` 调试符号。
- 表格使用累积测量：每一行都在上一行的基础上增加一个命令及其必要依赖。
- 增量与加入顺序有关。共享代码计入第一个使用它的命令，不能把不同顺序下的边际数据直接相加。

### 为什么 Hello World 只有约 1 MiB，实际程序却更大

同环境下，最简单的 .NET 10 Native AOT Hello World 为 1,106,944 bytes（1.06 MiB）。“单文件”表示发布形式，并不保证文件固定为 1 MiB。Native AOT 会把实际使用的运行时和基础类库代码编译进可执行文件；命令行解析、终端渲染、TLS、WebSocket、HTTP、Redis、ASP.NET Core 等功能都会增加原生代码。

### 精简方案的累积体积

这个实验移除了 `tr`、`k8s` 和 `web`，并同时移除失去用途的 Masuit、KubernetesClient 和 ASP.NET Core 依赖。最终只产生一个 `ken.exe`，无需附带运行时 DLL。

| 累积步骤 | 总体积 | 本步增加 |
| --- | ---: | ---: |
| Hello World | 1.06 MiB | - |
| 命令行外壳 | 4.09 MiB | 3.04 MiB |
| 加入 `sp` | 4.52 MiB | 0.42 MiB |
| 加入 `ws` | 6.72 MiB | 2.20 MiB |
| 加入 `ss` | 6.73 MiB | 0.01 MiB |
| 加入 `redis` | 9.78 MiB | 3.05 MiB |
| 加入 `update` | 9.90 MiB | 0.12 MiB |
| 加入 `wp` | 10.32 MiB | 0.42 MiB |
| 加入 `bm` | 10.34 MiB | 0.02 MiB |
| 加入 `mirror` | **12.18 MiB** | **1.84 MiB** |

主要增量来源：

- 命令行外壳包含 System.CommandLine、Spectre.Console 和异常处理，较 Hello World 增加约 3.04 MiB。
- `ws` 首次引入 WebSocket、HTTP 和 TLS 实现，增加约 2.20 MiB；后续 HTTP 命令可以复用这些代码。
- `redis` 引入 StackExchange.Redis 及相关实现，增加约 3.05 MiB。
- `mirror` 引入配置文件、XML、JSON、进程调用、日志和枚举生成相关代码，增加约 1.84 MiB。

### 保留全部命令的累积体积

下表按 `sp`、`ws`、`ss`、`tr`、`redis`、`k8s`、`web`、`update`、`wp`、`bm`、`mirror` 的顺序逐步加入功能。

| 累积步骤 | 运行体积 | 本步增加 | EXE 体积 |
| --- | ---: | ---: | ---: |
| Hello World | 1.06 MiB | - | 1.06 MiB |
| 命令行外壳 | 4.09 MiB | 3.04 MiB | 4.09 MiB |
| 加入 `sp` | 4.52 MiB | 0.42 MiB | 4.52 MiB |
| 加入 `ws` | 6.72 MiB | 2.20 MiB | 6.72 MiB |
| 加入 `ss` | 6.73 MiB | 0.01 MiB | 6.73 MiB |
| 加入 `tr` | 19.14 MiB | 12.41 MiB | 7.44 MiB |
| 加入 `redis` | 22.09 MiB | 2.94 MiB | 10.38 MiB |
| 加入 `k8s` | 46.55 MiB | 24.47 MiB | 34.85 MiB |
| 加入 `web` | 53.47 MiB | 6.91 MiB | 41.39 MiB |
| 加入 `update` | 53.50 MiB | 0.03 MiB | 41.42 MiB |
| 加入 `wp` | 53.90 MiB | 0.40 MiB | 41.82 MiB |
| 加入 `bm` | 53.91 MiB | 0.01 MiB | 41.84 MiB |
| 加入 `mirror` | **55.10 MiB** | **1.19 MiB** | **43.02 MiB** |

完整实验产物包含：

- `ken.exe`：45,111,296 bytes（43.02 MiB）。
- `libSkiaSharp.dll`：12,272,440 bytes（11.70 MiB）。它来自 `Masuit.Tools.Core -> Masuit.Tools.Abstractions -> SkiaSharp` 依赖链；项目中的 `tr` 使用了 Masuit 的私有 IP 判断。
- `aspnetcorev2_inprocess.dll`：392,528 bytes（0.37 MiB），由 `web` 使用的 ASP.NET Core 引入。

### AOT 兼容性结论

- 项目自身的动态 JSON 序列化需要改为 `System.Text.Json` 源生成。
- `update` 原先的反射代理创建应改为显式 `switch`；GitHub 最新版本查询可以直接调用 HTTP API，以避免 Octokit 的 AOT 警告。
- KubernetesClient 19.0.2 可以完成 Native AOT 编译，但 kubeconfig 的 YAML 解析会在运行时读取反射 `MetadataToken`。Native AOT 不提供该元数据，因此 `k8s` 命令会抛出 `TypeInitializationException`，不能视为可用。
- 若要发布可靠的 AOT 版本，当前最直接的方案是移除 `k8s`。若还要显著缩小体积，可继续移除 `tr` 和 `web`，并同步删除 Masuit 与 ASP.NET Core 引用。

### 后续体积优化建议

1. 删除命令时同步删除只供该命令使用的包、框架引用、工具类和原生资产；只删除命令注册并不会保证依赖文件消失。
2. 同时记录 EXE 大小和排除 PDB 后的发布目录大小。原生依赖可能不在 EXE 内，例如 SkiaSharp。
3. 使用累积发布或“完整版本减去单个功能”的实际发布结果衡量体积，不根据 NuGet 包大小推测。
4. 每次修改依赖后重新检查 AOT/裁剪警告，并实际运行对应命令。发布成功只能证明能够生成原生程序，不能证明所有运行路径兼容 AOT。
5. 发布产物中的 `.pdb` 仅用于调试，不应计入部署体积；需要排查原生崩溃时再单独保留。
