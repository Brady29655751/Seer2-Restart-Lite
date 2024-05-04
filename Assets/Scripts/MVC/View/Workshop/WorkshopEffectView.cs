using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopEffectView : Module
{
    [SerializeField] private Panel effectPanel, refSkillPanel, refBuffPanel, multiEffectPanel, helpPanel;
    [SerializeField] private Text helpText;
    [SerializeField] private RectTransform multiEffectContentRect;
    [SerializeField] private GameObject multiEffectButtonPrefab;

    private List<GameObject> multiEffectPrefabList = new List<GameObject>();

    public void SetEffectPanelActive(bool active) {
        effectPanel.SetActive(active);
    }

    public void SetRefSkillPanelActive(bool active) {
        refSkillPanel.SetActive(active);
    }

    public void SetRefBuffPanelActive(bool active) {
        refBuffPanel.SetActive(active);
    }

    public void SetMultiEffectPanelActive(bool active) {
        multiEffectPanel.SetActive(active);
    }

    public void SetMultiEffectList(List<Effect> multiEffectList, Action<int> callback) {
        multiEffectPrefabList.ForEach(Destroy);
        multiEffectPrefabList = multiEffectList.Select((x, i) => {
            int copy = i;
            var obj = Instantiate(multiEffectButtonPrefab, multiEffectContentRect);
            obj.GetComponent<IButton>()?.onPointerClickEvent.SetListener(() => callback?.Invoke(copy));
            obj.GetComponentInChildren<Text>()?.SetText(multiEffectList[copy].abilityOptionDict.Get("name", "效果 " + (copy + 1)));
            return obj;
        }).ToList();
    }

    public void OpenHelpPanel(string type) {
        string help = type switch {
            "priority"  => "效果结算顺序\n" 
                + "数字越小越优先",

            "condition" => "点击加号添加新细节\n" +
                "点击减号移除所有细节\n" +
                "多项条件细节须全部达成才会触发此效果",

            "conditionOption" => "不熟悉参数配置的人建议使用左下方的\n" + 
                "【参考现有技能】或【参考现有印记】\n" +
                "再自行微调参数",

            "ability" => "点击加号添加新细节\n" +
                "点击减号移除所有细节",

            "abilityOption" => "不熟悉参数配置的人建议使用左下方的\n" + 
                "【参考现有技能】或【参考现有印记】\n" +
                "再自行微调参数",

            _ => string.Empty,
        };
        helpText?.SetText(help);
        helpPanel?.SetActive(true);
    }

    public void SetAbility(EffectAbility ability, int abilityDetail) {
        
    }
}
