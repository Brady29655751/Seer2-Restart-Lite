using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleCardDeckView : BattleBaseView
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private IButton deckButton, graveButton, buffButton;
    [SerializeField] private GameObject checkCardPanel;
    [SerializeField] private RectTransform checkCardContentRect;

    protected override void Awake()
    {
        base.Awake();
        if (battle.settings.mode != BattleMode.Card)
        {
            gameObject.SetActive(false);
        }
    }

    public void CheckDeck()
    {
        TransformHelper.DestoryChildren(checkCardContentRect);

        var deck = battle.currentState?.myUnit.cardSystem.deck ?? new List<Skill>();
        for (int i = 0; i < deck.Count; i++)
        {
            int copy = i;
            var skill = deck[i];
            var obj = Instantiate(cardPrefab, checkCardContentRect);
            var card = obj?.GetComponent<PetSkillCardView>();

            card?.SetSkill(skill);
            card?.SetDraggable(false);
            card?.SetCallback(() => SetInfoPromptActive(true), "enter");
            card?.SetCallback(() => SetInfoPromptActive(false), "exit");
            card?.SetCallback(() => infoPrompt.SetCard(skill, copy % 4 < 2), "over");
        }

        checkCardPanel?.SetActive(true);
    }

    public void CheckGrave()
    {
        TransformHelper.DestoryChildren(checkCardContentRect);

        var grave = battle.currentState?.myUnit.cardSystem.grave ?? new List<Skill>();
        for (int i = 0; i < grave.Count; i++)
        {
            int copy = i;
            var skill = grave[i];
            var obj = Instantiate(cardPrefab, checkCardContentRect);
            var card = obj?.GetComponent<PetSkillCardView>();

            card?.SetSkill(skill);
            card?.SetDraggable(false);
            card?.SetCallback(() => SetInfoPromptActive(true), "enter");
            card?.SetCallback(() => SetInfoPromptActive(false), "exit");
            card?.SetCallback(() => infoPrompt.SetSkill(skill, copy % 4 < 2), "over");
        }

        checkCardPanel?.SetActive(true);
    }

    public void CheckBuff()
    {
        Hintbox.OpenHintboxWithContent("当前无任何加成", 16);
    }
}
