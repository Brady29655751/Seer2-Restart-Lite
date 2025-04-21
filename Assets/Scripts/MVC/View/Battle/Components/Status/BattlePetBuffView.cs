using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePetBuffView : BattleBaseView
{
    [SerializeField] private bool anchoredAtLeft;
    [SerializeField] private bool postionTranspose = false;
    [SerializeField] private bool isExtendMode = true;   // "Currently" buffs are extended or not.
    [SerializeField] private int numInOneRow = 7;
    [SerializeField] private int numLimitExtend = 7;
    [SerializeField] private int buffBlockSize = 32;
    [SerializeField] private GameObject buffPanel;
    [SerializeField] private GameObject buffButtonPrefab;

    private List<Buff> buffList = new List<Buff>();
    private List<BattlePetBuffBlockView> buffButtonList = new List<BattlePetBuffBlockView>();

    public void SetExtendMode(bool active) {
        if (active) {
            for (int i = 0; i < buffButtonList.Count; i++) {
                int copy = i;
                buffButtonList[i].gameObject.SetActive(true);
                buffButtonList[i].rectTransform.anchoredPosition = GetBuffBlockAnchoredPosition(copy);

                if ((i >= numLimitExtend) && (i == buffButtonList.Count - 1)) {
                    Action onPointerOver = () => OnPointerOver(copy);
                    Action onPointerClick = () => SetExtendMode(false);
                    buffButtonList[i].rectTransform.anchoredPosition = GetBuffBlockAnchoredPosition(copy);
                    buffButtonList[i].SetBuff(Buff.GetExtendUIBuff(false), onPointerEnter, OnPointerExit, onPointerOver.Invoke, onPointerClick.Invoke);
                }
            }
        } else {
            for (int i = 0; i < buffButtonList.Count; i++) {
                int copy = i;
                buffButtonList[i].gameObject.SetActive((i < numLimitExtend - 1) || (i == buffButtonList.Count - 1));
                buffButtonList[i].rectTransform.anchoredPosition = GetBuffBlockAnchoredPosition(copy);

                if ((i >= numLimitExtend) && (i == buffButtonList.Count - 1)) {
                    Action onPointerOver = () => OnPointerOver(copy);
                    Action onPointerClick = () => SetExtendMode(true);
                    buffButtonList[i].rectTransform.anchoredPosition = GetBuffBlockAnchoredPosition(numLimitExtend - 1);
                    buffButtonList[i].SetBuff(Buff.GetExtendUIBuff(true), onPointerEnter, OnPointerExit, onPointerOver.Invoke, onPointerClick.Invoke);
                }
            }
        }
        isExtendMode = active;
    }

    public void SetBuff(List<Buff> buffs, Action<Buff> onPointerClick = null) {
        List<Buff> newBuffList = buffs.Where(x => !x.info.hide).OrderBy(x => x.info.sortPriority).ToList();
        if (newBuffList.Count >= numLimitExtend)
            newBuffList.Add(Buff.GetExtendUIBuff(!isExtendMode));

        int diffLength = newBuffList.Count - buffList.Count;
        if (diffLength > 0)
            AddBuffBlocks(diffLength);
        else if (diffLength < 0)
            RemoveBuffBlocks(diffLength);

        buffList = newBuffList;
        SetBuffBlocks(onPointerClick);
        SetExtendMode(isExtendMode);
    }

    private Vector2 GetBuffBlockAnchoredPosition(int index) {
        int deltaX = (anchoredAtLeft ? 1 : -1) * buffBlockSize;
        int deltaY = -buffBlockSize;
        int col = index / numInOneRow;
        int row = index % numInOneRow;
        var posX = postionTranspose ? col : row;
        var posY = postionTranspose ? row : col;
        return new Vector2(0 + deltaX * posX, 0 + deltaY * posY);
    }

    private void AddBuffBlocks(int num) {
        int deltaX = (anchoredAtLeft ? 1 : -1) * buffBlockSize;
        int deltaY = -buffBlockSize;
        for (int i = 0; i < num; i++) {
            int col = (buffList.Count + i) / numInOneRow;
            int row = (buffList.Count + i) % numInOneRow;
            var posX = postionTranspose ? col : row;
            var posY = postionTranspose ? row : col;
            GameObject buffBlock = Instantiate(buffButtonPrefab, buffPanel.transform);
            BattlePetBuffBlockView blockView = buffBlock.GetComponent<BattlePetBuffBlockView>();
            RectTransform rect = buffBlock.GetComponent<RectTransform>();
            rect.SetAsLastSibling();
            rect.anchorMin = rect.anchorMax = rect.pivot = anchoredAtLeft ? Vector2.up : Vector2.one; 
            rect.anchoredPosition = new Vector2(0 + deltaX * posX, 0 + deltaY * posY);
            rect.localScale = (buffBlockSize / 32f) * Vector3.one;
            buffButtonList.Add(blockView);
        }
    }

    private void RemoveBuffBlocks(int num) {
        num = Mathf.Abs(num);
        for (int i = 0; i < num; i++)
            Destroy(buffButtonList[buffList.Count - i - 1].gameObject);
        
        buffButtonList.RemoveRange(buffList.Count - num, num);
    }

    private void SetBuffBlocks(Action<Buff> OnPointerClick) {
        for (int i = 0; i < buffList.Count; i++) {
            int copy = i;
            Action onPointerOver = () => OnPointerOver(copy);
            Action onPointerClick = () => {
                var buffId = buffList[copy]?.id ?? 0;
                if (buffId == Buff.GetExtendUIBuffId(true))
                    SetExtendMode(true);
                else if (buffId == Buff.GetExtendUIBuffId(false))
                    SetExtendMode(false);
                else
                    OnPointerClick?.Invoke(buffList[copy]);
            };
            buffButtonList[i].SetBuff(buffList[i], onPointerEnter, OnPointerExit, onPointerOver.Invoke, onPointerClick.Invoke);
        }
    }

    private void onPointerEnter() {
        SetInfoPromptActive(true);
    }

    private void OnPointerOver(int index) {
        var buff = buffList[index];
        var ignoreHint = buff.ignore ? "（失效中）" : string.Empty;
        string header = "<size=18><color=#52e5f9>" + buff.name + ignoreHint + "</color></size><size=4>\n\n</size>";
        string text = buff.description;
        Vector2 size = text.GetPreferredSize(15, 14, 21, 21 + 40);
        Vector2 fixPos = anchoredAtLeft ? new Vector2(12, -size.y + 24) : new Vector2(-size.x - 2, -size.y + 24);
        infoPrompt.SetInfoPrompt(size, header + text, fixPos, TextAnchor.MiddleLeft);
    }

    private void OnPointerExit() {
        SetInfoPromptActive(false);
    }

}
