using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : Module
{
    [SerializeField] private OptionSelectController optionSelectController;
    [SerializeField] private PetSelectController playerController, enemyController;
    [SerializeField] private DoorModel doorModel;
    [SerializeField] private DoorView doorView;

    private PetBagPanel petBagPanel;
    
    public override void Init() {
        playerController.SetStorage(Player.instance.petBag.Take(doorModel.petNum).ToList());
        playerController.Select(0);
    }

    public void SetDoor(string door) {
        doorModel.SetDoor(door);
        RefreshView();
    }

    public void SetMode(string mode) {
        doorModel.SetMode(mode);
        RefreshView();
    }

    public void RefreshView() {
        doorView.SetMode(doorModel.mode);
        doorView.SetFloor(Mathf.Min(doorModel.floorNum, 21).ToString());
        
        optionSelectController.Select(doorModel.doorIndex);
        
        playerController.SetStorage(playerController.GetPetSelections().Take(doorModel.petNum).ToList());
        playerController.Select(0);

        bool isSpecialDoor = (doorModel.door == "competition") || (doorModel.door == "hero");
        if (isSpecialDoor && (doorModel.floorNum < 21) && (doorModel.floorNum % 7 != 0))
            enemyController.SetStorage(new List<Pet>());
        else
            enemyController.SetStorage(doorModel.battleInfo.enemyInfo.Select(x => new Pet(x.petId, x.level)).ToList());

        enemyController.Select(0);
    }

    public void StartBattle() {
        doorModel.StartBattle();
    }

    public void RestartFromFirstFloor() {
        if (doorModel.mode == "easy") {
            Hintbox.OpenHintboxWithContent("简单模式无法从第1层重新开始哦！", 16);
            return;
        }

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要清除纪录，从第 1 层重新开始吗？", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(() => {
            doorModel.RestartFromFirstFloor();
            SetDoor(doorModel.door);
        });
    }

    public void OpenPetBag() {
        petBagPanel = Panel.OpenPanel<PetBagPanel>();
        petBagPanel.onCloseEvent += RefreshPetBag;
    }

    private void RefreshPetBag() {
        playerController.SetStorage(petBagPanel.petBag.Take(doorModel.petNum).ToList());
        playerController.Select(0);
        petBagPanel.onCloseEvent -= RefreshPetBag;
    }
}
