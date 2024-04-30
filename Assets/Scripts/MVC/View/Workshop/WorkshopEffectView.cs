using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopEffectView : Module
{
    [SerializeField] private Panel effectPanel, helpPanel;
    [SerializeField] private Text helpText;
    [SerializeField] private GameObject detailButtonPrefab;
    [SerializeField] private RectTransform condtionContentRect, abilityContentRect;
    private List<GameObject> conditionPrefabList = new List<GameObject>();
    private List<GameObject> abilityPrefabList = new List<GameObject>();

    public void SetEffectPanelActive(bool active) {
        effectPanel.SetActive(active);
    }

    public void OpenHelpPanel(string type) {
        string help = type switch {
            "probability" => "效果发动概率",

            "condition" => "点击加号添加新细节\n" +
                "点击减号移除所有细节\n" +
                "多项条件细节须全部达成才会触发此效果",

            "conditionOption" => "使用自订并不会进行详细检测\n" +
                "若产生错误将难以找出源头\n" +
                "不熟悉代码的人请勿使用",

            "ability" => "点击加号添加新细节\n" +
                "点击减号移除所有细节",

            "abilityOption" => "使用自订并不会进行详细检测\n" +
                "若产生错误将难以找出源头\n" +
                "不熟悉代码的人请勿使用",

            _ => string.Empty,
        };
        helpText?.SetText(help);
        helpPanel?.SetActive(true);
    }

    public void OnRemoveConditionOptions() {
        conditionPrefabList.ForEach(Destroy);
        conditionPrefabList.Clear();
    }

    public void OnRemoveAbilityOptions() {
        abilityPrefabList.ForEach(Destroy);
        abilityPrefabList.Clear();
    }

    public void SetAbility(EffectAbility ability, int abilityDetail) {
        
    }
}
