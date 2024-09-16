using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YiTeRogueChoiceView : Module
{
    [SerializeField] private Image icon;
    [SerializeField] private Text content;
    [SerializeField] private IButton button;

    public void SetChoice(YiTeRogueChoice choice, Action onAfterStepCallback, int eventPos) {
        gameObject.SetActive(choice != null);
        if (choice == null)
            return;

        SetContent(choice.description);
        SetCallback(choice.callback, onAfterStepCallback, eventPos);
        SetIcon(choice.icon);
    }

    public void SetIcon(Sprite sprite) {
        icon?.gameObject.SetActive(sprite != null);
        icon?.SetSprite(sprite);
    }

    public void SetContent(string text) {
        content?.SetText(text);
    }

    public void SetCallback(Action choiceCallback, Action onAfterStepCallback, int eventPos) {
        button.onPointerClickEvent.SetListener(() => {
            choiceCallback?.Invoke();
            YiTeRogueData.instance.Step(eventPos);
            onAfterStepCallback?.Invoke();
        });
    }
}
