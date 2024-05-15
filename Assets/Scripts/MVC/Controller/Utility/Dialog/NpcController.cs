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

    public void SetAction(Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        npcView.SetAction(this, npcList, infoPrompt);
    }

    public void SetBGM(string bgm)
    {
         
        AudioClip audioClip = ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/" + bgm);

        if (audioClip != null) {
            // 如果成功加载到 AudioClip，将其传递给 npcView.SetBGM 方法
            // 這裡不用先停止音樂，AudioSystem 會自動處理
            npcView.SetBGM(audioClip);
        } else {
            Debug.LogError("Failed to load AudioClip: " + bgm);
        }
    }
}
