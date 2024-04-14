using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class NoobRewardView : Module
{
    [SerializeField] private Text signDaysText;
    [SerializeField] private Image tomorrowImage;
    [SerializeField] private List<Image> rewardIcons;
    [SerializeField] private List<Image> receiveMarks;

    public void SetSignDays(int signDays) {
        signDaysText?.SetText(signDays.ToString());
        for (int i = 0; i < receiveMarks.Count; i++) {
            receiveMarks[i]?.gameObject.SetActive(i < signDays);
        }
    }

    public void SetTodaySigned(bool isTodaySigned) {
        tomorrowImage?.gameObject.SetActive(isTodaySigned);
    }

    public void SetRewardIcons(string[] rewardIconPaths) {
        for (int i = 0; i < rewardIcons.Count; i++) {
            rewardIcons[i]?.SetSprite(  ItemInfo.GetIcon(rewardIconPaths[i]));
        }
    }

}
