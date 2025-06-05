using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopItemView : Module
{
    [SerializeField] private Panel effectPanel, helpPanel;
    [SerializeField] private Text helpText;
    [SerializeField] private IButton iconButton;
    [SerializeField] private GameObject effectButtonPrefab;
    [SerializeField] private RectTransform effectContentRect;

    private List<GameObject> effectPrefabList = new List<GameObject>();

    public void OpenEffectPanel() {
        effectPanel.SetActive(true);
    }

    public void OpenHelpPanel(string type) {
        string help = type switch {
            "id" => "道具的序号只能输入负数",

            "price" => "此道具所需的「交易货币」数量，即购买和售出价格\n" +
                "例如：此道具价值200星钻，则填写200",

            "currency" => "此道具所需的「交易货币」种类，需填写该货币的道具序号\n" +
                "例1：此道具价值500赛尔豆，则填写赛尔豆的道具序号（1）\n" +
                "例2：此道具价值200星钻，则填写星钻的道具序号（2）\n" +
                "例3：此道具价值「6个序号为-100的道具」，则填写-100",

            "icon" => "点击左方格子上传图片，按下取消可以清除\n" +
                "请上传长宽比为1:1的png档案，不接受其他格式\n\n" +
                "若想使用其他图案，可以在【其他】填写「resId=现有路径」\n" +
                "例1：resId=10\n" +
                "例2：resId=Buffs/-100001\n" +
                "例1会直接指向对应道具序号的图标\n" +
                "注意：档案越大或者DIY越多会导致图片加载时间变久",

            "effect" => "点击加号添加新效果\n" +
                "点击减号移除最后一项效果",

            "description" => "若想替描述上色，请在上色文字左右两侧分别加上[色码(小写)]和[-]\n" +
                "例如：[ffbb33]上色文字[-]\n\n" +
                "道具描述是该道具的故事介绍，效果描述是该道具的效果介绍",

            "option" => "特殊自定义选项，选项如下请自行填写，多个选项请以 & 连接\n\n" +
                "可重复使用：removable=false\n" +
                "获得此道具时改为获得另一道具：getId=另一道具序号\n" +
                "道具列表点击获得：linkId=Workshop",

            _ => string.Empty,
        };
        helpText?.SetText(help);
        helpPanel?.SetActive(true);
    }

    public void SetItemInfo(ItemInfo itemInfo, Action<int> effectCallback) {
        effectPrefabList.ForEach(Destroy);
        effectPrefabList.Clear();
        foreach (var effect in itemInfo.effects)
            OnAddEffect(effect, effectCallback);
        
        OnUploadIcon(itemInfo.icon);
    }

    public void OnUploadIcon(Sprite sprite) {
        iconButton?.SetSprite(sprite);
    }

    public void OnClearIcon() {
        iconButton?.SetSprite(SpriteSet.GetDefaultIconSprite(true));
    }

    public void OnAddEffect(Effect effect, Action<int> callback = null) {
        var index = effectPrefabList.Count;
        var effectPrefab = Instantiate(effectButtonPrefab, effectContentRect);
        effectPrefab.GetComponent<IButton>()?.onPointerClickEvent.SetListener(() => callback?.Invoke(index));
        effectPrefab.GetComponentInChildren<Text>()?.SetText(effect.abilityOptionDict.Get("name", "效果 " + (index + 1)));
        effectPrefabList.Add(effectPrefab);
    }

    public void OnRemoveEffect() {
        if (effectPrefabList.Count == 0)
            return;
            
        Destroy(effectPrefabList.LastOrDefault());
        effectPrefabList.RemoveAt(effectPrefabList.Count - 1);
    }

    public void OnEditEffect(int index, Effect effect) {
        if (!index.IsInRange(0, effectPrefabList.Count))
            return;

        effectPrefabList[index].GetComponentInChildren<Text>()?.SetText(effect.abilityOptionDict.Get("name", "效果 " + (index + 1)));
    }
}
