using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFeatureView : Module
{
    private PetBagMode petBagMode;
    private Pet currentPet;
    private Action<Buff> onRemoveCallback = null;
    [SerializeField] private BattlePetBuffView defaultBuffView, afterwardBuffView;
    
    public void SetMode(PetBagMode mode) {
        petBagMode = mode;
    }

    public void SetPet(Pet pet) {
        currentPet = pet;

        var bornBuffs = (pet == null) ? new List<Buff>() : new List<Buff>(){ Buff.GetFeatureBuff(pet), Buff.GetEmblemBuff(pet) };
        var initBuffs = pet?.feature.afterwardBuffs?.Where(x => (x != null) && (x.info.position == "first")) ?? new List<Buff>();
        var afterwardBuffs = pet?.feature.afterwardBuffs?.Where(x => (x != null) && (x.info.position != "first")).ToList() ?? new List<Buff>();

        SetDefaultBuffs(bornBuffs.Concat(pet.info.ui.defaultBuffs).Concat(initBuffs).Concat(pet.resist.resistBuffs).ToList());
        SetAfterwardBuffs(afterwardBuffs);
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

        if (petBagMode == PetBagMode.Normal)
            Item.Add(item);

        SaveSystem.SaveData();

        Hintbox.OpenHintboxWithContent("已移除后天加成【" + buff.name + "】\n获得了1个【" + itemInfo.name + "】", 16);
        SetAfterwardBuffs(currentPet.feature.afterwardBuffs);
    }
    
}
