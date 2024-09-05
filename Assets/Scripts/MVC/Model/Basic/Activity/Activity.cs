using System;
using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Activity
{
    public ActivityInfo info => GetActivityInfo(id);

    [XmlAttribute("id")] public string id;

    [XmlArray("data"), XmlArrayItem(typeof(IKeyValuePair<string, string>), ElementName = "entry")]
    public List<IKeyValuePair<string, string>> data = new List<IKeyValuePair<string, string>>();

    public static ActivityInfo GetActivityInfo(string id) {
        return Database.instance.GetActivityInfo(id);
    }

    public static List<ActivityInfo> GetActivityInfoList() {
        return Database.instance.activityInfos;
    }

    public static Activity Find(string id) {
        var activityStorage = Player.instance.gameData.activityStorage;
        var activity = activityStorage.Find(x => x.id == id);
        if (activity == null) {
            activity = new Activity(id);
            activityStorage.Add(activity);
        }
        return activity;
    }

    public static void Link(string id) {
        ActivityInfo info = GetActivityInfo(id);
        if ((info == null) || (info.linkId == "none"))
            return;

        Panel.Link(info.linkId);
    }

    public static void DailyLogin() {
        var activityStorage = Player.instance.gameData.activityStorage;
        activityStorage.RemoveAll(x => x.info.type == ActivityType.Daily);
    }

    public Activity() {}
    public Activity(string id) {
        this.id = id;
        this.data = new List<IKeyValuePair<string, string>>();
    }

    public string GetData(string key, string defaultValue = null) {
        var entry = data.Find(x => x.key == key);
        return ((entry == null) || (entry.value == null)) ? defaultValue : entry.value;
    }

    public void SetData(string key, string value) {
        // Handle Value.
        // For example, [expr]200+activity[10001].damage[default]123
        if (value.TryTrimStart("[expr]", out var expr))
            value = Parser.ParseOperation(expr).ToString();

        var entry = data.Find(x => x.key == key);
        if (entry == null)
            data.Add(new IKeyValuePair<string, string>(key, value));
        else
            entry.value = value;
    }

}