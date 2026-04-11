using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LotteryPanel : Panel
{
    [SerializeField] private IText deadlineText, alreadyBuyText;
    [SerializeField] private IButton buyTicketButton, getLastRewardButton;
    [SerializeField] private Image prizeImage;
    [SerializeField] private PetSelectController lastJackpot, lastBuy, thisCandidate, thisBuy;
    [SerializeField] private List<Sprite> rewardTypeSprites;

    public Activity Lottery => Activity.Find("anniversary_2");
    private List<int> candidates => new List<int>(){ 421, 423, 238, 250, 1220 };
    private List<List<Item>> rewardItemList => new List<List<Item>>()
    {
        new List<Item>() { new Item(2, 2026), new Item(4, 3) },
        new List<Item>() { new Item(500000, 2), new Item(10207, 5), new Item(10209, 1), new Item(10239, 5), new Item(10240, 1) },
        new List<Item>() { new Item(10301, 5), new Item(10018, 5), new Item(10203, 10), new Item(10005, 1) },
        new List<Item>() { new Item(1, 5000), new Item(10223, 5), new Item(10201, 10) },
        new List<Item>() { new Item(1, 2000), new Item(10234, 2) },
    };

    protected override void Awake()
    {
        base.Awake();
        
        lastJackpot.onSetStorageEvent += lastJackpot.RefreshView;
        lastBuy.onSetStorageEvent += lastBuy.RefreshView;
        thisCandidate.onSetStorageEvent += thisCandidate.RefreshView;
        thisBuy.onSetStorageEvent += thisBuy.RefreshView;

        thisCandidate.onSelectPetEvent += SelectCandidatePet;;
    }

    public override void Init()
    {
        base.Init();
        SetDeadlineText();
        DecideJackpot();
        ShowRewardSituation();
        ShowBuySituation();
    }

    private void SetDeadlineText()
    {
        var ts = DateTime.Today.Date.AddDays(1) - DateTime.Now;
        deadlineText?.SetText($"{ts.Hours:D2}小时{ts.Minutes:D2}分钟");
    }

    private void DecideJackpot()
    {
        var lastJackpotTime = Lottery.GetData("jackpotTime", DateTime.MinValue);
        var thisBuyTime = Lottery.GetData("thisBuyTime", DateTime.MinValue);

        if ((lastJackpotTime == DateTime.MinValue) && (thisBuyTime == DateTime.MinValue))
        {
            // 没有购买过，什么都不做
            lastJackpot?.SetStorage(candidates.Select(x => Pet.GetExamplePet(x)).ToList());
            return;
        }

        if ((lastJackpotTime <DateTime.Today) && (thisBuyTime.Date <DateTime.Today))
        {
            // 之前购买的彩票，开奖！
            var thisJackpotTicket = candidates.Random(5);
            Lottery.SetData("jackpot", thisJackpotTicket.Select(x => x.ToString()).ConcatToString("/"));
            Lottery.SetData("jackpotTime",DateTime.Today);
            Lottery.SetData("lastBuy", Lottery.GetData("thisBuy", string.Empty));
            Lottery.SetData("lastBuyTime", thisBuyTime);
            Lottery.SetData("thisBuy", string.Empty);
            Lottery.SetData("thisBuyTime", DateTime.MinValue);
            Lottery.SetData("reward", false);
            SaveSystem.SaveData();
        }

        var jackpotTicket = Lottery.GetData("jackpot")?.ToIntList('/');
        if (ListHelper.IsNullOrEmpty(jackpotTicket))
            lastJackpot?.SetStorage(candidates.Select(x => Pet.GetExamplePet(x)).ToList());
        else
            lastJackpot?.SetStorage(jackpotTicket.Select(x => Pet.GetExamplePet(x)).ToList());
    }

    private void ShowRewardSituation()
    {
        var lastRewardType = GetLastRewardType();
        getLastRewardButton?.gameObject.SetActive(!Lottery.GetData("reward", false));
        prizeImage?.SetSprite(rewardTypeSprites.Get(Mathf.Max(lastRewardType, 0)));
    }

    private void ShowBuySituation()
    {
        var lastBuyTicket = Lottery.GetData("lastBuy")?.ToIntList('/');
        var thisBuyTicket = Lottery.GetData("thisBuy")?.ToIntList('/');
        var buyDate = Lottery.GetData("thisBuyTime",DateTime.Today.AddDays(-1));
        var isBuyToday = buyDate.Date >=DateTime.Today;

        buyTicketButton?.gameObject.SetActive(!isBuyToday);
        alreadyBuyText?.gameObject.SetActive(isBuyToday);
        thisCandidate?.SetStorage(candidates.Select(x => Pet.GetExamplePet(x)).ToList());
        thisCandidate?.Select(0);

        if (!ListHelper.IsNullOrEmpty(lastBuyTicket))
            lastBuy?.SetStorage(lastBuyTicket.Select(x => Pet.GetExamplePet(x)).ToList());
        else
            lastBuy?.SetStorage(candidates.Select(x => Pet.GetExamplePet(x)).ToList());

        if (!ListHelper.IsNullOrEmpty(thisBuyTicket))
            thisBuy?.SetStorage(thisBuyTicket.Select(x => Pet.GetExamplePet(x)).ToList());
        else
            thisBuy?.SetStorage(candidates.Select(x => Pet.GetExamplePet(x)).ToList());

        thisBuy?.Select(0);
    }

    public void GetLastTicketReward()
    {
        var rewardType = GetLastRewardType();

        switch (rewardType)
        {
            default:
                var reward = rewardItemList.Get(rewardType);
                if (ListHelper.IsNullOrEmpty(reward))
                    break;

                reward.ForEach(item => Item.OpenHintbox(item, Item.Add));
                break;

            case -1:
                var pet420 = new Pet(420);
                var pet422 = new Pet(422);
                Pet.OpenHintbox(pet422, Pet.Add);
                Pet.OpenHintbox(pet420, Pet.Add);
                break;
        }

        Lottery.SetData("reward", true);
        SaveSystem.SaveData();

        ShowRewardSituation();
    }

    private int GetLastRewardType()
    {
        var lastJackpotTicket = Lottery.GetData("jackpot")?.ToIntList('/');
        var lastBuyTicket = Lottery.GetData("lastBuy")?.ToIntList('/');
        if (lastBuyTicket == null)
            return -1;

        int correctCount = 0;
        for (int i = 0; i < lastJackpotTicket.Count; i++)
        {
            if (lastBuyTicket.Get(i) != lastJackpotTicket.Get(i))
                continue;

            correctCount++;
        }

        return Mathf.Clamp(5 - correctCount, 0, 4);
    }

    public void BuyLotteryTicket()
    {
        if (!Lottery.GetData("reward", false))
        {
            Hintbox.OpenHintboxWithContent("请先领取上次彩票的奖励！", 16);
            return;
        }

        var thisBuyTicket = thisBuy.GetPetSelections().Select(x => x?.id ?? 0);
        if (ListHelper.IsNullOrEmpty(thisBuyTicket) || thisBuyTicket.Contains(0))
        {
            Hintbox.OpenHintboxWithContent("请选满5个精灵！", 16);
            return;
        }

        var hintbox = Hintbox.OpenHintbox();
        hintbox.SetContent("买定离手，购买后无法再修改，确定要购买吗？", 16, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(OnConfirmBuyTicket);
    }

    private void OnConfirmBuyTicket()
    {
        Lottery.SetData("thisBuy", thisBuy.GetPetSelections().Select(x => x.id.ToString()).ConcatToString("/"));
        Lottery.SetData("thisBuyTime",DateTime.Today);
        SaveSystem.SaveData();

        ShowBuySituation();
    }

    private void SelectCandidatePet(Pet pet)
    {
        var thisBuyTime = Lottery.GetData("thisBuyTime", DateTime.MinValue);
        if (thisBuyTime.Date >= DateTime.Today)
            return;

        var cursor = thisBuy.GetCursor().FirstOrDefault();
        thisBuy.SetStorage(thisBuy.GetPetSelections().UpdateAt(cursor, pet).ToList());
        thisBuy.Select(cursor);
    }
}
