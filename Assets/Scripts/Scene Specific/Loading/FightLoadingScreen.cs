using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FTRuntime;

public class FightLoadingScreen : LoadingScreen
{
    private Battle battle => Player.instance.currentBattle;
    [SerializeField] private AudioClip startLoadingSound;
    [SerializeField] private List<BattlePetInfoView> playerView, enemyView;

    public override void OnBeforeLoading(Action callback)
    {
        GameManager.instance.ChangeState(GameState.Fight);
        AudioSystem.instance.StopMusic();
        AudioSystem.instance.StopEffect();

        SetLoadingProgressNumber(0);

        var parallelCount = battle.settings.parallelCount;
        var myPet = battle.currentState.myUnit.petSystem.petBag.Take(parallelCount).ToList();
        var opPet = battle.currentState.opUnit.petSystem.petBag.Take(parallelCount).ToList();
        
        for (int i = 0; i < playerView.Count; i++) {
            bool active = (i < myPet.Count) && (myPet[i] != null);
            playerView[i].gameObject.SetActive(active);
            if (!active)
                continue;

            playerView[i].SetPet(myPet[i]);
        }

        for (int i = 0; i < enemyView.Count; i++) {
            bool active = (i < opPet.Count) && (opPet[i] != null);
            enemyView[i].gameObject.SetActive(active);
            if (!active)
                continue;

            var pet = (opPet[i].buffController.GetBuff(3090) == null) ? opPet[i] : new BattlePet(new Pet(myPet[i].id, opPet[i]));
            enemyView[i].SetPet(pet);
        }
        
        // playerView.SetPet(myPet);
        // enemyView.SetPet((opPet.buffController.GetBuff(3090) == null) ? opPet : new BattlePet(new Pet(myPet.id, opPet)));

        AudioSystem.instance.PlaySound(startLoadingSound, AudioVolumeType.BattleSE);
        StartCoroutine(WaitSecondsCoroutine(1.5f, callback));
    }

    protected override IEnumerator ChangeSceneCoroutine(int sceneIndex, Action finishedCallback = null) {
        var currentMap = Player.instance.currentMap;
        var resId = (currentMap.resId == 0) ? currentMap.id : currentMap.resId;
        var fightMapId = (currentMap.fightMapId == 0) ? resId : currentMap.fightMapId;
        var fightMapPath = (currentMap.fightMapId == 0) ? ("Maps/bg/" + resId) : ("Maps/fightBg/" + currentMap.fightMapId);
        
        Player.SetSceneData("fightBgIsMod", Map.IsMod(fightMapId));
        Player.SetSceneData("fightBg", ResourceManager.instance.GetLocalAddressables<Sprite>(fightMapPath, Map.IsMod(fightMapId)));
        Player.SetSceneData("captureAnim", ResourceManager.instance.GetLocalAddressables<RuntimeAnimatorController>("Pets/capture/capture"));

        float progress = 0;
        if (PhotonNetwork.IsConnected) {
            if (NetworkManager.IsMasterClient) {
                PhotonNetwork.LoadLevel(sceneIndex);
            }
            while ((progress = PhotonNetwork.LevelLoadingProgress) < 1) {
                ShowLoadingProgress(progress);
                yield return null;
            }
        } else {
            var operation = SceneManager.LoadSceneAsync(sceneIndex);
            while (!operation.isDone) {
                ShowLoadingProgress(operation.progress / 0.9f);
                yield return null;
            }   
        }
        finishedCallback?.Invoke();
    }

    public override void OnAfterLoading(Action callback)
    {
        SwfManager.GetInstance(false)?.Pause();
        StartCoroutine(WaitSecondsCoroutine(1.2f, () => {            
            callback?.Invoke();
            SwfManager.GetInstance(false)?.Resume();
        }));
    }

}
