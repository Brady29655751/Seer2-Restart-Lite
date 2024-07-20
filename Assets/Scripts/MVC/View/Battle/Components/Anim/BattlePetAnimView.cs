using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePetAnimView : BattleBaseView
{
    [SerializeField] private IAnimator skillAnim, captureAnim;
    [SerializeField] private Image battlePetImage;

    public bool isDone => isPetDone && isCaptureDone;
    protected bool isCaptureDone = true;
    protected bool isPetDone = true;
    protected string defalutSuperTrigger = "super";
    protected string defalutSecondSuperTrigger = "secondSuper";

    public override void Init()
    {
        base.Init();
        captureAnim.anim.runtimeAnimatorController = (RuntimeAnimatorController)Player.GetSceneData("captureAnim");
    }

    public void SetPet(BattlePet pet) {
        SettingsData settingsData = Player.instance.gameData.settingsData;
        float animSpeed = (battle.settings.mode == BattleMode.PVP) ? 1f : settingsData.battleAnimSpeed;
        
        battlePetImage.SetSprite(pet.ui.battleImage);
        captureAnim.anim.SetFloat("speed", animSpeed);
        skillAnim.anim.SetFloat("speed", animSpeed);
        defalutSuperTrigger = defalutSecondSuperTrigger = ((pet.battleStatus.atk >= pet.battleStatus.mat) ? "physic" : "special");
    }

    public void SetPetAnim(Skill skill, PetAnimationType type) {
        string trigger = type switch {
            PetAnimationType.CaptureSuccess => "success",
            PetAnimationType.CaptureFail => "fail",
            PetAnimationType.Physic => "physic",
            PetAnimationType.Special => "special",
            PetAnimationType.Property => "property",
            PetAnimationType.Super => defalutSuperTrigger,
            PetAnimationType.SecondSuper => defalutSecondSuperTrigger,
            _ => string.Empty
        };
        
        if (trigger == string.Empty)
            return;

        bool isCaptureSuccess = (type == PetAnimationType.CaptureSuccess);
        bool isCaptureFail = (type == PetAnimationType.CaptureFail);
        if (isCaptureSuccess || isCaptureFail) {
            isCaptureDone = false;
            captureAnim.onAnimHitEvent.SetListener(() => OnPetCapture(isCaptureSuccess));
            captureAnim.onAnimEndEvent.SetListener(OnPetEnd);
            captureAnim.anim.SetTrigger(trigger);
            return;
        }

        isPetDone = false;
        skillAnim.transform.SetAsLastSibling();
        skillAnim.onAnimHitEvent.SetListener(OnPetHit);
        skillAnim.onAnimEndEvent.SetListener(OnPetEnd);
        skillAnim.anim.SetTrigger(trigger);
        StartCoroutine(PreventAnimStuckCoroutine(10));
    }

    /*
    protected void PlaySkillSound(Skill skill) {
        string type = (skill.type == SkillType.必杀) ? "Super" : "Normal";
        string soundId = skill.soundId;
        AudioClip sound = ResourceManager.instance.GetSE("Skill/" + type + "/" + soundId);
        AudioSystem.instance.PlaySound(sound, AudioVolumeType.BattleSE);
        petAnim.onAnimStartEvent.RemoveAllListeners();
    }
    */

    protected IEnumerator PreventAnimStuckCoroutine(float timeout) {
        float stuckTime = 0;
        while (stuckTime < timeout) {
            if (isPetDone)
                yield break;
            
            stuckTime += 1f;
            yield return new WaitForSeconds(1);
        }
        OnPetHit();
        yield return new WaitForSeconds(1);
        OnPetEnd();
    }

    protected void OnPetCapture(bool isSuccess) {
        battlePetImage.gameObject.SetActive(!isSuccess);
    }

    protected void OnPetHit() {
        UI.ProcessQuery(true);
    }

    protected void OnPetEnd() {
        isPetDone = true;
        isCaptureDone = true;
    }

}
