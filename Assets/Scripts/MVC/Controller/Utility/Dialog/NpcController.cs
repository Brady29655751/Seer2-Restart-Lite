using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : Module
{
    [SerializeField] private NpcModel npcModel;
    [SerializeField] private NpcView npcView;

    public NpcInfo GetInfo() {
        return npcModel.npcInfo;
    }

    public void SetActive(bool active) {
        gameObject.SetActive(active);
    }

    public void SetNpcInfo(NpcInfo info) {
        npcModel.SetNpcInfo(info);
        npcView.SetNpcInfo(info);
    }

    public void SetIcon(string resId) {
        npcView.SetIcon(resId);
    }

    public void SetColor(Color color) {
        npcView.SetColor(color);
    }

    public void SetName(string name) {
        npcView.SetName(name);
    }

    public void SetAction(Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        npcView.SetAction(this, npcList, infoPrompt);
    }

    public void SetFarmAction(Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        npcView.SetFarmAction(this, npcList, infoPrompt);
    }

    public void SetBGM(string bgm)
    {
        ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/" + bgm, onSuccess: (audioClip) => {
            if (audioClip != null)
                npcView.SetBGM(audioClip);
        });
    }
}
