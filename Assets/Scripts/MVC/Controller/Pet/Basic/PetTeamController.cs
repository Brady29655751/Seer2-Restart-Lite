using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PetTeamController : Module
{
    [SerializeField] private PetSelectController selectController;
    [SerializeField] private PetTeamModel teamModel;
    [SerializeField] private PetSelectView teamView;
    [SerializeField] private PageView pageView;

    public event Action<Pet[]> onSelectTeamSuccessEvent;

    private int petCount => (int)PhotonNetwork.CurrentRoom.CustomProperties["count"];

    public override void Init()
    {
        base.Init();
        SetStorage(Player.instance.gameData.pvpPetTeam);
    }

    public void SetStorage(List<IKeyValuePair<string, Pet[]>> storage) {
        var defaultTeam = Enumerable.Repeat(3, petCount).Select(x => Pet.GetExamplePet(x)).ToArray();
        var defaultChoice = new IKeyValuePair<string, Pet[]>("默认队伍", defaultTeam);
        var mergeStorage = storage.Concat(new List<IKeyValuePair<string, Pet[]>>(){ defaultChoice }).ToList();

        teamModel.SetStorage(mergeStorage);
        teamModel.Filter(x => x.value.Count(x => x != null) == petCount);
        OnSetPage();
    }

    public void OnCreateTeam() {
        string name = teamModel.teamName;
        string message = "存在同名称队伍配置，请重新命名";

        if (string.IsNullOrEmpty(name))
            message = "队伍名称不能为空！";

        else if ((name != "默认队伍") && (!Player.instance.gameData.pvpPetTeam.Exists(x => x.key == name))) {
            var team = selectController.GetPetSelections()?.Select(x => (x == null) ? null : new Pet(x)).ToArray();
            Player.instance.gameData.pvpPetTeam.Add(new IKeyValuePair<string, Pet[]>(name, team));
            SetStorage(Player.instance.gameData.pvpPetTeam);
            SaveSystem.SaveData();

            message = "成功保存当前PVP队伍配置";
        }

        Hintbox.OpenHintboxWithContent(message, 16);
    }

    public void OnDeleteTeam() {
        if (teamModel.currentSelectedItems.Length == 0)
            return;

        var name = teamModel.currentSelectedItems.FirstOrDefault()?.key;

        if (name == "默认队伍")
            return;

        var hintbox = Hintbox.OpenHintboxWithContent("确定要删除以下队伍吗？\n【" + name + "】", 16);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmDeleteTeam);


        void OnConfirmDeleteTeam() {
            Player.instance.gameData.pvpPetTeam.RemoveAll(x => x.key == name);
            SetStorage(Player.instance.gameData.pvpPetTeam);
            SaveSystem.SaveData();

            Hintbox.OpenHintboxWithContent("删除队伍成功", 16);
        }
    }

    public void OnConfirmTeam() {
        var team = teamModel.currentSelectedItems.FirstOrDefault()?.value.Select(x => (x == null) ? null : new Pet(x)).ToArray();
        onSelectTeamSuccessEvent?.Invoke(team);

        Hintbox.OpenHintboxWithContent("切换队伍成功", 16);
    }

    private void OnSetPage() {
        teamModel.Select(0);
        teamView.SetStorageCountText(teamModel.currentSelectedItems.FirstOrDefault()?.key);
        teamView.SetSelections(teamModel.currentSelectedItems.FirstOrDefault()?.value);

        pageView?.SetPage(teamModel.page, teamModel.lastPage);
    }

    public void PrevPage() {
        teamModel.PrevPage();
        OnSetPage();
    }

    public void NextPage() {
        teamModel.NextPage();
        OnSetPage();
    }
}
