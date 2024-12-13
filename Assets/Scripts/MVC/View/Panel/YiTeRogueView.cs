using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YiTeRogueView : UIModule
{
    [SerializeField] private float eventBlockScale = 1.8f;
    [SerializeField] private ScrollRect rogueScrollRect;
    [SerializeField] private RectTransform rogueContentRect;
    [SerializeField] private BattlePetBuffView buffView;
    [SerializeField] private GameObject buffButtonPrefab, arrowPrefab;
    [SerializeField] private List<GameObject> arrowList = new List<GameObject>();
    [SerializeField] private List<BattlePetBuffBlockView> eventButtonList = new List<BattlePetBuffBlockView>();

    [SerializeField] private GameObject choicePanel;
    [SerializeField] private Text titleText, contentText;
    [SerializeField] private List<YiTeRogueChoiceView> choiceList;

    public void Clear() {
        arrowList.ForEach(x => Destroy(x.gameObject));
        eventButtonList.ForEach(x => Destroy(x.gameObject));

        arrowList.Clear();
        eventButtonList.Clear();
    }

    public void SetMap() {
        Clear();

        var eventMap = YiTeRogueData.instance.eventMap;
        var isNextPosLocked = YiTeRogueData.instance.isNextPosLocked;
        var nextPos = YiTeRogueData.instance.nextPos;
        var floor = YiTeRogueData.instance.floor;
        var trace = YiTeRogueData.instance.trace;
        YiTeRogueEvent rogueEvent = eventMap[0];

        rogueContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120 + 120 * eventMap[eventMap.Count - 1].step);
        rogueScrollRect.horizontalNormalizedPosition = trace.Count * 1f / YiTeRogueEvent.GetEndStepByFloor(floor);

        // Instantiate first block.
        InstantiateEventBlock(rogueEvent, trace);
        if (trace.Count == 0)
            return;

        // Instantiate blocks in the middle.
        for (int i = 0; i < trace.Count - 1; i++) {
            int delta = trace[i + 1] - trace[i];
            InstantiateEventArrows(rogueEvent, new List<int>(){ delta });
            rogueEvent = eventMap.Find(x => (x.step == i + 1) && (x.pos == trace[i + 1]));
            InstantiateEventBlock(rogueEvent, trace);
        }

        if (rogueEvent?.next == null)
            return;

        // Instantiate last blocks for chosen.
        var chosenNext = isNextPosLocked ? new List<int>(){ nextPos - rogueEvent.pos } : rogueEvent.next;
        InstantiateEventArrows(rogueEvent, chosenNext);
        var currentEventList = eventMap.FindAll(x => (x.step == rogueEvent.step + 1) && 
            (chosenNext.Exists(y => x.pos == rogueEvent.pos + y)));

        foreach (var e in currentEventList)
            InstantiateEventBlock(e, trace);
    }

    private void InstantiateEventBlock(YiTeRogueEvent rogueEvent, List<int> trace) {
        if (rogueEvent == null)
            return;

        var buff = new Buff(rogueEvent.eventIconBuffId);
        var buffBlock = Instantiate(buffButtonPrefab, rogueContentRect);
        var blockView = buffBlock.GetComponent<BattlePetBuffBlockView>();
        var rect = buffBlock.GetComponent<RectTransform>();
        
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up; 
        rect.anchoredPosition = new Vector2(35 + 120 * rogueEvent.step, -170 - 80 * rogueEvent.pos);
        rect.localScale = eventBlockScale * Vector3.one;
        blockView.SetBuff(buff, () => SetInfoPromptActive(true), () => SetInfoPromptActive(false), 
            () => infoPrompt.SetBuff(buff), () => {
                if (trace.Count == rogueEvent.step) {
                    YiTeRogueData.instance.Click(rogueEvent.pos);
                    OpenChoicePanel(rogueEvent);
                } else
                    Hintbox.OpenHintboxWithContent("此事件已完成！", 16);
        });
        eventButtonList.Add(blockView);
    }

    private void InstantiateEventArrows(YiTeRogueEvent rogueEvent, List<int> nextList) {
        if ((rogueEvent == null) || (nextList == null))
            return;

        foreach (var delta in nextList) {
            var arrowBlock = Instantiate(arrowPrefab, rogueContentRect);
            var arrowX = ((delta == -1) ? 125 : 145) + 120 * rogueEvent.step;
            var arrowY = ((delta == -1) ? -130 : ((delta == 0) ? -185 : -250)) - 80 * rogueEvent.pos;
            var rect = arrowBlock.GetComponent<RectTransform>();
            rect.rotation = Quaternion.Euler(0, 0, -90 - 45 * delta);
            rect.anchoredPosition = new Vector2(arrowX, arrowY);
            arrowList.Add(arrowBlock);
        }
    }

    public void OpenChoicePanel(YiTeRogueEvent rogueEvent, bool withStep = true) {
        choicePanel.SetActive(true);
        titleText?.SetText(rogueEvent.title);
        contentText?.SetText(rogueEvent.content);
        var choices = rogueEvent.choiceList;
        for (int i = 0; i < choiceList.Count; i++) {
            choiceList[i].SetChoice((i < choices.Count) ? choices[i] : null, SetMap, rogueEvent.pos, withStep);
        }
    }

    public void CloseChoicePanel() {
        SetMap();
        choicePanel.SetActive(false);
    }

    public void ToggleBuffPanel() {
        var isBuffPanelActive = buffView.gameObject.activeSelf;
        rogueScrollRect.gameObject.SetActive(isBuffPanelActive);
        buffView.gameObject.SetActive(!isBuffPanelActive);
    }
}
