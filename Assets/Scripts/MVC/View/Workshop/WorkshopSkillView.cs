using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopSkillView : Module
{
    [SerializeField] private Panel effectPanel, helpPanel;
    [SerializeField] private Text helpText;
    [SerializeField] private GameObject effectButtonPrefab;
    [SerializeField] private RectTransform effectContentRect;

    private List<GameObject> effectPrefabList = new List<GameObject>();

    public void OpenEffectPanel() {
        effectPanel.SetActive(true);
    }

    public void OpenHelpPanel(string type) {
        string help = type switch {
            "id" => "技能的序号只能输入-10001以下的数字",

            "accuracy" => "技能命中率，单位为%，填写95即为95%\n" + 
                "必中为基础命中率再加上1000",

            "effect" => "点击加号添加新效果\n" +
                "点击减号移除最后一项效果",

            "description" => "若想替描述上色，请在上色文字左右两侧分别加上[色码(小写)]和[-]\n" + 
                "例如：[ffbb33]上色文字[-]\n\n" +
                "按下预览按钮可以预览buff描述",

            "option" => "特殊自定义选项，选项如下请自行填写，多个选项请以 & 连接\n\n" +
                "参照BUFF描述：ref_buff=序号[数值]/序号[数值]...\n" +
                "暴击率：critical=数值(默认为5%)\n" +
                "无视护盾：ignore_shield=true\n" + 
                "无视能力变化：ignore_powerup=true\n\n" +
                "例1：暴击率35%且無視護盾，critical=35&ignore_shield=true\n" +
                "例2：参照流血（30点）和魅惑描述，ref_buff=1004[30]/101",

            _ => string.Empty,
        };
        helpText?.SetText(help);
        helpPanel?.SetActive(true);
    }

    public void SetSkill(Skill skill, Action<int> effectCallback) {
        effectPrefabList.ForEach(Destroy);
        effectPrefabList.Clear();
        foreach (var effect in skill.effects)
            OnAddEffect(effect, effectCallback);
    }

    public void OnAddEffect(Effect effect, Action<int> callback) {
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
