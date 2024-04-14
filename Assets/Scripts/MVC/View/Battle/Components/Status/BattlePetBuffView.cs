using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePetBuffView : BattleBaseView
{
    [SerializeField] private bool anchoredAtLeft;
    [SerializeField] private GameObject buffPanel;
    [SerializeField] private GameObject buffButtonPrefab;

    private int numInOneRow => 7;
    private List<Buff> buffList = new List<Buff>();
    private List<BattlePetBuffBlockView> buffButtonList = new List<BattlePetBuffBlockView>();

    public void SetBuff(List<Buff> buffs) {
        List<Buff> newBuffList = buffs.Where(x => !x.info.hide).OrderBy(x => x.info.sortPriority).ToList();
        int diffLength = newBuffList.Count - buffList.Count;
        if (diffLength > 0) {
            AddBuffBlocks(diffLength);
        } else if (diffLength < 0) {
            RemoveBuffBlocks(diffLength);
        }
        buffList = newBuffList;
        SetBuffBlocks();
    }

    private void AddBuffBlocks(int num) {
        int deltaX = (anchoredAtLeft ? 1 : -1) * 32;
        int deltaY = -32;
        for (int i = 0; i < num; i++) {
            int col = (buffList.Count + i) / numInOneRow;
            int row = (buffList.Count + i) % numInOneRow;
            GameObject buffBlock = Instantiate(buffButtonPrefab, buffPanel.transform);
            BattlePetBuffBlockView blockView = buffBlock.GetComponent<BattlePetBuffBlockView>();
            RectTransform rect = buffBlock.GetComponent<RectTransform>();
            rect.SetAsLastSibling();
            rect.anchorMin = rect.anchorMax = rect.pivot = anchoredAtLeft ? Vector2.up : Vector2.one; 
            rect.anchoredPosition = new Vector2(0 + deltaX * row, 0 + deltaY * col);
            buffButtonList.Add(blockView);
        }
    }

    private void RemoveBuffBlocks(int num) {
        num = Mathf.Abs(num);
        for (int i = 0; i < num; i++) {
            Destroy(buffButtonList[buffList.Count - i - 1].gameObject);
        }
        buffButtonList.RemoveRange(buffList.Count - num, num);
    }

    private void SetBuffBlocks() {
        for (int i = 0; i < buffList.Count; i++) {
            int copy = i;
            Action onPointerOver = () => OnPointerOver(copy);
            buffButtonList[i].SetBuff(buffList[i], onPointerEnter, OnPointerExit, onPointerOver.Invoke);
        }
    }

    private void onPointerEnter() {
        SetInfoPromptActive(true);
    }

    private void OnPointerOver(int index) {
        var buff = buffList[index];
        string header = "<size=18><color=#52e5f9>" + buff.name + "</color></size><size=4>\n\n</size>";
        string text = buff.description;
        Vector2 size = text.GetPreferredSize(15, 14, 21, 21 + 40);
        Vector2 fixPos = anchoredAtLeft ? new Vector2(12, -size.y + 24) : new Vector2(-size.x - 2, -size.y + 24);
        infoPrompt.SetInfoPrompt(size, header + text, fixPos, TextAnchor.MiddleLeft);
    }

    private void OnPointerExit() {
        SetInfoPromptActive(false);
    }

}
