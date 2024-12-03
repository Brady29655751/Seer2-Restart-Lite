using System.Collections;
using System.Collections.Generic;
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
        if (Activity.Find("noob").GetData(noobIntroKey) == "done") {
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
        Activity.Find("noob").SetData(noobIntroKey, "done");
        SaveSystem.SaveData();
    }
}
