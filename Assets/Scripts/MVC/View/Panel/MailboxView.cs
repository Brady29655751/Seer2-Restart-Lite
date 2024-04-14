using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailboxView : Module
{
    [SerializeField] private MailStorageView storageView;
    [SerializeField] private MailContentView contentView;


    public void SetMailbox(Mail[] mails) {
        storageView.SetMailbox(mails);
    }

    public void SetChosen(bool[] chosen) {
        storageView.SetChosen(chosen);
    }

    public void ReadMail(Mail mail) {
        storageView.SetToggleActive(false);
        storageView.gameObject.SetActive(false);
        contentView.gameObject.SetActive(true);
        contentView.SetMail(mail);
    }

    public void BackToStorage() {
        storageView.SetToggleActive(true);
        storageView.gameObject.SetActive(true);
        contentView.gameObject.SetActive(false);
    }

}
