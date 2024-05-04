using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopLearnBuffModel : Module
{
    public const int MAX_SEARCH_COUNT = 50;
    [SerializeField] private IInputField searchInputField, idInputField;

    public int id => (int.TryParse(idInputField.inputString, out var buffId)) ? buffId : 0;

    public List<BuffInfo> buffInfoList = new List<BuffInfo>();
    public BuffInfo currentBuffInfo => Buff.GetBuffInfo(id);

    public void Search() {
        if (string.IsNullOrEmpty(searchInputField.inputString)) {
            Hintbox.OpenHintboxWithContent("搜索名称不能为空！", 16);
            return;
        }

        idInputField.SetInputString(string.Empty);

        // Try search skills with same name.
        var buffInfoDict = Database.instance.buffInfoDict;
        buffInfoList = buffInfoDict.Where(x => x.Value.name == searchInputField.inputString)
            .OrderBy(x => x.Key).Select(x => x.Value).Take(MAX_SEARCH_COUNT).ToList();

        if (buffInfoList.Count > 0)
            return;

        // No skills with same name. Search similar result.
        buffInfoList = buffInfoDict.Where(x => x.Value.name.Contains(searchInputField.inputString))
            .OrderBy(x => x.Key).Select(x => x.Value).Take(MAX_SEARCH_COUNT).ToList();
    }

    public void Select(int index) {
        if (!index.IsInRange(0, buffInfoList.Count)) {
            idInputField.SetInputString(string.Empty);
            return;
        }

        idInputField.SetInputString(buffInfoList[index]?.id.ToString() ?? string.Empty);
    }

    public bool VerifyDIYLearnBuff(out string error) {
        error = string.Empty;

        if (string.IsNullOrEmpty(idInputField.inputString)) {
            error = "印记序号不能为空！";
            return false;
        }

        if (Buff.GetBuffInfo(id) == null) {
            error = "该印记序号不存在！";
            return false;
        }

        return true;
    }
}
