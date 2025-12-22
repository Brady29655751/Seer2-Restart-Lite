using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopLearnBuffView : UIModule
{
    [SerializeField] private PageMode mode = PageMode.Scroll;
    [SerializeField] private BattlePetBuffBlockView buffBlockView;
    [SerializeField] private RectTransform selectBuffContentRect;
    [SerializeField] private GameObject selectBuffButtonPrefab;
    [SerializeField] private List<IButton> selectBuffButtonList = new List<IButton>();

    public void SetSelections(BuffInfo[] selections, Action<int> callback) {
        if (mode == PageMode.Scroll)
        {
            selectBuffButtonList.ForEach(x => Destroy(x.gameObject));
            selectBuffButtonList = selections.Select((x, i) => { 
                int copy = i;
                var obj = Instantiate(selectBuffButtonPrefab, selectBuffContentRect);
                var btn = obj?.GetComponent<IButton>();

                btn?.onPointerClickEvent.SetListener(() => callback?.Invoke(copy));
                obj?.GetComponentInChildren<Text>()?.SetText(selections[copy].id + "/" + selections[copy].name);

                return btn;
            }).ToList();   
        } 
        else
        {
            selectBuffButtonList.ForEach((x, i) => {
                int copy = i;
                var isNull = selections.Get(copy) == null;
                
                x.gameObject.SetActive(!isNull);
                if (isNull)
                    return;   
                
                x.onPointerClickEvent.SetListener(() => callback?.Invoke(copy));
                x.GetComponentInChildren<Text>()?.SetText(selections[copy].id + "/" + selections[copy].name);
            });
        }

        // Clear current preview
        OnPreviewBuff(null);
    }

    public void OnPreviewBuff(BuffInfo buffInfo) {
        var buff = (buffInfo == null) ? null : new Buff(buffInfo.id);
        buffBlockView.SetBuff(buff, () => infoPrompt.SetActive(true), () => infoPrompt.SetActive(false),
            () => 
            {
                infoPrompt.SetBuff(buff, false);

                Vector2 fixPos = new Vector2(-infoPrompt.size.x - 2, -infoPrompt.size.y / 3 + 24);
                infoPrompt.SetPositionOffset(fixPos);
            });
    }

}
