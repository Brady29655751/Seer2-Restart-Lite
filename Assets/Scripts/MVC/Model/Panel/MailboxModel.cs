using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MailboxModel : SelectModel<Mail>
{
    public Mail[] mails => selections;

    public int offset { get; protected set; }
    public int mailId => GetMailId(offset);
    public Mail currentMail => mailId.IsInRange(0, count) ? resultStorage[mailId] : null;

    public MailboxMode mode { get; protected set; } = MailboxMode.Storage;

    public override void PrevPage()
    {
        base.PrevPage();
        SetOffset(capacity - 1);
    }

    public override void NextPage()
    {
        base.NextPage();
        SetOffset(0);
    }

    public override int GetRefreshPageAfterRemoved()
    {
        if (resultStorage.Count % capacity == 1) {
            SetOffset(0);
        }
        return base.GetRefreshPageAfterRemoved();
    }

    public int GetMailId(int offset) {
        return capacity * page + offset;
    }

    public void SetOffset(int offset) {
        this.offset = Mathf.Min(offset, capacity - 1);
    }

    public void SetMode(MailboxMode mode) {
        this.mode = mode;
    }

}

public enum MailboxMode {
    Storage = 0,
    Content = 1,
}
