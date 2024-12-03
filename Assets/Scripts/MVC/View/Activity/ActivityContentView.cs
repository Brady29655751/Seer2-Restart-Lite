using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ActivityContentView : Module
{
    [SerializeField] private Image background;
    [SerializeField] private IText titleText;
    [SerializeField] private Text titleLegacyText;
    [SerializeField] private Text contentText;
    [SerializeField] private Text timeText;
    [SerializeField] private List<PetItemBlockView> itemBlockViews;

    public void SetActivity(ActivityInfo activity) {
        if (activity == null) {
            Clear();
            return;
        }
        var rewardIcons = activity.rewardIcons;
        background?.gameObject.SetActive(true);
        background?.SetSprite(  activity.activityBackground);
        titleText?.SetText(activity.name);
        titleLegacyText?.SetText(activity.name);
        contentText?.SetText(activity.description);
        timeText?.SetText(activity.time);
        for (int i = 0; i < itemBlockViews.Count; i++) {
            itemBlockViews[i].SetRewardIcon((i < rewardIcons.Count) ?   ItemInfo.GetIcon(rewardIcons[i]) : null);
        }
    }

    private void Clear() {
        background?.gameObject.SetActive(false);
        titleText?.SetText(string.Empty);
        titleLegacyText?.SetText(string.Empty);
        contentText?.SetText(string.Empty);
        timeText?.SetText(string.Empty);
        for (int i = 0; i < itemBlockViews.Count; i++) {
            itemBlockViews[i].SetRewardIcon(null);
        }
    }
}
