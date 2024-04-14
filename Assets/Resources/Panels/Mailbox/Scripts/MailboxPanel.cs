using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailboxPanel : Panel
{
    private List<Mail> mailbox => Player.instance.gameData.mailStorage;
    [SerializeField] private MailboxController mailboxController;

    public override void Init()
    {
        base.Init();
        mailboxController.SetMailbox(mailbox);
    }



}
