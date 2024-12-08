using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NoobIntroController : Module
{
    [SerializeField] private string noobIntroKey;
    [SerializeField] private List<GameObject> introObjects = new List<GameObject>();
    [SerializeField] private List<Vector2Int> rewardItems = new List<Vector2Int>();

    private int cursor = -1;

    public override void Init()
    {
        base.Init();
        CheckNoob();
    }

    private void CheckNoob() {
        if (Activity.Noob.GetData(noobIntroKey) == "done") {
            gameObject.SetActive(false);
            return;
        }
        NextIntro();
    }

    public void NextIntro() {
        introObjects.Get(cursor)?.SetActive(false);
        
        cursor++;
        if (cursor < introObjects.Count) {
            introObjects.Get(cursor)?.SetActive(true);
            return;
        }

        rewardItems.ForEach(reward => {
            var item = new Item(reward.x, reward.y);
            Item.Add(item);
            Item.OpenHintbox(item);
        });
        Activity.Noob.SetData(noobIntroKey, "done");
        SaveSystem.SaveData();

        OnFinishIntro();
    }

    public void OnFinishIntro(string jump = null) {
        jump ??= noobIntroKey;

        switch (jump) {
            default:
                break;
            case "map":
                TeleportHandler.Teleport(61);
                break;
            case "train":
                Map.TestBattle(2, 201, "default");
                break;
            case "skip":
                Activity.Noob.SetData("train", "done");
                Activity.Noob.SetData("battle", "done");
                SaveSystem.SaveData();
                Panel.OpenPanel<SignRewardPanel>();
                Hintbox.OpenHintboxWithContent("恭喜你完成新手教学！\n有任何问题可以打开地图下方的指引哦！", 16);
                CheckNoob();
                break;
        }
    }
}
