using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogModel : Module
{
    public DialogInfo info { get; protected set; }

    public void OpenDialog(DialogInfo info) {
        this.info = info;
    }
}
