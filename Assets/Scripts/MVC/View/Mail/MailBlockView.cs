using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MailBlockView : Module
{
    [SerializeField] private IButton button;
    [SerializeField] private Toggle toggle;
    [SerializeField] private IText senderText;
    [SerializeField] private IText titleText;
    [SerializeField] private IText dateText;

    private Mail mail;
    public bool isNull => mail == null;

    public void SetMail(Mail mail) {
        this.mail = mail;
        if (mail == null) {
            Clear();
            return;
        }
        senderText?.SetText(mail.sender);
        titleText?.SetText(mail.title);
        dateText?.SetText(mail.date.ToString("yyyy-MM-dd"));
    }

    public void Clear() {
        senderText?.SetText(string.Empty);
        titleText?.SetText(string.Empty);
        dateText?.SetText(string.Empty);
    }

    public void SetChosen(bool chosen) {
        toggle.SetIsOnWithoutNotify(chosen);
    }
}
