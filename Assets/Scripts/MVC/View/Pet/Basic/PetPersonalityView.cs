using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetPersonalityView : Module
{
    public ResourceManager RM => ResourceManager.instance;
    [SerializeField] private Image relationGraph;
    [SerializeField] private IText personalityText;
    [SerializeField] private IText noteText;

    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private GameObject personalityCellPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;

    private List<PersonalityCell> personalityCellList = new List<PersonalityCell>();

    public void CreatePersonalityList(List<Personality> personalitys, Action<Personality> callback = null, Action onFinishCreateCallback = null) {
        StartCoroutine(CreatePersonalityListCoroutine(personalitys, callback, onFinishCreateCallback));
    }

    private IEnumerator CreatePersonalityListCoroutine(List<Personality> personalitys, Action<Personality> callback = null, Action onFinishCreateCallback = null) {
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(-1, "正在加载性格");
        var personalitysCreatedEachTime = Mathf.Max(100, Mathf.FloorToInt(personalitys.Count * 0.01f));
        for (int i = 0; i < personalitys.Count; i += personalitysCreatedEachTime) {
            float progress = i * 1f / personalitys.Count;
            personalityCellList?.AddRange(personalitys.GetRange(i, Mathf.Min(personalitysCreatedEachTime, personalitys.Count - i)).Select(x => CreatePersonalityCell(x, callback)));
            loadingScreen.SetText("正在加载性格 " + i + " / " + personalitys.Count);
            loadingScreen.ShowLoadingProgress(progress);
            yield return null;
        }
        SceneLoader.instance.HideLoadingScreen();
        onFinishCreateCallback?.Invoke();
    }

    private PersonalityCell CreatePersonalityCell(Personality personality, Action<Personality> callback) {
        var obj = GameObject.Instantiate(personalityCellPrefab, contentRect);
        var cell = obj?.GetComponent<PersonalityCell>();

        cell?.SetInfoPrompt(infoPrompt);
        cell?.SetPersonality(personality);
        cell?.SetCallback(callback);
        return cell;
    }

    public void SetPersonalityList(List<Personality> personalitys) {
        for (int i = 0; i < personalityCellList.Count; i++)
            personalityCellList[i].SetPersonality((i < personalitys.Count) ? personalitys[i] : (Personality)(-1));
    }

    public void SetPersonalityText(Personality personality) {
        personalityText?.SetText(personality.ToString());
    }

    public void SetNote(string note) {
        noteText?.SetText(note);
    }

    public void SetRelationGraph(Personality personality) {
        int id = (int)personality;
        Sprite sprite = RM.GetSprite("Personality/Relation/" + id);
        relationGraph?.SetSprite(sprite);
    }

}
