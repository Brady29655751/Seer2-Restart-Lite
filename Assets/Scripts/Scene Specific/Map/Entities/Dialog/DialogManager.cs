using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : Manager<DialogManager>
{
    [SerializeField] private RectTransform UILayer;
    [SerializeField] private RectTransform dialogLayer;
    [SerializeField] private DialogController dialogController;
    [SerializeField] private RectTransform dialogStoryLayer;
    [SerializeField] private DialogController dialogStoryController;

    public NpcController currentNpc { get; private set; }

    private void SetDialogLayerActive(bool acitve) {
        UILayer.gameObject.SetActive(!acitve);
        dialogLayer.gameObject.SetActive(acitve);
    }

    private void SetStoryDialogLayerActive(bool acitve) {
        UILayer.gameObject.SetActive(!acitve);
        dialogStoryLayer.gameObject.SetActive(acitve);
    }
    public void SetCurrentNpc(NpcInfo info) {
        currentNpc = ((Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList"))?.Get(info.id);
        Player.instance.currentNpcId = info.id;
    }

    public void OpenDialog(DialogInfo info) {
        Player.instance.isShootMode = false;
        dialogLayer.SetAsLastSibling();
        
        if (info == null) {
            CloseDialog();
            return;
        }

        if (dialogStoryLayer.gameObject.activeSelf)
        {
            SetStoryDialogLayerActive(false);
        }

        SetDialogLayerActive(true);
        dialogController.OpenDialog(info);
    }
    
    public void OpenStoryDialog(DialogInfo info) {
        if (info == null) {
            CloseDialog();
            return;
        }
        if (dialogLayer.gameObject.activeSelf)
        {
            SetDialogLayerActive(false);
        }

        SetStoryDialogLayerActive(true);
        dialogStoryController.OpenDialog(info);
    }

  
    public void CloseDialog() {
        SetDialogLayerActive(false);
        SetStoryDialogLayerActive(false);
        Player.instance.currentNpcId = 0;
    }

}
