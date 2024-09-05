using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFeatureView : Module
{
    private Pet currentPet;
    private Action<Buff> onRemoveCallback = null;
    [SerializeField] private BattlePetBuffView defaultBuffView, afterwardBuffView;
    
    public void SetPet(Pet pet) {
        currentPet = pet;
        SetDefaultBuffs(pet.info.ui.defaultBuffs);
        SetAfterwardBuffs(pet.feature.afterwardBuffs);
    }

    public void SetOnRemoveCallback(Action<Buff> onRemoveCallback) {
        this.onRemoveCallback = onRemoveCallback;
    }
    
    private void SetDefaultBuffs(List<Buff> defaultBuffs) {
        defaultBuffView.SetBuff(defaultBuffs);
    }
    
    private void SetAfterwardBuffs(List<Buff> afterwardBuffs) {
        afterwardBuffView.SetBuff(afterwardBuffs, (buff) => {
            OnRemoveAfterwardBuff(buff);
            onRemoveCallback?.Invoke(buff);
        });
    }
    
    private void OnRemoveAfterwardBuff(Buff buff) {
        var itemInfo = Item.GetItemInfo(buff.info.itemId);
        if (itemInfo == null) {
            Hintbox.OpenHintboxWithContent("该加成无法移除（无对应道具）", 16);    
            return;
        }

        currentPet.feature.afterwardBuffIds.Remove(buff.id);
        Item.Add(new Item(buff.info.itemId, 1));
        SaveSystem.SaveData();

        Hintbox.OpenHintboxWithContent("已移除后天加成【" + buff.name + "】\n获得了1个【" + itemInfo.name + "】", 16);
        SetAfterwardBuffs(currentPet.feature.afterwardBuffs);
    }
    
}
