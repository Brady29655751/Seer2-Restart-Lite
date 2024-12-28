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
        SetDefaultBuffs((pet == null) ? new List<Buff>() : (new List<Buff>(){ Buff.GetFeatureBuff(pet), Buff.GetEmblemBuff(pet) }).Concat(pet.info.ui.defaultBuffs).ToList());
        SetAfterwardBuffs(pet?.feature.afterwardBuffs ?? new List<Buff>());
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

        var item = new Item(buff.info.itemId, 1);
        // var handler = new EffectHandler();
        // 
        // handler.AddEffects(currentPet, item.effects.Where(x => x.timing == EffectTiming.OnRemoveBuff).ToList());
        // handler.CheckAndApply(null, false);

        currentPet.feature.afterwardBuffIds.Remove(buff.id);
        Item.Add(item);

        SaveSystem.SaveData();

        Hintbox.OpenHintboxWithContent("已移除后天加成【" + buff.name + "】\n获得了1个【" + itemInfo.name + "】", 16);
        SetAfterwardBuffs(currentPet.feature.afterwardBuffs);
    }
    
}
