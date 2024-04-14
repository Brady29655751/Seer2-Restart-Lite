using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogController : Module
{
    [SerializeField] private DialogModel dialogModel;
    [SerializeField] private DialogView dialogView;

    public void OpenDialog(DialogInfo info) {
        dialogModel.OpenDialog(info);
        dialogView.OpenDialog(info);
    }

    public void OnBackgroundClick() {
        if (dialogModel.info.replyHandler.Count != 1)
            return;

        var handler = dialogModel.info.replyHandler[0];
        if (handler.action != NpcAction.OpenDialog)
            return;

        if ((handler.param == null) || (handler.param.Count != 1))
            return;

        if (handler.param[0] != "null")
            return;

        DialogManager.instance.CloseDialog();
    }
}
