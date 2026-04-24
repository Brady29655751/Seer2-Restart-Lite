using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FTRuntime;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Video;
using Spine.Unity;

public class BattlePetAnimView : BattleBaseView
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRenderImage;
    [SerializeField] private IAnimator skillAnim, captureAnim;
    [SerializeField] private SpriteRenderer battlePetSprite;
    [SerializeField] private Transform battlePetAnimContainer;
    [SerializeField] private FightCamaraController camara;
    [SerializeField] private bool isMyView;
    [SerializeField] private IAnimator powerSkillAnim;

    private int cursorOffset = 0;
    private Pet currentPet;
    private PetUI currentPetUI; //仅限有动画的精灵使用这个字段,如果不为null,说明这个精灵有动画
    private GameObject currentPetAnim;
    private SwfClipController CurrentPetSwfAnim => currentPetAnim?.GetComponent<SwfClipController>();
    private SkeletonAnimation CurrentPetSkeletonAnim;

    private float animSpeed => //(battle.settings.mode == BattleMode.PVP) ? 1f : 
        Player.instance.gameData.settingsData.battleAnimSpeed;

    public bool isDone => isPetDone && isCaptureDone;
    protected bool isCaptureDone = true;
    protected bool isPetDone = true;
    protected string defalutSuperTrigger = "super";

    private Dictionary<GameObject, GameObject> cloneDictionary = new Dictionary<GameObject, GameObject>(); //当遇到抢动画事件时,克隆体存在这里面.通过本体来寻找保存的克隆体

    protected override void Awake()
    {
        base.Awake();
        videoPlayer.loopPointReached += (x) => RenderTexture.ReleaseTemporary(x.targetTexture);
    }

    public override void Init()
    {
        base.Init();
        captureAnim.anim.runtimeAnimatorController = (RuntimeAnimatorController)Player.GetSceneData("captureAnim");
        captureAnim.anim.SetFloat("speed", this.animSpeed);
        skillAnim.anim.SetFloat("speed", this.animSpeed);
    }

    private void OnDestroy()
    {
        if (videoPlayer?.targetTexture != null)
            RenderTexture.ReleaseTemporary(videoPlayer.targetTexture);
    }


    public void SetPet(BattlePet pet, int cursorOffset = 0)
    {
        GameObject tmp;

        this.cursorOffset = cursorOffset;
        this.currentPet = pet;
        this.defalutSuperTrigger = (pet.battleStatus.atk >= pet.battleStatus.mat) ? "physic" : "special";

        //检测是否有动画,如果有需要立刻关闭,否则直接出现在舞台上
        if ((cursorOffset == 0) && (tmp = pet.ui.GetBattleAnim(PetAnimationType.Idle)) != null)
        {
            if (CurrentPetSkeletonAnim != null)
            {
                DestroyImmediate(this.currentPetAnim);
            }

            this.currentPetAnim = tmp;
            this.currentPetAnim.transform.SetParent(this.battlePetAnimContainer);
            this.CurrentPetSkeletonAnim = this.currentPetAnim?.GetComponent<SkeletonAnimation>();
            this.CurrentPetSkeletonAnim?.Initialize(true);
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
            battlePetSprite.gameObject.transform.localScale = new Vector3(1920 / pet.ui.battleImage.rect.width, 1080 / pet.ui.battleImage.rect.height, 1) / (cursorOffset + 1);
            battlePetSprite.gameObject.SetActive(true);

            // 修正位置
            battlePetSprite.transform.position = new Vector3((isMyView ? -1 : 1) * 9.6f / (cursorOffset + 1), -5.8f / (cursorOffset + 1), battlePetSprite.transform.position.z);
        }
    }

    private void SetPetPresent()
    {
        if (battle.settings.parallelCount == 1)
        {
            // Spine anim
            if (CurrentPetSkeletonAnim != null)
            {
                if (CurrentPetSkeletonAnim.FindAnimation(PetAnimationType.Present) == null)
                {
                    this.SetPetIdle(null);
                    return;
                }

                CurrentPetSkeletonAnim.SetTimeScale(this.animSpeed);
                CurrentPetSkeletonAnim.SetSortingOrder(isMyView ? 2 : 1); // spine sorting order 不能双方在同一层
                this.ApplyAnimation();
                CurrentPetSkeletonAnim.SetAnimation(0, PetAnimationType.Present, false);
                CurrentPetSkeletonAnim.AnimationState.Complete += (x) => SetPetIdle(null);
                return;
            }

            // Swf anim, but no present anim, directly go to idle
            if ((this.currentPetAnim = this.currentPetUI.GetBattleAnim(PetAnimationType.Present)) == null)
            {
                this.SetPetIdle(null);
                return;
            }

            // Swf anim, play present anim
            CheckIfAnimUsed(); //防止抢动画
            isPetDone = false; //如果为false,说明动画还在播放,播完了变成idle状态,这个变量就会变成true,进行下一步

            if (CurrentPetSwfAnim != null)
            {
                CurrentPetSwfAnim.rateScale = this.animSpeed;
                CurrentPetSwfAnim.autoPlay = false;
                CurrentPetSwfAnim.loopMode = SwfClipController.LoopModes.Once;
                CurrentPetSwfAnim.OnStopPlayingEvent += SetPetIdle; //注册一个事件监听,当动画播放完毕后,回到idle状态
                this.ApplyAnimation();
                CurrentPetSwfAnim.GotoAndPlay(0);
            }
        }
        else
        {
            this.SetPetIdle(null);
        }
    }

    public void SetPetSkillAnim(Skill skill, PetAnimationType type)
    {
        // isPetDone表示UI有沒有播放完畢
        isPetDone = false;

        // 若為必殺技則立刻播放技能音效
        // PetUI 和 SoundInfo 有可能為 Null，必須用?.額外判定
        var skillAnimSpeed = this.animSpeed * (skill?.animSpeed ?? 1);
        var sound = string.IsNullOrEmpty(skill?.soundId) ? this.currentPet?.ui?.soundInfo?.GetSoundByType(type) : skill?.soundId;

        var hitFrame = this.currentPet?.ui?.hitInfo?.GetHitFrameByType(type) ?? 0;
        var videoStartFrame = this.currentPet?.ui?.hitInfo?.GetVideoStartFrameByType(type) ?? -1;

        bool isSuperSkill = type is PetAnimationType.Super or PetAnimationType.SecondSuper;
        if (isSuperSkill)
            PlaySkillSound(sound);

        void PlayDefaultAnim()
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
            skillAnim.anim.SetFloat("speed", skillAnimSpeed);
            skillAnim.transform.SetAsLastSibling();
            skillAnim.onAnimHitEvent.SetListener(() =>
            {
                if (!isSuperSkill)
                    PlaySkillSound(sound);

                OnPetHit();
            });
            skillAnim.onAnimEndEvent.SetListener(OnPetEnd);
            skillAnim.anim.SetTrigger(trigger);   
        }

        GameObject tmp;

        //使用技能动画,或者是通用技能动画
        if (this.currentPetUI != null)
        {
            if (CurrentPetSkeletonAnim != null)
            {
                if (CurrentPetSkeletonAnim.FindAnimation(type) == null)
                {
                    PlayDefaultAnim();
                }
                else
                {
                    CurrentPetSkeletonAnim.SetSortingOrder(3 * (6 - cursorOffset)); //攻击动画在3层,其他精灵动画都在1层,spine避免抢动画在2层,但捕捉动画在UI层在上面的
                    CurrentPetSkeletonAnim.SetTimeScale(skillAnimSpeed);
                    this.ApplyAnimation(false);
                    this.ScreenShake(type);
                    CurrentPetSkeletonAnim.SetAnimation(0, type, false);
                    CurrentPetSkeletonAnim.AnimationState.Complete += (x) => SetPetIdle(null);

                    _ = SetDelayCallback((int)(hitFrame * 1000 / (24 * skillAnimSpeed)), () =>
                    {
                        // 若不為必殺技則擊中才有音效
                        if (!isSuperSkill)
                            PlaySkillSound(sound);

                        OnPetHit();
                    });
                }
            }
            else if ((tmp = this.currentPetUI.GetBattleAnim(type)) != null)
            {
                this.currentPetAnim = tmp;
                this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x, this.currentPetAnim.transform.position.y, 2);

                if (CurrentPetSwfAnim != null)
                {
                    CurrentPetSwfAnim.clip.sortingOrder = 3 * (6 - cursorOffset); //攻击动画在3层,其他精灵动画都在1层,spine避免抢动画在2层,但捕捉动画在UI层在上面的
                    CurrentPetSwfAnim.rateScale = skillAnimSpeed;
                    CurrentPetSwfAnim.autoPlay = false;
                    CurrentPetSwfAnim.loopMode = SwfClipController.LoopModes.Once;
                }

                this.ApplyAnimation(false);
                this.ScreenShake(type);

                if (videoStartFrame >= 0)
                {
                    videoPlayer.url = this.currentPet?.ui?.hitInfo?.GetVideoUrl(type);
                    videoPlayer.playbackSpeed = skillAnimSpeed;
                    videoRenderImage.texture = videoPlayer.targetTexture = RenderTexture.GetTemporary(1920, 1080);
                    videoRenderImage.color = Color.clear;
                    videoPlayer.Prepare();

                    void OnVideoEnd(VideoPlayer vp)
                    {
                        videoPlayer.loopPointReached -= OnVideoEnd;

                        if (CurrentPetSwfAnim != null)
                        {
                            CurrentPetSwfAnim.OnStopPlayingEvent += SetPetIdle;
                            CurrentPetSwfAnim.Play(false);
                        }

                        videoRenderImage.color = Color.clear;
                        RenderTexture.ReleaseTemporary(videoPlayer.targetTexture);
                    }

                    _ = SetDelayCallback((int)(videoStartFrame * 1000 / (24 * skillAnimSpeed)), () =>
                    {
                        videoPlayer.loopPointReached += OnVideoEnd;
                        videoRenderImage.color = Color.white;

                        if (CurrentPetSwfAnim != null)
                            CurrentPetSwfAnim.Stop(false);

                        videoPlayer.Play();
                    });
                }
                else
                {
                    if (CurrentPetSwfAnim != null)
                        CurrentPetSwfAnim.OnStopPlayingEvent += SetPetIdle; //注册一个事件监听,当动画播放完毕后,回到idle状态    
                }

                _ = SetDelayCallback((int)(hitFrame * 1000 / (24 * skillAnimSpeed)), () =>
                {
                    // 若不為必殺技則擊中才有音效
                    if (!isSuperSkill)
                        PlaySkillSound(sound);

                    OnPetHit();
                });

                if (CurrentPetSwfAnim != null)
                    CurrentPetSwfAnim.GotoAndPlay(0);
            }
            else
            {
                PlayDefaultAnim();
            }
        }
        else if (videoStartFrame >= 0)
        {
            videoPlayer.url = this.currentPet?.ui?.hitInfo?.GetVideoUrl(type);
            videoPlayer.playbackSpeed = skillAnimSpeed;
            videoRenderImage.texture = videoPlayer.targetTexture = RenderTexture.GetTemporary(1920, 1080);
            videoRenderImage.color = Color.clear;
            videoPlayer.Prepare();

            void OnVideoEnd(VideoPlayer vp)
            {
                videoPlayer.loopPointReached -= OnVideoEnd;
                videoRenderImage.color = Color.clear;
                RenderTexture.ReleaseTemporary(videoPlayer.targetTexture);
            }

            _ = SetDelayCallback((int)(videoStartFrame * 1000 / (24 * skillAnimSpeed)), () =>
            {
                videoPlayer.loopPointReached += OnVideoEnd;
                videoRenderImage.color = Color.white;
                videoPlayer.Play();
            });

            _ = SetDelayCallback((int)(hitFrame * 1000 / (24 * skillAnimSpeed)), () =>
            {
                // 若不為必殺技則擊中才有音效
                if (!isSuperSkill)
                    PlaySkillSound(sound);

                OnPetHit();
            });
        }
        else  //为null,说明这个精灵没有动画,执行通用技能动画
        {
            PlayDefaultAnim();
        }

        StartCoroutine(PreventAnimStuckCoroutine(24));
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
        if (currentPetUI != null)
        {
            if (CurrentPetSkeletonAnim != null)
            {
                if (CurrentPetSkeletonAnim.FindAnimation(type) == null)
                {
                    this.SetPetIdle(null);
                    return;
                }

                CurrentPetSkeletonAnim.SetSortingOrder(1 * (6 - cursorOffset));
                this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x, this.currentPetAnim.transform.position.y, 0);
                CurrentPetSkeletonAnim.SetTimeScale(this.animSpeed);
                this.ApplyAnimation();
                CurrentPetSkeletonAnim.AnimationState.SetAnimation(0, type.ToString(), false);    
                CurrentPetSkeletonAnim.AnimationState.Complete += (x) => SetPetIdle(null);
            }
            else if ((tmp = this.currentPetUI.GetBattleAnim(type)) != null)
            {
                this.currentPetAnim = tmp;
                CurrentPetSwfAnim.clip.sortingOrder = 1 * (6 - cursorOffset);
                this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x, this.currentPetAnim.transform.position.y, 0);
                CurrentPetSwfAnim.rateScale = animSpeed;
                CurrentPetSwfAnim.autoPlay = false;
                CurrentPetSwfAnim.loopMode = SwfClipController.LoopModes.Once;
                CurrentPetSwfAnim.OnStopPlayingEvent += SetPetIdle; //注册一个事件监听,当动画播放完毕后,回到idle状态
                this.ApplyAnimation();
                CurrentPetSwfAnim.GotoAndPlay(0);
                //需要播一遍动画,然后回到idle状态,idle状态调用OnPetEnd   
            } 
            else
            {
                this.OnPetEnd();
            }
        }
        else
        {
            this.OnPetEnd();
        }
    }

    public void SetPetStateAnim(PetAnimationType type) //失败,胜利,濒死
    {
        GameObject tmp;
        if (currentPetUI != null)
        {
            if (CurrentPetSkeletonAnim != null)
            {
                if (CurrentPetSkeletonAnim.FindAnimation(type) == null)
                {
                    this.OnPetEnd();
                    return;
                }

                CurrentPetSkeletonAnim.SetSortingOrder((isMyView ? 2 : 1) * (6 - cursorOffset)); // spine sorting order 不能双方在同一层
                this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x, this.currentPetAnim.transform.position.y, 0);
                this.ApplyAnimation();
                CurrentPetSkeletonAnim.AnimationState.SetAnimation(0, type.ToString(), type is PetAnimationType.Dying);
            }
            else if ((tmp = this.currentPetUI.GetBattleAnim(type)) != null)
            {
                this.currentPetAnim = tmp;
                CurrentPetSwfAnim.clip.sortingOrder = 1 * (6 - cursorOffset);
                this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x, this.currentPetAnim.transform.position.y, 0);
                CurrentPetSwfAnim.autoPlay = false;
                this.ApplyAnimation();
                CurrentPetSwfAnim.GotoAndPlay(0);   
            } 
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

        if (CurrentPetSkeletonAnim != null)
        {
            CheckIfAnimUsed();
            CurrentPetSkeletonAnim.Initialize(true);
            CurrentPetSkeletonAnim.SetSortingOrder((isMyView ? 2 : 1) * (6 - cursorOffset)); // spine sorting order 不能双方在同一层
            CurrentPetSkeletonAnim.SetAnimation(0, PetAnimationType.Idle, true);
        }
        else
        {
            this.currentPetAnim = this.currentPetUI.GetBattleAnim(PetAnimationType.Idle);
            CheckIfAnimUsed();
            if (CurrentPetSwfAnim != null)
                CurrentPetSwfAnim.clip.sortingOrder = 1 * (6 - cursorOffset);
        }

        this.currentPetAnim.transform.position = new Vector3(this.currentPetAnim.transform.position.x, this.currentPetAnim.transform.position.y, 0);
        this.ApplyAnimation();
        this.OnPetEnd();   
    }

    protected void OnPetCapture(bool isSuccess)
    {
        if (isSuccess)
        {
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

    private void PlaySkillSound(string soundId)
    {
        if (string.IsNullOrEmpty(soundId) || (soundId == "0") || (soundId == "none"))
            return;
        
        var trimSoundId = soundId;
        bool isMod = (!string.IsNullOrEmpty(soundId)) && soundId.TryTrimStart("Mod/", out trimSoundId);
        ResourceManager.instance.GetLocalAddressables<AudioClip>($"BGM/skill/{trimSoundId}", isMod, (sound) =>
        {
            AudioSystem.instance.PlaySound(sound, AudioVolumeType.BattleSE);
        });
    }

    //如果两边是相同id精灵,那就不妙啦,两边会抢动画,还好只有出场,待机,濒死和失败(两边同时死亡不是没有可能)动画有可能两边同时发生
    //因此做一个检测,如果当前动画是活跃状态,动画的容器不为null是其他的容器,就说明这个动画在被对方使用,我们需要克隆一个对象,并缓存起来
    private void CheckIfAnimUsed()                                    
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

    private void ApplyAnimation(bool fixRateScale = true)
    {
        TransformHelper.DisableAllChild(this.battlePetAnimContainer);

        if (fixRateScale)
        {
            if (CurrentPetSwfAnim != null)
                CurrentPetSwfAnim.rateScale = this.animSpeed;

            if (CurrentPetSkeletonAnim != null)
                CurrentPetSkeletonAnim.SetTimeScale(this.animSpeed);
        }

        this.currentPetAnim.SetActive(true);

        foreach (Transform anim in this.battlePetAnimContainer)
        {
            if (!anim.gameObject.activeSelf)
                Destroy(anim.gameObject);
        }

        if ((!isMyView && this.currentPetAnim.transform.localScale.x > 0) ||
            (isMyView && this.currentPetAnim.transform.localScale.x < 0))
        {
            //如果this是敌方的View,而且动画没被翻转,那么就把动画翻转.如果已经翻转过(x<0),当然就不用翻转
            //有的时候会复用敌方精灵的动画,这时候还需要通过后面那个判断,把动画再翻转回来
            TransformHelper.Flip(this.currentPetAnim.transform);
        }

        this.currentPetAnim.transform.SetParent(this.battlePetAnimContainer);
        this.currentPetAnim.transform.Translate(Vector3.back * this.currentPetAnim.transform.position.z);   // Z座標在0以上才看的到
    }

    private void ScreenShake(PetAnimationType type)
    {
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
    }


    private static GameObject CloneAnim(GameObject original)
    {
        // 创建原始对象的副本
        GameObject copy = Object.Instantiate(original);
        copy.name = original.name; // 保持名称一致
        return copy;
    }

    private async Task SetDelayCallback(int delay, Action action)
    {
        await Task.Delay(delay);
        action();
    }
}