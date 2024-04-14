using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailContentView : Module
{
    [SerializeField] private MailBlockView titleView;
    [SerializeField] private IText contentText;
    [SerializeField] private List<PetItemBlockView> itemBlockViews;

    public void SetMail(Mail mail) {
        titleView?.SetMail(mail);
        contentText?.SetText(mail.content);
        SetItems(mail.items);
    }

    private void SetItems(List<Item> items) {
        for (int i = 0; i < itemBlockViews.Count; i++) {
            if (i >= items.Count) {
                itemBlockViews[i].gameObject.SetActive(false);
                continue;
            }
            itemBlockViews[i].gameObject.SetActive(true);
            itemBlockViews[i].SetItem(items[i]);
        }
    }
}
