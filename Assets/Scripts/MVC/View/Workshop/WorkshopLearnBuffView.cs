using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopLearnBuffView : UIModule
{
    [SerializeField] private BattlePetBuffBlockView buffBlockView;
    [SerializeField] private RectTransform selectBuffContentRect;
    [SerializeField] private GameObject selectBuffButtonPrefab;

    private List<GameObject> selectBuffButtonPrefabList = new List<GameObject>();

    public void SetSelections(List<BuffInfo> selections, Action<int> callback) {
        selectBuffButtonPrefabList.ForEach(Destroy);
        selectBuffButtonPrefabList = selections.Select((x, i) => { 
            int copy = i;
            var obj = Instantiate(selectBuffButtonPrefab, selectBuffContentRect);
            obj.GetComponent<IButton>()?.onPointerClickEvent.SetListener(() => callback?.Invoke(copy));
            obj.GetComponentInChildren<Text>()?.SetText(selections[copy].id + "/" + selections[copy].name);
            return obj;
        }).ToList();

        // Clear current preview
        OnPreviewBuff(null);
    }

    public void OnPreviewBuff(BuffInfo buffInfo) {
        var buff = (buffInfo == null) ? null : new Buff(buffInfo.id);
        buffBlockView.SetBuff(buff, () => infoPrompt.SetActive(true), () => infoPrompt.SetActive(false),
            () => infoPrompt.SetBuff(buff, false));
    }

}
