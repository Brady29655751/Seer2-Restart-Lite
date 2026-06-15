using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapUIController : Module
{
    [SerializeField] private MapInfoController infoController;
    [SerializeField] private MapPetCornerController petCornerController;
    [SerializeField] private MapMenuController menuController;
    [SerializeField] private MapWorldController worldController;
    [Header("Hover Feedback")]
    [SerializeField] private bool enableMapUIHoverFeedback = true;
    [SerializeField] private float mapUIHoverScale = 1.06f;
    [SerializeField] private float mapUIHoverLerpSpeed = 12f;

    public void SetMap(Map map) {
        infoController.SetMapInfoText(map.name);
        infoController.SetWeather(map.weather);
        infoController.SetDayNightSwitch(map.dayNightSwitch);
        
        petCornerController.SetPet(Player.instance.petBag.FirstOrDefault());
        ConfigureHoverFeedback();
    }

    private void ConfigureHoverFeedback()
    {
        if (!enableMapUIHoverFeedback)
            return;

        AddPetCornerHoverFeedback();
        AddPetShortcutHoverFeedback();
        AddLeftBarHoverFeedback();
        AddNamedHoverFeedback("Map Corner");
        AddNamedHoverFeedback("Mini Map Button");
        AddMapInfoHoverFeedback();
        AddMenuHoverFeedback();
    }

    private void AddPetCornerHoverFeedback()
    {
        if (petCornerController == null)
            return;

        AddNamedDescendantHoverFeedback(petCornerController.transform, "Background Button");
        AddNamedDescendantHoverFeedback(petCornerController.transform, "Recover");
    }

    private void AddPetShortcutHoverFeedback()
    {
        var petShortcut = FindActiveRectTransform("Pet Shortcut");
        if (petShortcut == null)
            return;

        AddHoverFeedbackToButtons(petShortcut, null);
        AddNamedDescendantHoverFeedback(petShortcut, "Background Button");
    }

    private void AddLeftBarHoverFeedback()
    {
        foreach (var leftBar in FindActiveRectTransforms("Left Bar"))
        {
            if (leftBar.GetComponent<MapNoobController>() == null)
                continue;

            AddHoverFeedbackToButtons(leftBar, null);
        }
    }

    private void AddMenuHoverFeedback()
    {
        if (menuController == null)
            return;

        var settingsArea = FindDescendant(menuController.transform, "Settings Area");
        var settingIcon = settingsArea == null ? null : FindDescendant(settingsArea, "Setting Icon") as RectTransform;

        AddHoverFeedbackToButtons(menuController.transform, null, buttonTransform =>
        {
            if ((settingsArea != null) && buttonTransform.IsChildOf(settingsArea))
                return settingIcon;

            return buttonTransform as RectTransform;
        });
    }

    private void AddMapInfoHoverFeedback()
    {
        if (infoController == null)
            return;

        AddHoverFeedbackToButtons(infoController.transform, null);
    }

    private void AddNamedHoverFeedback(string targetName)
    {
        var target = FindActiveRectTransform(targetName);
        if (target == null)
            return;

        var buttons = target.GetComponentsInChildren<IButton>(true);
        if (buttons.Length > 0)
        {
            foreach (var button in buttons)
            {
                AddHoverFeedback(button.gameObject, target);
            }
            return;
        }

        AddHoverFeedback(target.gameObject, target);
    }

    private void AddHoverFeedbackToButtons(Transform root, RectTransform overrideTarget)
    {
        AddHoverFeedbackToButtons(root, overrideTarget, null);
    }

    private void AddHoverFeedbackToButtons(Transform root, RectTransform overrideTarget, Func<Transform, RectTransform> getButtonTarget)
    {
        var handledObjects = new HashSet<GameObject>();
        foreach (var button in root.GetComponentsInChildren<IButton>(true))
        {
            var target = overrideTarget != null ? overrideTarget : (getButtonTarget?.Invoke(button.transform) ?? button.rect);
            AddHoverFeedback(button.gameObject, target);
            handledObjects.Add(button.gameObject);
        }

        foreach (var button in root.GetComponentsInChildren<Button>(true))
        {
            if (handledObjects.Contains(button.gameObject))
                continue;

            var target = overrideTarget != null ? overrideTarget : (getButtonTarget?.Invoke(button.transform) ?? button.transform as RectTransform);
            AddHoverFeedback(button.gameObject, target);
        }
    }

    private void AddNamedDescendantHoverFeedback(Transform root, string targetName)
    {
        var target = FindDescendant(root, targetName) as RectTransform;
        if (target == null)
            return;

        var buttons = target.GetComponents<IButton>();
        if (buttons.Length > 0)
        {
            foreach (var button in buttons)
            {
                AddHoverFeedback(button.gameObject, button.rect);
            }
            return;
        }

        var nativeButton = target.GetComponent<Button>();
        AddHoverFeedback(nativeButton != null ? nativeButton.gameObject : target.gameObject, target);
    }

    private void AddHoverFeedback(GameObject eventObject, RectTransform target)
    {
        if ((eventObject == null) || (target == null))
            return;

        var feedback = eventObject.GetComponent<UIHoverScaleFeedback>();
        if (feedback == null)
        {
            feedback = eventObject.AddComponent<UIHoverScaleFeedback>();
        }

        feedback.SetFeedback(target, mapUIHoverScale, mapUIHoverLerpSpeed);
    }

    private RectTransform FindActiveRectTransform(string targetName)
    {
        return FindActiveRectTransforms(targetName).FirstOrDefault();
    }

    private List<RectTransform> FindActiveRectTransforms(string targetName)
    {
        var results = new List<RectTransform>();
        foreach (var rect in FindObjectsOfType<RectTransform>(true))
        {
            if (rect.gameObject.activeInHierarchy && (rect.name == targetName))
                results.Add(rect);
        }

        return results;
    }

    private Transform FindDescendant(Transform root, string targetName)
    {
        if (root == null)
            return null;

        foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == targetName)
                return rect.transform;
        }

        return null;
    }
}
