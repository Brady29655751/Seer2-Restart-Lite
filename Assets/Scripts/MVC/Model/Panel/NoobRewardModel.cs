using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoobRewardModel : Module
{
    [SerializeField] private string rewardIconDataPath;
    [SerializeField] private string rewardItemDataPath;

    public Activity activity = null;
    public bool isTodaySigned => IsTodaySigned();
    public int signedDays => int.Parse(activity.GetData("signedDays", "0"));
    public string[] rewardIcons { get; private set; }
    public string[] rewardItems { get; private set; }

    protected override void Awake() {
        rewardIcons = ResourceManager.LoadCSV(rewardIconDataPath);
        rewardItems = ResourceManager.LoadCSV(rewardItemDataPath);
        activity = Activity.Find("noob_reward");
    }

    public bool IsTodaySigned() {
        var lastSignedDate = DateTime.Parse(activity.GetData("lastSignedDate", DateTime.MinValue.Date.ToString())).Date;
        return (DateTime.Now.Date - lastSignedDate).Days < 1;
    }

    public void Sign() {
        GetReward(rewardItems.GetRange(4 * signedDays, 4 * (signedDays + 1)));

        activity.SetData("lastSignedDate", DateTime.Now.Date.ToString());
        activity.SetData("signedDays", (signedDays + 1).ToString());
        
        SaveSystem.SaveData();
    }

    private void GetReward(List<string> rewards) {
        for (int i = 0; i < rewards.Count; i++) {
            var rewardInfo = rewards[i].Trim().Split('/');
            int id = int.Parse(rewardInfo[1]);
            int num = int.Parse(rewardInfo[2]);
            if (rewardInfo[0] == "Item") {
                Item item = new Item(id, num);
                Item.Add(item);
                Item.OpenHintbox(item);
            } else if (rewardInfo[0] == "Pet") {
                Pet pet = new Pet(id, num);
                Pet.Add(pet);

                ItemHintbox itemHintbox = Hintbox.OpenHintbox<ItemHintbox>();
                itemHintbox.SetTitle("提示");
                itemHintbox.SetContent("获得了 " + pet.name + " ！", 14, FontOption.Arial);
                itemHintbox.SetOptionNum(1);

                itemHintbox.SetIcon( pet.ui.icon);
            }
        }
    }
}
