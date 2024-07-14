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

    public void SetChoice(YiTeRogueChoice choice) {
        gameObject.SetActive(choice != null);
        if (choice == null)
            return;

    }

    public void SetIcon(Sprite sprite) {
        icon?.SetSprite(sprite);
    }

    public void SetContent(string text) {
        content?.SetText(text);
    }

    public void SetCallback(Action callback) {
        button.onPointerClickEvent.SetListener(() => callback?.Invoke());
    }
}
