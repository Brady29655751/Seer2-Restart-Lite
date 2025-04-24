using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public void SetSprite(Sprite sprite) {
        npcView.SetSprite(sprite);
    }

    public void SetPosition(Vector2 pos) {
        npcView.SetPosition(pos);
    }

    public void SetRotation(Vector3 rotation) {
        npcView.SetRotation(rotation);
    }

    public void SetRect(Vector2 pos, Vector2 size, Quaternion rotation) {
        npcView.SetRect(pos, size, rotation);
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

    public void Shoot() {
        if (!Player.instance.isShootMode)
            return;

        if (PlayerController.instance == null)
            return;

        PlayerController.instance.Shoot(GetInfo()?.eventHandler?.Where(x => x.typeId == "shoot").Select(x =>
            NpcHandler.GetNpcEntity(this, x, (Dictionary<int, NpcController>)Player.GetSceneData("mapNpcList"))).ToList());
    }
}
