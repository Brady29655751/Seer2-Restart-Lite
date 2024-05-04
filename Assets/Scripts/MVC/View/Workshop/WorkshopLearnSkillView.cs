using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopLearnSkillView : UIModule
{
    [SerializeField] private GameObject learnLevelObject;
    [SerializeField] private PetSkillBlockView skillBlockView;
    [SerializeField] private RectTransform selectSkillContentRect;
    [SerializeField] private GameObject selectSkillButtonPrefab;

    private List<GameObject> selectSkillButtonPrefabList = new List<GameObject>();

    public void SetLearnLevelActive(bool active) {
        learnLevelObject?.SetActive(active);
    }

    public void SetSelections(List<Skill> selections, Action<int> callback) {
        selectSkillButtonPrefabList.ForEach(Destroy);
        selectSkillButtonPrefabList = selections.Select((x, i) => { 
            int copy = i;
            var obj = Instantiate(selectSkillButtonPrefab, selectSkillContentRect);
            obj.GetComponent<IButton>()?.onPointerClickEvent.SetListener(() => callback?.Invoke(copy));
            obj.GetComponentInChildren<Text>()?.SetText(selections[copy].id + "/" + selections[copy].name);
            return obj;
        }).ToList();

        // Clear current preview
        OnPreviewSkill(null);
    }

    public void OnPreviewSkill(LearnSkillInfo skill) {
        skillBlockView.SetSkill(skill);
    }

    public void ShowSkillInfo(Skill skill) {
        infoPrompt.SetSkill(skill, false);
    }
}
