using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YiTeRogueView : UIModule
{
    [SerializeField] private float eventBlockScale = 1.8f;
    [SerializeField] private RectTransform rogueContentRect;
    [SerializeField] private GameObject buffButtonPrefab, arrowPrefab;
    [SerializeField] private List<GameObject> arrowList = new List<GameObject>();
    [SerializeField] private List<BattlePetBuffBlockView> eventButtonList = new List<BattlePetBuffBlockView>();

    public void SetMap(List<YiTeRogueEvent> eventMap, List<int> trace) {
        rogueContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 120 + 120 * eventMap[eventMap.Count - 1].step);

        for (int i = 0; i < eventMap.Count; i++) {
            var rogueEvent = eventMap[i];

            // Instantiate event block
            var buff = new Buff(rogueEvent.eventIconBuffId);
            var buffBlock = Instantiate(buffButtonPrefab, rogueContentRect);
            var blockView = buffBlock.GetComponent<BattlePetBuffBlockView>();
            RectTransform rect = buffBlock.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up; 
            rect.anchoredPosition = new Vector2(35 + 120 * rogueEvent.step, -170 - 80 * rogueEvent.pos);
            rect.localScale = eventBlockScale * Vector3.one;
            blockView.SetBuff(buff, () => SetInfoPromptActive(true), () => SetInfoPromptActive(false), 
                () => infoPrompt.SetBuff(buff), rogueEvent.Trigger);
            eventButtonList.Add(blockView);

            // Instantiate arrow.
            if (rogueEvent.next != null) {
                foreach (var delta in rogueEvent.next) {
                    var arrowBlock = Instantiate(arrowPrefab, rogueContentRect);
                    var arrowX = ((delta == -1) ? 125 : 145) + 120 * rogueEvent.step;
                    var arrowY = ((delta == -1) ? -130 : ((delta == 0) ? -185 : -250)) - 80 * rogueEvent.pos;
                    rect = arrowBlock.GetComponent<RectTransform>();
                    rect.rotation = Quaternion.Euler(0, 0, -90 - 45 * delta);
                    rect.anchoredPosition = new Vector2(arrowX, arrowY);
                    arrowList.Add(arrowBlock);
                }
            }
        }
    }
}
