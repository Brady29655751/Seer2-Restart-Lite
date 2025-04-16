using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreateRoomController : Module
{
    [SerializeField] private CreateRoomModel createModel;
    [SerializeField] private CreateRoomView createView;
    [SerializeField] private WorkshopLearnBuffController buffController;

    public override void Init() {
        base.Init();
        buffController.SetDIYSuccessCallback(info => AddRule(info.id));
    }

    public void SetPetCount(int count) {
        createModel.SetPetCount(count);
        createView.SetPetCount(count);
    }

    public void SetTurnTime(int time) {
        createModel.SetTurnTime(time);
        createView.SetTurnTime(time);
    }

    public void SetItemBagApproval(bool approved) {
        createModel.SetItemBagApproval(approved);
        createView.SetItemBagApproval(approved);
    }

    public void SetTeamReveal(bool reveal) {
        createModel.SetTeamReveal(reveal);
        createView.SetTeamReveal(reveal);
    }

    public void AddRule(int buffId) {
        createModel.AddRule(buffId);
        createView.SetRules(createModel.rules);
    }

    public void RemoveRule() {
        createModel.RemoveRule();
        createView.SetRules(createModel.rules);
    }

    public void CreateRoom() {
        createView.CreateRoom();
        createModel.CreateRoom();
    }
}
