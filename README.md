# 小猫互动不扣血

**Demon Lord** 游戏 Mod，让小猫房间和女神房间的互动不再消耗 HP。

## 功能

- **小猫房间**：与黑猫 NPC 互动时不造成伤害
- **女神房间**：选择能力时不扣除最大 HP

## 安装方法

### 方法一：创意工坊订阅（推荐）

在 Steam 创意工坊搜索并订阅此 Mod。

### 方法二：手动安装

1. 下载最新版本的 Release 文件
2. 解压到游戏 Mod 目录：
   ```
   C:\Users\<用户名>\AppData\LocalLow\YuWave\DemonLordJustABlock\LocalMods\
   ```
3. 启动游戏，在 Mod 管理界面启用「小猫互动不扣血」

## 使用说明

启用 Mod 后自动生效，无需额外操作：

- 进入**黑猫房间**，与小猫互动时 HP 不会减少
- 进入**女神房间**，选择能力时 HP 上限不会减少

## 技术细节

本 Mod 使用 [Harmony](https://github.com/pardeike/Harmony) 库进行方法拦截：

| 拦截方法 | 效果 |
|---------|------|
| `UnitObjectPlayer.GetDamage()` | 当攻击者为猫 NPC (unitType=1114) 时跳过伤害 |
| `UnitObjectOther.HandlePurchase()` | 将 HP 价格设为 0，选择不扣血 |

## 版本历史

| 版本 | 日期 | 更新内容 |
|------|------|---------|
| V0.1.1 | 2026-06-20 | 更改 Mod 名称，添加专属图标 |
| V0.1.0 | 2026-06-20 | 初始版本，实现小猫和女神房间免扣血功能 |

## 开发

### 编译环境

- .NET SDK (netstandard2.1)
- [0Harmony](https://github.com/pardeike/Harmony)
- DemonLordJustABlock 游戏 DLL

### 编译命令

```bash
cd CodeMods
dotnet build -c Release
```

### 项目结构

```
src/NoBloodMod/
├── mod.json              # Mod 配置
├── icon.png              # Mod 图标
└── CodeMods/
    ├── codemod.json      # 代码 Mod 配置
    ├── NoBloodMod.cs     # 主程序源码
    ├── NoBloodMod.csproj # 项目文件
    ├── 0Harmony.dll      # Harmony 库
    └── NoBloodMod.dll    # 编译产物
```

## 许可证

MIT License

## 反馈

如有问题或建议，请在 GitHub Issues 中提出。

---

 Generated with [Claude Code](https://claude.com/claude-code)
