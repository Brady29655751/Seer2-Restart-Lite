using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopBuffView : Module
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
            "id" => "BUFF的序号只能输入-100001以下的数字(特性纹章除外)\n" + 
            "特性和纹章类buff的序号需要填写900000(纹章800000)+该精灵基础型态之序号（去除负号）\n" +
            "例如：波克尔的序号为 -12，基础型态(皮皮)的序号为 -10，\n" + 
            "因此皮皮的特性buff需要填写900010，纹章buff需要填写800010\n",
            
            "copy" => "打上同序号印记时的处理方式\n" + 
                "特性、纹章类的覆盖必须是【覆盖旧印记】",

            "turn" => "BUFF预设回合数，输入负数表示印记永久存在\n" + 
                "技能或其他效果打上印记时若未指定回合数，则会默认为预设回合数",

            "icon" => "点击左方格子上传图片，按下取消可以清除\n" +
                "请上传长宽比为1:1的png档案，不接受其他格式\n\n" + 
                "若想使用其他buff图案，可以在【其他】填写「res=现有序号」例如：res=10\n" +
                "特性和纹章类buff无须上传图片，但序号需要填写900000(纹章800000)+该精灵基础型态之序号（去除负号）\n" +
                "例如：波克尔的序号为 -12，基础型态(皮皮)的序号为 -10，\n" + 
                "因此皮皮的特性buff需要填写900010，纹章buff需要填写800010\n" +
                "注意：档案越大或者DIY越多会导致游戏初始加载时间变久",

            "effect" => "点击加号添加新效果\n" +
                "点击减号移除最后一项效果",

            "description" => "若想替描述上色，请在上色文字左右两侧分别加上[色码(小写)]和[-]\n" + 
                "例如：[ffbb33]上色文字[-]\n\n" +
                "若想在描述中显示buff当前的数值，请以[value]替代\n" + 
                "例如：记号当前为[value]个\n\n" + 
                "按下预览按钮可以预览buff描述",

            "option" => "特殊自定义选项，选项如下请自行填写，多个选项请以 & 连接\n\n" +
                "隐藏不显示：hide=true  若buff隐藏则表示只能透过自己清除\n" +
                "数值最大值：max_val=数值\n" + 
                "数值最小值：min_val=数值\n\n" +
                "例1：弱点记号最多叠加2个，因此 max_val=2\n" +
                "例2：hide=true&max_val=3\n" + 
                "例2通常用于库奇鲁的穿击波(或类似机制)之技能",

            _ => string.Empty,
        };
        helpText?.SetText(help);
        helpPanel?.SetActive(true);
    }

    public void OnUploadIcon(Sprite sprite) {
        iconButton?.SetSprite(sprite);
    }

    public void OnClearIcon() {
        iconButton?.SetSprite(SpriteSet.GetDefaultIconSprite(true));
    }

    public void OnAddEffect(Effect effect) {
        var effectPrefab = Instantiate(effectButtonPrefab, effectContentRect);
        effectPrefab.GetComponent<IButton>()?.SetInteractable(false, false);
        effectPrefab.GetComponentInChildren<Text>()?.SetText(effect.abilityOptionDict.Get("name", "效果 " + (effectPrefabList.Count + 1)));
        effectPrefabList.Add(effectPrefab);
    }

    public void OnRemoveEffect() {
        if (effectPrefabList.Count == 0)
            return;
            
        Destroy(effectPrefabList.LastOrDefault());
        effectPrefabList.RemoveAt(effectPrefabList.Count - 1);
    }
}
