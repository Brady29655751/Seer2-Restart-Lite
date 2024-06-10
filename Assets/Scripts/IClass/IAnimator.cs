using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class IAnimator : IMonoBehaviour
{
    public RectTransform rect { get; protected set; }
    public Animator anim { get; protected set; }
    public RuntimeAnimatorController animController => anim?.runtimeAnimatorController;

    public UnityEvent onAnimStartEvent = new UnityEvent();
    public UnityEvent onAnimEndEvent = new UnityEvent();
    public UnityEvent onAnimHitEvent = new UnityEvent();

    protected override void Awake() {
        base.Awake();
        rect = GetComponent<RectTransform>();
        anim = GetComponent<Animator>();
    }

    public virtual void OnAnimStart() {
        onAnimStartEvent?.Invoke();
    }

    public virtual void OnAnimEnd() {
        onAnimEndEvent?.Invoke();
    }

    public virtual void OnAnimHit() {
        onAnimHitEvent?.Invoke();
    }

    private void OnDestroy() {
        onAnimStartEvent.RemoveAllListeners();
        onAnimEndEvent.RemoveAllListeners();
        onAnimHitEvent.RemoveAllListeners();
    }

    public void SetAnimClip(AnimationClip clip) {
        if (clip == null)
            return;

        AnimatorOverrideController aoc = new AnimatorOverrideController(animController);
        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

        foreach (var a in aoc.animationClips)
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, clip));

        aoc.ApplyOverrides(anims);
        anim.runtimeAnimatorController = aoc;
    }

}
