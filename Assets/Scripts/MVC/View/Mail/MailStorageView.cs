using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailStorageView : Module
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private List<MailBlockView> mailBlockViews;

    public void SetMailbox(Mail[] mails) {
        for (int i = 0; i < mailBlockViews.Count; i++) {
            mailBlockViews[i]?.SetMail((i < mails.Length) ? mails[i] : null);
        }
    }

    public void SetChosen(bool[] chosen) {
        int activeCount = mailBlockViews.Count(x => !x.isNull);
        toggle?.SetIsOnWithoutNotify(chosen.Count(x => x) == activeCount);
        for (int i = 0; i < mailBlockViews.Count; i++) {
            mailBlockViews[i].SetChosen((i < activeCount) ? chosen[i] : false);
        }
    }

    public void SetToggleActive(bool active) {
        toggle.gameObject.SetActive(active);    
    }
}
