using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FusionPanel : Panel
{
    [SerializeField] private FusionModel fusionModel;
    [SerializeField] private FusionView fusionView;

    public void SelectMainPet() {
        var panel = Hintbox.OpenHintbox<PetSelectHintbox>();
        panel.SetTitle("请选择要融合的主精灵");
        panel.SetStorage(Player.instance.petBag.ToList());
        panel.SetConfirmSelectCallback(SetMainPet);
    }

    private void SetMainPet(Pet pet) {
        fusionModel.SetMainPet(pet);
        fusionView.SetMainPet(pet);
        fusionView.SetItem(fusionModel.fusionItems);
    }

    public void SelectSubPet() {
        var panel = Hintbox.OpenHintbox<PetSelectHintbox>();
        panel.SetTitle("请选择要融合的副精灵");
        panel.SetStorage(Player.instance.petBag.ToList());
        panel.SetConfirmSelectCallback(SetSubPet);
    }

    private void SetSubPet(Pet pet) {
        fusionModel.SetSubPet(pet);
        fusionView.SetSubPet(pet);
        fusionView.SetItem(fusionModel.fusionItems);
    }

    public void OnConfirmFusion() {
        if (!fusionModel.TryFusion(out var resultPetId))
            return;

        OnAfterFusion(resultPetId);
    }

    private void OnAfterFusion(int resultPetId) {
        fusionModel.fusionItems.ForEach(item => Item.Remove(item.id, item.num));

        var pet = new Pet(resultPetId);
        pet.talent.iv = Mathf.Clamp(fusionModel.mainPet.talent.iv * 2 / 3 + fusionModel.subPet.talent.iv * 1 / 3 + 2, 0, 31);
        pet.basic.personality = Status.Add(fusionModel.mainPet.basic.personality, fusionModel.subPet.basic.personality);
        pet.feature.SetTrait();
        Pet.Add(pet);
        SaveSystem.SaveData();
        Hintbox.OpenHintboxWithContent("获得了" + pet.name + "！", 16);;
    }
}
