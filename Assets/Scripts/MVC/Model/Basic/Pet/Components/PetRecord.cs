using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetRecord
{
    public int winFightNum = 0;
    public int loseFightNum = 0;

    public List<IKeyValuePair<string, string>> recordDict;

    public PetRecord() {
        recordDict = new List<IKeyValuePair<string, string>>();
    }

    public PetRecord(Dictionary<string, object> record) {
        winFightNum = (int)record.Get("winFightNum", 0);
        loseFightNum = (int)record.Get("loseFightNum", 0);

        recordDict = record.Select(x => new IKeyValuePair<string, string>(x.Key, x.Value.ToString())).ToList();
        recordDict.RemoveAll(x => (x.key == "winFightNum") || (x.key == "loseFightNum"));
    }

    public PetRecord(PetRecord rhs) {
        winFightNum = rhs.winFightNum;
        loseFightNum = rhs.loseFightNum;
        recordDict = rhs.recordDict.Select(x => new IKeyValuePair<string, string>(x)).ToList();
    }

    public string GetRecord(string key, string defaultValue = null) {
        return key switch {
            "winFightNum" => winFightNum.ToString(),
            "loseFightNum" => loseFightNum.ToString(),
            _ => recordDict.Find(x => x.key == key)?.value ?? defaultValue
        };
    }

    public bool TryGetRecord(string key, out string value) {
        value = GetRecord(key, null);
        return value != null;
    }

    public void SetRecord(string key, object value) {
        recordDict.RemoveAll(x => x.key == key);

        if (value != null)
            recordDict.Add(new IKeyValuePair<string, string>(key, value.ToString()));
            
        SaveSystem.SaveData();
    }
}
