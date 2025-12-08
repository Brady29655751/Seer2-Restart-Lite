using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BattleCardHandView : BattleBaseView
{
    public Image cardStayPanel, cardUsePanel;


    [Header("Card Layout Settings")]
    public float radius = 200f ;          // Arc radius in UI units (pixels)
    public float maxAngle = 15f;         // Max spread angle (degrees)
    public float yOffset = -50f;         // Vertical offset in UI space
    public float animationSpeed = 10f;   // Lerp speed for smooth movement
    public List<PetSkillCardView> cards = new List<PetSkillCardView>(); // cards


    private int isDraggingIndex = -1;
    private bool needsUpdate = false;
    private List<Vector2> targetPositions = new List<Vector2>();
    private List<Quaternion> targetRotations = new List<Quaternion>();


    protected override void Awake()
    {
        base.Awake();
        cards.ForEach(x => x.gameObject.SetActive(false));
    }

    public override void Init()
    {
        base.Init();
        // SetHand(100, Enumerable.Range(10001, 7).Select(x => Skill.GetSkill(x)).ToList());
    }

    void Update()
    {
        // Smoothly animate towards target positions
        for (int i = 0; i < Mathf.Min(cards.Count, targetPositions.Count); i++)
        {
            if (i == isDraggingIndex)
                continue;

            cards[i].rectTransform.anchoredPosition = Vector2.Lerp(
                cards[i].rectTransform.anchoredPosition,
                targetPositions[i],
                Time.deltaTime * animationSpeed
            );

            cards[i].rectTransform.localRotation = Quaternion.Lerp(
                cards[i].rectTransform.localRotation,
                targetRotations[i],
                Time.deltaTime * animationSpeed
            );
        }
    }

    void LateUpdate()
    {
        if (needsUpdate)
        {
            CalculateCardLayout();
            needsUpdate = false;
        }
    }

    /// <summary>
    /// Calculates target positions and rotations for cards.
    /// </summary>
    private void CalculateCardLayout()
    {
        targetPositions.Clear();
        targetRotations.Clear();

        var cards = this.cards.Where(x => x.gameObject.activeSelf).ToList();
        if (cards.Count == 0) return;

        float angleStep = 0;
        if (cards.Count > 1)
            angleStep = Mathf.Min(maxAngle * 2 / (cards.Count - 1), maxAngle * 2 / 3);

        float startAngle = -angleStep * (cards.Count - 1) / 2;

        for (int i = 0; i < cards.Count; i++)
        {
            float angle = startAngle + angleStep * i;
            float rad = Mathf.Deg2Rad * angle;

            Vector2 pos = new Vector2(
                Mathf.Sin(rad) * radius * cards.Count,
                yOffset + Mathf.Cos(rad) * 20f
            );

            Quaternion rot = Quaternion.Euler(0, 0, -angle);

            targetPositions.Add(pos);
            targetRotations.Add(rot);
        }
    }

    /// <summary>
    /// Forces a manual refresh of the hand layout.
    /// </summary>
    public void RefreshHand()
    {
        needsUpdate = true;
    }

    public void SetHand(int anger, List<Skill> skills)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(i < skills.Count);
            cards[i].SetSkill(skills.Get(i));
            cards[i].SetDraggable(anger >= (skills.Get(i)?.anger ?? int.MaxValue));
        }

        RefreshHand();
    }


    public void ShowDescription(int index)
    {
        if (index < 0 || index >= cards.Count)
            return;

        if (cards[index].currentSkill == null)
            return;

        infoPrompt.SetCard(cards[index].currentSkill, index < cards.Count * 3 / 4, false);
    }


    public void OnBeginDrag(int index, RectTransform cardTransform)
    {
        SetDraggingCardIndex(index);
        cardTransform.localRotation = Quaternion.identity;
    }

    public void OnEndDrag(int index, RectTransform cardTransform)
    {
        
    }

    public void OnDrop(int index, RectTransform cardTransform)
    {
        SetDraggingCardIndex(-1);
    }

    public void OnUse(int index, RectTransform cardTransform)
    {
        StartCoroutine(UseSkillAfterDelay(isDraggingIndex, 0.5f));
    }

    private IEnumerator UseSkillAfterDelay(int index, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        cards[index].gameObject.SetActive(false);
        battle.currentState.myUnit.cardSystem.Use(index);
        battle.SetSkill(cards[index].currentSkill, true);

        SetDraggingCardIndex(-1);
    }

    private void SetDraggingCardIndex(int index)
    {
        if (index >= cards.Count)
            return;

        bool isDraggingCard = index >= 0;

        cardStayPanel.raycastTarget = isDraggingCard;
        cardUsePanel.raycastTarget = isDraggingCard;

        if (!isDraggingCard)
        {
            SetInfoPromptActive(false);

            isDraggingIndex = -1;
            foreach (var card in cards)
            {
                card?.SetRaycastTarget(true);
            }
            return;
        }

        isDraggingIndex = index;
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i]?.SetRaycastTarget(i == isDraggingIndex);
        }
    }

    
}

