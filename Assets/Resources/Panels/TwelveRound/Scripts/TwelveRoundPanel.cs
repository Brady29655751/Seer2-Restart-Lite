using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TwelveRoundPanel : Panel
{
    [SerializeField] private Image petImage;
    [SerializeField] private IText petNameText;
    [SerializeField] private Text roundText;
    [SerializeField] private GameObject challengeButton, finishMark;

    private Activity activity => Activity.Find("twelve_round");
    private int round => activity.GetData<int>($"round[{petId}]", "0");
    private bool isFinished => round >= 12;
    private string petName => Pet.GetPetInfo(petId)?.name ?? string.Empty;
    private Sprite petSprite => Pet.GetPetInfo(petId)?.ui.idleImage ?? SpriteSet.Empty;
    private TwelveRoundData data => GetData();

    private int petId = 0;
    private Dictionary<int, List<Item>> itemList = new Dictionary<int, List<Item>>();


    public override void SetPanelIdentifier(string id, string param)
    {
        if (id.TryTrimStart("item", out var trimId))
        {
            var rewardList = param.Split('/');
            for (int i = 0; i < rewardList.Length; i++)
            {
                var roundRewardExpr = rewardList[i];
                var roundReward = roundRewardExpr.Split(',');
                var roundRewardList = new List<Item>();

                foreach (var itemExpr in roundReward)
                {
                    var pIndex = itemExpr.IndexOf('[');
                    if (pIndex < 0)
                        roundRewardList.Add(new Item(int.Parse(itemExpr)));
                    else
                        roundRewardList.Add(new Item(int.Parse(itemExpr.Substring(0, pIndex)), int.Parse(itemExpr.TrimParentheses())));
                }

                if (trimId.TryTrimParentheses(out var itemIdExpr))
                    itemList.Set(int.Parse(itemIdExpr), roundRewardList);
                else
                    itemList.Set(i + 1, roundRewardList);
            }
            return;
        }

        switch (id)
        {
            default:
                base.SetPanelIdentifier(id, param);
                break;

            case "pet":
                SetPet(int.Parse(param));
                break;

            case "pos":
                SetPetImagePosition(param.ToVector2());
                break;

            case "size":
                SetPetImageSize(param.ToVector2());
                break;
        }
    }

    public TwelveRoundData GetData()
    {
        return new TwelveRoundData()
        {
            petId = petId,
            pos = petImage.rectTransform.anchoredPosition,
            size = petImage.rectTransform.sizeDelta,
            itemList = itemList.ToDictionary(entry => entry.Key, entry => entry.Value?.Select(x => (x == null) ? null : new Item(x)).ToList()),
        };
    }

    public void SetData(TwelveRoundData data)
    {
       SetPet(data.petId);
       SetPetImagePosition(data.pos);
       SetPetImageSize(data.size);
    }

    public void SetPet(int petId)
    {
        this.petId = petId;
        petImage.SetSprite(petSprite);
        petNameText.SetText(petName);
        roundText.SetText($"当前已完成 {round} 轮");

        finishMark.SetActive(isFinished);
        challengeButton.SetActive(!isFinished);
    }

    public void SetPetImagePosition(Vector2 pos)
    {
        petImage.rectTransform.anchoredPosition = pos;
    }

    public void SetPetImageSize(Vector2 size)
    {
        petImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        petImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    public void Challenge()
    {
        if (isFinished)
        {
            Hintbox.OpenHintboxWithContent("已完成12轮，无法继续挑战！", 16);
            return;
        }

        var bossInfo = BossInfo.GetRandomEnemyInfo(petId.SingleToList(), 60 + 5 * round);
        bossInfo.initBuffIds = "-8,-3,17,20";
        bossInfo.status = new BattleStatus(){ hp = 500 * (round + 1), };
        // bossInfo.loopSkillIds = bossInfo.loopSkills.TakeLast(5).Append(bossInfo.loopSkills.Get(3, bossInfo.loopSkills.First())).Select(x => x.id.ToString()).ConcatToString(",");

        var data = this.data;
        var battleInfo = new BattleInfo()
        {
            enemyInfo = bossInfo.SingleToList(),
            winHandler = NpcButtonHandler.Callback(OnChallengeWin).SingleToList(),
            loseHandler = NpcButtonHandler.Callback(OnChallengeLose).SingleToList(),
        };

        Battle.StartBattle(battleInfo);
    }
    
    private void OnChallengeWin()
    {
        var items = itemList.Get(round + 1, new Item(1, 500).SingleToList());
        foreach (var item in items)
        {
            Item.Add(item);
            Item.OpenHintbox(item);
        }

        activity.SetData($"round[{petId}]", round + 1);
        SaveSystem.SaveData();

        Hintbox.OpenHintboxWithContent($"恭喜完成第 <color=#ffbb33>{round}</color> 轮挑战！", 16);
    }

    private void OnChallengeLose()
    {
        
    }
}

public class TwelveRoundData
{
    public int petId;
    public Vector2 pos, size;
    public Dictionary<int, List<Item>> itemList = new Dictionary<int, List<Item>>();
}
