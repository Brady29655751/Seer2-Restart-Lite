using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopSkillListView : Module
{
    [SerializeField] private InfoPrompt infoPrompt;
    [SerializeField] private GameObject skillCellPrefab;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentRect;

    private List<SkillCell> skillCellList = new List<SkillCell>();

    public void CreateSkillList(List<Skill> skills, Action<Skill> callback = null) {
        StartCoroutine(CreateSkillListCoroutine(skills, callback));
    }

    private IEnumerator CreateSkillListCoroutine(List<Skill> skills, Action<Skill> callback = null) {
        var loadingScreen = SceneLoader.instance.ShowLoadingScreen(-1, "正在加载技能");
        var skillsCreatedEachTime = Mathf.Max(400, Mathf.FloorToInt(skills.Count * 0.05f));
        for (int i = 0; i < skills.Count; i += skillsCreatedEachTime) {
            float progress = i * 1f / skills.Count;
            skillCellList?.AddRange(skills.GetRange(i, Mathf.Min(skillsCreatedEachTime, skills.Count - i)).Select(x => CreateSkillCell(x, callback)));
            loadingScreen.SetText("正在加载技能 " + i + " / " + skills.Count);
            loadingScreen.ShowLoadingProgress(progress);
            yield return null;
        }
        SceneLoader.instance.HideLoadingScreen();
    }

    private SkillCell CreateSkillCell(Skill skill, Action<Skill> callback) {
        var obj = GameObject.Instantiate(skillCellPrefab, contentRect);
        var cell = obj?.GetComponent<SkillCell>();

        cell?.SetInfoPrompt(infoPrompt);
        cell?.SetSkill(skill);
        cell?.SetCallback(callback, "id");
        return cell;
    }

    public void ShowResult(List<Skill> skills, bool isIdFilter) {
        if (isIdFilter) {
            ScrollToSkill(skills?.FirstOrDefault());
            return;
        }
            
        SetSkillList(skills);
    }

    public void SetSkillList(List<Skill> skills) {
        for (int i = 0; i < skillCellList.Count; i++)
            skillCellList[i].SetSkill((i < skills.Count) ? skills[i] : null);
    }

    public void ScrollToSkill(Skill skill) {
        if (skill == null) {
            scrollRect.verticalNormalizedPosition = 1;
            return;
        }

        var cell = skillCellList?.Find(x => (x != null) && (skill.id == x.currentSkill.id));
        if (cell == null) {
            scrollRect.verticalNormalizedPosition = 1;
            return;
        }

        var pos = cell.rectTransform.anchoredPosition;
        scrollRect.verticalNormalizedPosition = 1 + pos.y / contentRect.rect.size.y;
    }
}
