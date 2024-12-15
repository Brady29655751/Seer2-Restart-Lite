using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : UIModule
{
    [SerializeField] protected Image background;
    [SerializeField] protected IButton ESCButton;

    public event Action onCloseEvent;

    public static T OpenPanel<T>() where T : Panel {
        string name = typeof(T).Name.TrimEnd("Panel");
        GameObject canvas = GameObject.Find("Canvas");
        GameObject prefab = ResourceManager.instance.GetPanel(name);
        if (prefab == null)
            return null;

        GameObject obj = Instantiate(prefab, canvas.transform);
        obj.transform.SetAsLastSibling();
        T panel = obj.GetComponentInChildren<T>();
        return panel;
    }

    public new static Panel OpenPanel(string panelName) {
        if ((panelName == "null") || (panelName == "none")) {
            Panel.CloseTopPanel();
            return null;
        }

        bool isModPanel = panelName.TryTrimStart("[Mod]", out var trimPanelName);
        panelName = isModPanel ? "Mod" : panelName.Replace("Panel", string.Empty);

        int optionIndex = panelName.IndexOf('[');
        string options = string.Empty;
        if (optionIndex >= 0) {
            options = panelName.Substring(optionIndex);
            panelName = panelName.Substring(0, optionIndex);
        }

        GameObject canvas = GameObject.Find("Canvas");
        GameObject prefab = ResourceManager.instance.GetPanel(panelName);

        if (prefab == null)
            return null;

        GameObject obj = Instantiate(prefab, canvas.transform);
        obj.transform.SetAsLastSibling();
        Panel panel = obj.GetComponentInChildren<Panel>();

        if (isModPanel) {
            bool isSuccessLoading = SaveSystem.TryLoadPanelMod(trimPanelName, out var panelData);
            ((ModPanel)panel).SetPanelData(isSuccessLoading ? panelData : null);
            return panel;
        }
        
        while (options.TryTrimParentheses(out var action)) {
            var param = action.Split('=');
            panel.SetPanelIdentifier(param[0], param[1]);
            options = options.TrimStart("[" + action + "]");
        }

        return panel;
    }

    public static void ClosePanel<T>() where T : Panel {
        Panel panel = GameObject.FindFirstObjectByType<T>();
        if (panel != null)
            panel.ClosePanel();
    }

    public static void Link(string linkId) {
        if (string.IsNullOrEmpty(linkId) || (linkId == "none"))
            return;

        if (int.TryParse(linkId, out int mapId))
            TeleportHandler.Teleport(mapId);
        else 
            Panel.OpenPanel(linkId);
    }

    public static void CloseTopPanel() {
        GameObject canvas = GameObject.Find("Canvas");
        Panel panel = null;
        foreach (Transform child in canvas.transform)
            panel = child.GetComponentInChildren<Panel>() ?? panel;
        
        if (panel != null)
            panel.ClosePanel();
    }

    public virtual void ClosePanel() {
        onCloseEvent?.Invoke();
        
        if (background != null) {
            Destroy(background.gameObject);
        }
        Destroy(gameObject);
    }

    public void SetActive(bool active) {
        var panel = background?.gameObject ?? gameObject;
        panel.SetActive(active);
    }

    public virtual void SetPanelIdentifier(string id, string param) {

    }

    public void SetBackgroundColor(Color color) {
        background.color = color;
    }

}
