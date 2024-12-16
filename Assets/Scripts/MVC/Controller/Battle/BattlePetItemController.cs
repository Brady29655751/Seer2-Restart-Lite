using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePetItemController : Module
{
    public Battle battle => Player.instance.currentBattle;
    public BattleState state => battle?.currentState;
    [SerializeField] private PetItemModel itemModel;
    [SerializeField] private BattlePetItemView itemView;
    [SerializeField] private PageView pageView;

    public void SetItemBag(List<Item> itemBag) {
        itemModel.SetStorage(itemBag);
        OnItemSetPage();
    }

    public void OnItemButtonClick(int index) {
        Item item = itemModel.items[index];
        if (item == null)
            return;

        if (!item.IsUsable(state.myUnit, state)) {
            Hintbox hintbox = Hintbox.OpenHintbox();
            hintbox.SetTitle("使用失败");
            hintbox.SetContent("目前无法使用此道具", 14, FontOption.Arial);
            hintbox.SetOptionNum(1);
            return;
        }
        battle.SetSkill(Skill.GetItemSkill(item), true);
        if (battle.settings.isSimulate)
            return;
        
        if (battle.settings.mode == BattleMode.YiTeRogue)
            Item.RemoveFrom(item.id, 1, YiTeRogueData.instance.itemBag);
        else
            Item.Remove(item.id, 1);
    }

    public void SetDescriptionBoxActive(bool active) {
        itemView.SetDescriptionBoxActive(active);
    }

    public void ShowItemInfo(int index) {
        if (!index.IsInRange(0, itemModel.items.Length)) {
            SetDescriptionBoxActive(false);
            return;
        }
        itemView.ShowItemInfo(index, itemModel.items[index]);
    }

    public void OnItemSetPage() {
        itemView.SetItems(itemModel.selections.ToList());
        pageView?.SetPage(itemModel.page, itemModel.lastPage);
    }

    public void OnItemPrevPage() {
        itemModel.PrevPage();
        OnItemSetPage();
    }

    public void OnItemNextPage() {
        itemModel.NextPage();
        OnItemSetPage();
    }

}
