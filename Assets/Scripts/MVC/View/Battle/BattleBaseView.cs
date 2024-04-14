using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBaseView : Module
{
    public Battle battle => Player.instance.currentBattle;
    public BattleManager UI => battle.UI;
    [SerializeField] protected DescriptionBox descriptionBox;
    [SerializeField] protected InfoPrompt infoPrompt;

    public virtual void SetDescriptionBoxActive(bool active) {
        descriptionBox.gameObject.SetActive(active);
    }

    public virtual void SetInfoPromptActive(bool active) {
        infoPrompt.SetActive(active);
    }
    
}
