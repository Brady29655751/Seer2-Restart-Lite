# 阿卡迪亚：重启

用 Unity 重现的《约瑟传说》（赛尔号II）**单机**游戏，此仓库主要存放源码。  
添加了部分自制内容，基本上算是同人游戏。  
数据储存于本地，与淘米官方不互通。  

## 游戏特色
1. 从零开始自制，有部分原创内容（例如原创精灵和boss挑战），也修改了部分原本内容（例如平衡调整）
2. 玩家可以透过游戏内的【创意工坊】或本地编辑文件来添加专属游戏内容，实装新精灵、道具、地图等，也可以和其他玩家分享这些专属内容  

## 如何游玩？
Dropbox因容量接近免费额度上限，未来将不会在dropbox上发布，联网更新功能也已停用  
现在游玩请加入以下群组：[Discord](https://discord.gg/g9PzpBsbcH)

## 我想自己开发游戏内容和代码，如何部署？
[参考 Issue #4](https://github.com/Brady29655751/Seer2-Restart-Lite/issues/4)  

运行中可能会报错 Failed to read data for the AssetBundle 'pfa_XXX'，是正常现象。  
部分精灵没有实装动画，并不影响游戏运行。若有自动暂停情形，可以在Unity的Console或Preference选项禁用Error Pause

## 目录说明

*内部资料：Assets/Resources/Data/  
*面板资源：Assets/Resources/Panel/  
*系统相关：Assets/Scripts/System/  
*基础架构：Assets/Scripts/MVC/Model/Basic/  
*战斗相关：Assets/Scripts/Scene Specific/Battle  

## CI自动构建工作流

仓库使用 GitHub Actions 自动执行 Unity CI，配置文件是 [`.github/workflows/unity-ci.yaml`](.github/workflows/unity-ci.yaml)。  
它的作用很简单：

- 推送到 `main` 时，自动跑测试并构建Windows版本
- 提交Pull Request时，自动跑测试，避免明显问题进入主分支
- 手动触发时，可以按需选择是否测试，以及构建 Android / Windows / WebGL / macOS
- 构建完成后，会把产物和日志上传到GitHub Actions Artifacts，方便下载

## 怎么使用这个CI？

如果你想自己在GitHub上构建：

0. Fork仓库
1. 打开仓库的Actions页面
2. 选择Unity CI工作流
3. 点击Run workflow
4. 勾选你要执行的内容
5. 等待任务完成后，到本次运行的Artifacts下载构建结果和日志

## 使用前需要准备什么？

这个CI依赖Unity许可证相关的GitHub Secrets。没有这些配置时，工作流无法正常测试或构建。

- `UNITY_EMAIL`
- `UNITY_PASSWORD`
- `UNITY_TOTP_KEY`
- `UNITY_SERIAL`或`UNITY_LICENSE`
- `ACCESS_TOKEN`：GitHub Token
  - 权限：Read and Write access to secrets

仓库里也提供了一个“只刷新许可证”的手动入口，可用于自动更新`UNITY_LICENSE`。
