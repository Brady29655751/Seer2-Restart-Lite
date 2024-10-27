using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FTRuntime;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BattlePetAnimView : BattleBaseView
{
    [SerializeField] private IAnimator skillAnim, captureAnim;
    [SerializeField] private SpriteRenderer battlePetSprite;
    [SerializeField] private Transform battlePetAnimContainer;
    [SerializeField] private FightCamaraController camara;
    [SerializeField] private bool isMyView;
    [SerializeField] private IAnimator powerSkillAnim;

    private PetUI currentPetUI; //仅限有动画的精灵使用这个字段,如果不为null,说明这个精灵有动画
    private GameObject currentPetAnim;

    private float animSpeed => (battle.settings.mode == BattleMode.PVP)
        ? 1f
        : Player.instance.gameData.settingsData.battleAnimSpeed;

    public bool isDone => isPetDone && isCaptureDone;
    protected bool isCaptureDone = true;
    protected bool isPetDone = true;
    protected string defalutSuperTrigger = "super";

    private Dictionary<GameObject, GameObject>
        cloneDictionary = new Dictionary<GameObject, GameObject>(); //当遇到抢动画事件时,克隆体存在这里面.通过本体来寻找保存的克隆体

    public override void Init()
    {
        base.Init();
        captureAnim.anim.runtimeAnimatorController = (RuntimeAnimatorController)Player.GetSceneData("captureAnim");
        captureAnim.anim.SetFloat("speed", this.animSpeed);
        skillAnim.anim.SetFloat("speed", this.animSpeed);
    }


    public void SetPet(BattlePet pet)
    {
        GameObject tmp;

        this.defalutSuperTrigger = (pet.battleStatus.atk >= pet.battleStatus.mat) ? "physic" : "special";
        if ((tmp = pet.ui.GetBattleAnim(PetAnimationType.Physic)) !=
            null) //检测是否有物攻动画,如果有,说明这个精灵有动画,但是需要立刻关闭,否则直接出现在舞台上
        {
            this.currentPetAnim = tmp;
            this.currentPetAnim.transform.SetParent(this.battlePetAnimContainer);
            this.currentPetAnim.SetActive(false);
            battlePetSprite.gameObject.SetActive(false);
            this.currentPetUI = pet.ui;
            this.SetPetPresent();
        }
        else
        {
            // GameObject == null 不代表真的是Null，僅為Unity認定的Null
            // 此時無法做SetActive，但使用 ?. 會檢測到非Null而繼續執行導致報錯
            if (this.currentPetAnim != null)
                this.currentPetAnim.SetActive(false);  

            this.currentPetUI = null; //当前精灵没有动画,但是上一只换场过来的精灵有动画,所以要把这个设为null,并且把动画关闭
            this.currentPetAnim = null;

            battlePetSprite.sprite = pet.ui.battleImage;
            battlePetSprite.gameObject.transform.localScale = new Vector3(1920 / pet.ui.battleImage.rect.width,
                1080 / pet.ui.battleImage.rect.height, 1);
            battlePetSprite.gameObject.SetActive(true);

            // 修正位置
            battlePetSprite.transform.position = new Vector3((isMyView ? -1 : 1) * 9.6f, -5.8f, 
                battlePetSprite.transform.position.z);    
        }
    }

    private void SetPetPresent()
    {
        if ((this.currentPetAnim = this.currentPetUI.GetBattleAnim(PetAnimationType.Present)) != null)
        {
            //检测是否有出场动画,如果有,播放出场动画.如果没有,直接播放待机动画
            CheckIfAnimUsed(); //防止抢动画
            isPetDone = false; //如果为false,说明动画还在播放,播完了变成idle状态,这个变量就会变成true,进行下一步
            SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
            controller.rateScale = this.animSpeed;
            controller.autoPlay = false;
            controller.loopMode = SwfClipController.LoopModes.Once;
            controller.OnStopPlayingEvent += SetPetIdle; //注册一个事件监听,当动画播放完毕后,回到idle状态
            this.ApplyAnimation();
            controller.GotoAndPlay(0);
        }
        else
        {
            this.SetPetIdle(null);
        }
    }

    public void SetPetSkillAnim(PetAnimationType type)
    {
        // isPetDone表示UI有沒有播放完畢
        isPetDone = false;

        // 若為必殺技則立刻播放技能音效
        // PetUI 和 SoundInfo 有可能為 Null，必須用?.額外判定
        var sound = this.currentPetUI?.soundInfo?.GetSoundByType(type);
        bool isSuperSkill = (type is PetAnimationType.Super or PetAnimationType.SecondSuper);
        if (isSuperSkill)
            PlaySkillSound(sound);

        //使用技能动画,或者是通用技能动画
        GameObject tmp;
        if (this.currentPetUI != null && (tmp = this.currentPetUI.GetBattleAnim(type)) != null)
        {
            this.currentPetAnim = tmp;
            //如果两个都不为null,说明这个精灵有动画,执行这个
            SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
            controller.clip.sortingOrder = 2; //攻击动画在2层,其他精灵动画都在1层,但捕捉动画在UI层在上面的
            this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x,
                this.currentPetAnim.transform.position.y, 2);
            controller.rateScale = animSpeed;
            controller.autoPlay = false;
            controller.loopMode = SwfClipController.LoopModes.Once;
            controller.OnStopPlayingEvent += SetPetIdle; //注册一个事件监听,当动画播放完毕后,回到idle状态
            this.ApplyAnimation();
            if (type is PetAnimationType.Super or PetAnimationType.SecondSuper)
            {
                this.camara.ScreenShake(0.4f);
                if (this.isMyView)
                {
                    this.powerSkillAnim.anim.SetTrigger("PowerSkillStart");
                }
                else
                {
                    this.powerSkillAnim.anim.SetTrigger("PowerSkillStartOp");
                }
            }

            _ = SetOnHit((int)(this.currentPetUI.hitInfo.GetFrameByType(type) * 1000 / (24 * animSpeed)), () => {  
                // 若不為必殺技則擊中才有音效
                if (!isSuperSkill)
                    PlaySkillSound(sound); 

                OnPetHit(); 
            });
            controller.GotoAndPlay(0);
        }
        else //为null,说明这个精灵没有动画,执行通用技能动画
        {
            string trigger = type switch
            {
                PetAnimationType.Physic => "physic",
                PetAnimationType.Special => "special",
                PetAnimationType.Property => "property",
                PetAnimationType.Super => this.defalutSuperTrigger,
                PetAnimationType.SecondSuper => this.defalutSuperTrigger,
                _ => string.Empty
            };
            skillAnim.transform.SetAsLastSibling();
            skillAnim.onAnimHitEvent.SetListener(() => {
                if (!isSuperSkill)
                    PlaySkillSound(sound);

                OnPetHit();
            });
            skillAnim.onAnimEndEvent.SetListener(OnPetEnd);
            skillAnim.anim.SetTrigger(trigger);
        }

        StartCoroutine(PreventAnimStuckCoroutine(24));
    }

    private async Task SetOnHit(int delay, Action action)
    {
        await Task.Delay(delay);
        action();
    }

    public void SetCaptureAnim(PetAnimationType type)
    {
        string trigger = type switch
        {
            PetAnimationType.CaptureSuccess => "success",
            PetAnimationType.CaptureFail => "fail",
            _ => string.Empty
        };
        bool isCaptureSuccess = (type == PetAnimationType.CaptureSuccess);
        isCaptureDone = false;
        captureAnim.onAnimHitEvent.SetListener(() => OnPetCapture(isCaptureSuccess));
        captureAnim.onAnimEndEvent.SetListener(OnPetEnd);
        captureAnim.anim.SetTrigger(trigger);
    }

    public void SetPetReactionAnim(PetAnimationType type)
    {
        //包括被打/被暴击/闪避
        this.isPetDone = false;
        GameObject tmp;
        if (currentPetUI != null && (tmp = this.currentPetUI.GetBattleAnim(type)) != null)
        {
            this.currentPetAnim = tmp;
            SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
            controller.clip.sortingOrder = 1;
            this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x,
                this.currentPetAnim.transform.position.y, 0);
            controller.rateScale = animSpeed;
            controller.autoPlay = false;
            controller.loopMode = SwfClipController.LoopModes.Once;
            controller.OnStopPlayingEvent += SetPetIdle; //注册一个事件监听,当动画播放完毕后,回到idle状态
            this.ApplyAnimation();
            controller.GotoAndPlay(0);
            //需要播一遍动画,然后回到idle状态,idle状态调用OnPetEnd
        }
        else
        {
            this.OnPetEnd();
        }
    }

    public void SetPetStateAnim(PetAnimationType type) //失败,胜利,濒死
    {
        GameObject tmp;
        if (currentPetUI != null && (tmp = this.currentPetUI.GetBattleAnim(type)) != null)
        {
            this.currentPetAnim = tmp;
            SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
            controller.clip.sortingOrder = 1;
            this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x,
                this.currentPetAnim.transform.position.y, 0);
            controller.autoPlay = false;
            this.ApplyAnimation();
            controller.GotoAndPlay(0);
        }

        this.OnPetEnd();
    }

    private void SetPetIdle(SwfClipController otherController)
    {
        //注意这个otherController是别的动画的controller,没啥用
        if (otherController != null)
        {
            //otherController若不是null,说明是别的动画播放完毕切换来的,需要取消监听
            otherController.OnStopPlayingEvent -= SetPetIdle;
        }

        this.currentPetAnim = this.currentPetUI.GetBattleAnim(PetAnimationType.Idle);
        CheckIfAnimUsed();
        SwfClipController controller = this.currentPetAnim.GetComponent<SwfClipController>();
        controller.clip.sortingOrder = 1;
        this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x,
            this.currentPetAnim.transform.position.y, 0);
        this.ApplyAnimation();
        this.OnPetEnd();
    }

    private void CheckIfAnimUsed() //如果两边是相同id精灵,那就不妙啦,两边会抢动画,还好只有出场,待机,濒死和失败(两边同时死亡不是没有可能)动画有可能两边同时发生
        //因此做一个检测,如果当前动画是活跃状态,动画的容器不为null是其他的容器,就说明这个动画在被对方使用,我们需要克隆一个对象,并缓存起来
    {
        if (this.currentPetAnim.activeInHierarchy && this.currentPetAnim.transform.parent != null &&
            this.currentPetAnim.transform.parent != this.battlePetAnimContainer)
        {
            GameObject originAnim = this.currentPetAnim;
            this.currentPetAnim = this.cloneDictionary.Get(this.currentPetAnim);
            if (this.currentPetAnim is null)
            {
                this.currentPetAnim = CloneAnim(originAnim);
                this.cloneDictionary.Add(originAnim, this.currentPetAnim);
            }
        }
    }

    private void PlaySkillSound(string soundId) {
        if (string.IsNullOrEmpty(soundId) || (soundId == "0") || (soundId == "none"))
            return;
        
        ResourceManager.instance.GetLocalAddressables<AudioClip>("BGM/skill/" + soundId, false, (sound) => {
            AudioSystem.instance.PlaySound(sound, AudioVolumeType.BattleSE);
        });
    }

    protected IEnumerator PreventAnimStuckCoroutine(float timeout)
    {
        float stuckTime = 0;
        while (stuckTime < timeout)
        {
            if (isPetDone)
                yield break;

            stuckTime += 1f;
            yield return new WaitForSeconds(1);
        }

        OnPetHit();
        yield return new WaitForSeconds(1);
        OnPetEnd();
    }

    private void ApplyAnimation()
    {
        TransformHelper.DisableAllChild(this.battlePetAnimContainer);
        this.currentPetAnim.GetComponent<SwfClipController>().rateScale = this.animSpeed;
        this.currentPetAnim.SetActive(true);
        if ((!isMyView && this.currentPetAnim.transform.localScale.x > 0) ||
            (isMyView && this.currentPetAnim.transform.localScale.x < 0))
        {
            //如果this是敌方的View,而且动画没被翻转,那么就把动画翻转.如果已经翻转过(x<0),当然就不用翻转
            //有的时候会复用敌方精灵的动画,这时候还需要通过后面那个判断,把动画再翻转回来
            TransformHelper.Flip(this.currentPetAnim.transform);
        }

        this.currentPetAnim.transform.SetParent(this.battlePetAnimContainer);
    }

    protected void OnPetCapture(bool isSuccess)
    {
        if (isSuccess) {
            battlePetSprite.gameObject.SetActive(false);
            return;
        }
    }

    protected void OnPetHit()
    {
        UI.ProcessQuery(true);
    }

    protected void OnPetEnd()
    {
        isPetDone = true;
        isCaptureDone = true;
    }


    private static GameObject CloneAnim(GameObject original)
    {
        // 创建原始对象的副本
        GameObject copy = Object.Instantiate(original);
        copy.name = original.name; // 保持名称一致
        return copy;
    }
}