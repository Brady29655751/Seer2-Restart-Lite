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

