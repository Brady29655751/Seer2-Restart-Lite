using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : Manager<DialogManager>
{
    [SerializeField] private RectTransform UILayer;
    [SerializeField] private RectTransform dialogLayer;
    [SerializeField] private DialogController dialogController;

    public NpcController currentNpc { get; private set; }

    private void SetDialogLayerActive(bool acitve) {
        UILayer.gameObject.SetActive(!acitve);
        dialogLayer.gameObject.SetActive(acitve);
    }

    public void SetCurrentNpc(NpcInfo info) {
        currentNpc = ((Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList"))?.Get(info.id);
        Player.instance.currentNpcId = info.id;
    }

    public void OpenDialog(DialogInfo info) {
        if (info == null) {
            CloseDialog();
            return;
        }

        SetDialogLayerActive(true);
        dialogController.OpenDialog(info);
    }

    public void CloseDialog() {
        SetDialogLayerActive(false);
    }

}
