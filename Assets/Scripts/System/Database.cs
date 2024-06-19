using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Database : Singleton<Database>
{
    private ResourceManager RM => ResourceManager.instance;

    public Dictionary<int, Skill> skillDict = new Dictionary<int, Skill>();
    public Dictionary<int, PetFeatureInfo> featureInfoDict = new Dictionary<int, PetFeatureInfo>();
    public Dictionary<int, PetInfo> petInfoDict = new Dictionary<int, PetInfo>();
    public Dictionary<int, ItemInfo> itemInfoDict = new Dictionary<int, ItemInfo>();
    public Dictionary<int, BuffInfo> buffInfoDict = new Dictionary<int, BuffInfo>();
    public Dictionary<int, MissionInfo> missionInfoDict = new Dictionary<int, MissionInfo>();
    public Dictionary<string, ActivityInfo> activityInfoDict = new Dictionary<string, ActivityInfo>();

    public List<MissionInfo> missionInfos = new List<MissionInfo>();
    public List<ActivityInfo> activityInfos = new List<ActivityInfo>();

    public void Init() {
        RM.LoadPetInfo((x) => petInfoDict = x, (y) => featureInfoDict = y);
        RM.LoadSkill((x) => skillDict = x);
        RM.LoadBuffInfo((x) => buffInfoDict = x);
        RM.LoadItemInfo((x) => itemInfoDict = x);
        RM.LoadMissionInfo((x) => { 
            missionInfoDict = x;
            missionInfos = missionInfoDict.Select(entry => entry.Value).ToList();
        });
        RM.LoadActivityInfo((x) => { 
            activityInfoDict = x; 
            activityInfos = activityInfoDict.Select(entry => entry.Value).OrderByDescending(x => ActivityInfo.IsMod(x.id) ? 1 : 0).ThenByDescending(x => x.releaseDate).ToList();
        });
    }

    public bool VerifyData(out string error) {
        if (missionInfoDict.Count != GameManager.versionData.missionData.totalMissionCount) {
            error = "获取任务档案失败";
            return false;
        }
        if (activityInfoDict.Count == 0) {
            error = "获取活动资讯档案失败";
            return false;
        }
        if (itemInfoDict.Count == 0) {
            error = "获取道具档案失败";
            return false;
        }
        if (buffInfoDict.Count == 0) {
            error = "获取战斗印记档案失败";
            return false;
        }
        if (skillDict.Count == 0) {
            error = "获取技能档案失败";
            return false;
        }
        if (featureInfoDict.Count == 0) {
            error = "获取特性档案失败";
            return false;
        }
        if (petInfoDict.Count == 0) {
            error = "获取精灵档案失败";
            return false;
        }
        error = string.Empty;
        return true;
    }

    public void GetMap(int id, Action<Map> onSuccess = null, Action<string> onFail = null) {
        RM.LoadMap(id, onSuccess, onFail);
    }

    public Skill GetSkill(int id) {
        return (id == 0) ? null : skillDict.Get(id);
    }

    public PetFeatureInfo GetFeatureInfo(int id) {
        return (id == 0) ? null : featureInfoDict.Get(id);
    }

    public PetInfo GetPetInfo(int id) {
        return (id == 0) ? null : petInfoDict.Get(id);
    }

    public ItemInfo GetItemInfo(int id) {
        if (id == 0)
            return null;

        return itemInfoDict.Get(id);
    }

    public BuffInfo GetBuffInfo(int id) {
        if (id == 0)
            return null;

        return buffInfoDict.Get(id);
    }

    public ActivityInfo GetActivityInfo(string id) {
        if (id == string.Empty)
            return null;

        return activityInfoDict.Get(id);
    }

    public MissionInfo GetMissionInfo(int id) {
        return (id == 0) ? null : missionInfoDict.Get(id);
    }
    
}
