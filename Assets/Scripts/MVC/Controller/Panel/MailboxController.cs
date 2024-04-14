using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailboxController : Module
{
    [SerializeField] private MailboxModel mailboxModel;
    [SerializeField] private MailboxView mailboxView;
    [SerializeField] private PageView storagePageView;
    [SerializeField] private PageView contentPageView;

    public override void Init()
    {
        base.Init();
        storagePageView?.gameObject.SetActive(mailboxModel.mode == MailboxMode.Storage);
        contentPageView?.gameObject.SetActive(mailboxModel.mode == MailboxMode.Content);
    }

    public void SetMailbox(List<Mail> mails) {
        mailboxModel.SetStorage(mails);
        OnSetStoragePage();
    }

    public void OnSetStoragePage() {
        mailboxView.SetMailbox(mailboxModel.mails);
        mailboxView.SetChosen(mailboxModel.isSelected);
        storagePageView?.SetPage(mailboxModel.page, mailboxModel.lastPage);
    }

    public void OnSetContentPage() {
        contentPageView?.SetPage(mailboxModel.mailId, mailboxModel.count - 1);
    }

    public void Select(int id) {
        if (id == -1) {
            mailboxModel.SelectAll(mailboxModel.cursor.Length != mailboxModel.selectionSize);
            mailboxView.SetChosen(mailboxModel.isSelected);
            return;
        }
        mailboxModel.Select(id);
        mailboxView.SetChosen(mailboxModel.isSelected);
    }

    public void ReadMail(int offset) {
        if (mailboxModel.GetMailId(offset) >= mailboxModel.count) {
            BackToStorage();
            return;
        }
        mailboxModel.SetMode(MailboxMode.Content);
        mailboxModel.SetOffset(offset);

        mailboxModel.currentMail.Read();
        mailboxView.ReadMail(mailboxModel.currentMail);
        
        storagePageView?.gameObject.SetActive(false);
        contentPageView?.gameObject.SetActive(true);
        OnSetStoragePage();
        OnSetContentPage();
    }

    public void BackToStorage() {
        mailboxModel.SetMode(MailboxMode.Storage);
        mailboxView.BackToStorage();

        storagePageView?.gameObject.SetActive(true);
        contentPageView?.gameObject.SetActive(false);
    }

    public void Remove() {
        if (mailboxModel.mode == MailboxMode.Storage && mailboxModel.cursor.Length == 0)
            return;

        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("确定要刪除吗", 14, FontOption.Arial);
        hintbox.SetOptionNum(2);
        hintbox.SetOptionCallback(GetRemoveAction());
    }

    private Action GetRemoveAction() {
        Action remove = mailboxModel.mode switch {
            MailboxMode.Storage => RemoveStorage,
            MailboxMode.Content => RemoveContent,
            _ => RemoveStorage
        };
        return () => {
            remove.Invoke();
            mailboxModel.SelectAll(false);
            OnSetStoragePage();
            OnSetContentPage();
        };
    }

    private void RemoveStorage() {
        var cursor = mailboxModel.cursor;
        for (int i = cursor.Length - 1; i >= 0; i--) {
            mailboxModel.Remove(cursor[i]);
        }
    }

    private void RemoveContent() {
        mailboxModel.Remove(mailboxModel.offset);
        ReadMail(mailboxModel.mailId);
    }

    public void GetItem() {
        if (mailboxModel.mode == MailboxMode.Storage) {
            if (mailboxModel.cursor.Length == 0)
                return;
            
            foreach (var mail in mailboxModel.currentSelectedItems) {
                mail.GetItem();
            }
        } else if (mailboxModel.mode == MailboxMode.Content) {
            mailboxModel.mails[mailboxModel.offset].GetItem();
            ReadMail(mailboxModel.offset);
        }
        mailboxModel.SelectAll(false);
        mailboxView.SetChosen(mailboxModel.isSelected);
        Mail.OnGetItemSuccess();
    }

    public void PrevStoragePage() {
        mailboxModel.PrevPage();
        OnSetStoragePage();
    }

    public void NextStoragePage() {
        mailboxModel.NextPage();
        OnSetStoragePage();
    }

    public void PrevContentPage() {
        if (mailboxModel.offset == 0) {
            PrevStoragePage();
        } else {
            mailboxModel.SetOffset(mailboxModel.offset - 1);
        }
        ReadMail(mailboxModel.offset);
        OnSetContentPage();
    }

    public void NextContentPage() {
        if (mailboxModel.offset + 1 == mailboxModel.selectionCapacity) {
            NextStoragePage();
        } else {
            mailboxModel.SetOffset(mailboxModel.offset + 1);
        }
        ReadMail(mailboxModel.offset);
        OnSetContentPage();
    }

}
