using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChangeNameModel : Module
{
    [SerializeField] private IInputField ipf;
    [SerializeField] private IDropdown idp;
    public bool isDone { get; private set; } = true;
    public string inputString => ipf.inputString;

    private List<Item> achievementList = new List<Item>();
    public string achievement => (idp.value == 0) ? "无" : achievementList[idp.value - 1].name;

    public override void Init()
    {
        base.Init();
        achievementList = Player.instance.gameData.itemStorage.Where(x => (x != null) && (x.info.type == ItemType.Achievement) || (x.info.type == ItemType.Shoot))
            .OrderByDescending(x => x.info.type).ThenBy(x => x.id).ToList();

        idp.SetDropdownOptions("无".SingleToList().Concat(achievementList.Select(x => x.name)).ToList());
        SetAchievement(Player.instance.gameData.achievement);
    }

    public void SetAchievement(int value) {
        idp.value = achievementList.FindIndex(x => x.id == value) + 1;
    }

    public void SetDone(bool done) {
        isDone = done;
    }

    public void OnAfterChangeName() {
        SetDone(!isDone);
    }

    public void OnAfterChangeAchievement(int value) {
        Player.instance.gameData.achievement = (value == 0) ? 0 : achievementList[value - 1].id;
        SaveSystem.SaveData();
    }

}
