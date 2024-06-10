using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopPetView : Module
{
    [SerializeField] private List<GameObject> petContentPages = new List<GameObject>();
    [SerializeField] private Panel petPreviewPanel, learnSkillPanel, learnBuffPanel, helpPanel;
    [SerializeField] private Text helpText;
    [SerializeField] private IButton iconButton, emblemButton, battleButton;
    [SerializeField] private GameObject skillButtonPrefab;
    [SerializeField] private RectTransform skillContentRect;
    [SerializeField] private Image petPreviewImage, emblemPreviewImage;

    private List<GameObject> skillPrefabList = new List<GameObject>();
    
    protected override void Awake() {
        petContentPages.ForEach(x => {
            x.SetActive(true);
            x.SetActive(false);
        });
    }

    public IButton GetButton(string type) {
        return type switch {
            "icon" => iconButton,
            "emblem" => emblemButton,
            "battle" => battleButton,
            _ => null,
        };
    }

    public Image GetPreviewImage(string type) {
        return type switch {
            "emblem" => emblemPreviewImage,
            "battle" => petPreviewImage,
            _ => null,
        };
    }

    public void SetPage(int page) {
        if (!page.IsInRange(0, petContentPages.Count))
            return;

        petContentPages.ForEach(x => x.SetActive(false));
        petContentPages[page].SetActive(true);
    }

    public void OpenLearnSkillPanel() {
        learnSkillPanel.SetActive(true);
    }

    public void OpenLearnBuffPanel() {
        learnBuffPanel.SetActive(true);
    }

    public void OpenHelpPanel(string type) {
        string help = type switch {
            "id" => "精灵的序号只能输入-13以下的数字",
            "subElement" => "精灵的副属性。单属性精灵请填写普通",
            "height" => "精灵的身高体重会有5点浮动值",
            "weight" => "精灵的身高体重会有5点浮动值",
            "baseStatus" => "介于0到255之间。能力值计算方式为\n" +
                "非体力：((种族值x2+个体值+(学习力/4))x(等级/100)+5)x性格修正\n" +
                "体力：((种族值x2+个体值+(学习力/4))x(等级/100)+10+等级\n",
            "description" => "精灵图鉴描述，无法换行也无法使文字上色",

            "baseId" => "该精灵最低型态的序号\n" +
                "例：波克尔的最低型态为皮皮",
            "evolveId" => "该精灵进化型态的序号\n" +
                "无进化型态请填0\n" +
                "若有分支进化请填写默认进化型态\n\n" +
                "例如：拉奥(ID:7)的进化型态是拉奥苗(ID:8)，因此拉奥(ID:7)的【进化序号】需要填写8",
            "evolveLevel" => "该精灵的进化等级\n" +
                "不是透过等级自然进化的请填0",
            "expType" => "升级到100级所需的经验总和",
            "skill" => "点击加号新增技能学习信息\n" +
                "点击减号删除最后一项信息\n\n" + 
                "进化型态精灵的技能只需填写新获得的技能\n" +
                "但是进化前型态的【进化序号】需要填写此精灵序号",
            "feature" => "需要先去印记那边制作特性与纹章印记\n" +
                "特性和纹章类buff的序号需要填写900000(纹章800000)+该精灵基础型态之序号（去除负号）\n\n" +
                "进化型态的精灵无须填写，沿用基础型态的特性和纹章",
            "emblem" => "需要先去印记那边制作特性与纹章印记\n"+
                "特性和纹章类buff的序号需要填写900000(纹章800000)+该精灵基础型态之序号（去除负号）\n\n" +
                "进化型态的精灵无须填写，沿用基础型态的特性和纹章",


            "iconSprite" => "点击格子上传图片，按下取消可以清除\n" +
                "请上传长宽比为1:1的png档案，不接受其他格式\n\n" + 
                "注意：档案越大或者DIY越多会导致图片加载时间变久",
            "emblemSprite" => "点击格子上传图片，按下取消可以清除\n" +
                "请上传长宽比为1:1的png档案，不接受其他格式\n\n" + 
                "注意：档案越大或者DIY越多会导致图片加载时间变久",
            "battleSprite" => "点击格子上传图片，按下取消可以清除\n" +
                "为了统一大小方便处理，请上传长宽比为16:9的png档案\n" + 
                "推荐大小960x540像素，图片需要透明背景\n" +
                "精灵的中心点大约落在以左方为基准的220像素位置\n" +
                "资料全部完成后可以按下预览查看（示意图仅供参考）\n\n" +
                "注意：档案越大或者DIY越多会导致图片加载时间变久",
            "skin" => "此功能暂不开放\n",
            "option" => "特殊自定义选项，选项如下请自行填写，多个选项请以 & 连接\n\n" +
                "更改默认皮肤：default_skin=精灵序号\n" +
                "更改默认特性：default_feature=基础序号",

            _ => string.Empty,
        };
        helpText?.SetText(help);
        helpPanel?.SetActive(true);
    }

    public void SetPetInfo(PetInfo petInfo) {
        skillPrefabList.ForEach(Destroy);
        skillPrefabList.Clear();

        var skillInfo = petInfo.skills;
        for (int i = 0; i < skillInfo.skillIdList.Count; i++) {
            var skill = Skill.GetSkill(skillInfo.skillIdList[i], false);
            if (skill == null)
                continue;

            var level = skillInfo.learnLevelList[i];
            var learnInfo = new LearnSkillInfo(skill, level);

            OnAddSkill(learnInfo);
        }

        OnUploadSprite("icon", petInfo.ui.icon);
        OnUploadSprite("emblem", petInfo.ui.emblemIcon);
        OnUploadSprite("battle", petInfo.ui.battleImage);
    }

    public void OnUploadSprite(string type, Sprite sprite) {
        GetButton(type)?.SetSprite(sprite);
        GetPreviewImage(type)?.SetSprite(sprite);
    }

    public void OnClearSprite(string type) {
        GetButton(type)?.SetSprite(SpriteSet.GetDefaultIconSprite(type != "battle"));
        GetPreviewImage(type)?.SetSprite(SpriteSet.GetDefaultIconSprite(type != "battle"));
    }

    public void OnAddSkill(LearnSkillInfo info) {
        var skillPrefab = Instantiate(skillButtonPrefab, skillContentRect);
        skillPrefab.GetComponent<IButton>()?.SetInteractable(false, false);
        skillPrefab.GetComponentInChildren<Text>()?.SetText(info.skill.name + " Lv " + info.value);
        skillPrefabList.Add(skillPrefab);
    }

    public void OnRemoveSkill() {
        if (skillPrefabList.Count == 0)
            return;

        Destroy(skillPrefabList.LastOrDefault());
        skillPrefabList.RemoveAt(skillPrefabList.Count - 1);
    }
    
    public void OnPreviewPet() {
        petPreviewPanel.SetActive(true);
    }
    
}
