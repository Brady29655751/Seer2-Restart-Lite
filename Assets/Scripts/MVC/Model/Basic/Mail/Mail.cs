using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mail
{
    public bool isRead = false;
    public string sender;
    public string title;
    public string content;
    public DateTime date;

    [XmlArray("gift"), XmlArrayItem(typeof(Item), ElementName = "item")]
    public List<Item> items;

    public Mail() {}

    public Mail(string sender, string title, string content, DateTime date, List<Item> items = null) {
        isRead = false;
        this.sender = sender;
        this.title = title;
        this.content = content;
        this.date = date;
        this.items = items ?? new List<Item>();
    }

    public Mail(Mail rhs) {
        isRead = rhs.isRead;
        sender = rhs.sender;
        title = rhs.title;
        content = rhs.content;
        date = rhs.date;
        items = rhs.items.Select(x => new Item(x)).ToList();
    }

    public static Mail Find(Predicate<Mail> predicate) {
        return Player.instance.gameData.mailStorage.Find(predicate);
    }

    public static void Add(Mail mail) {
        Player.instance.gameData.mailStorage.Add(mail);
    }

    public static void AddRange(IEnumerable<Mail> mailList) {
        Player.instance.gameData.mailStorage.AddRange(mailList);
    }

    public static void Remove(Mail mail) {
        Player.instance.gameData.mailStorage.Remove(mail);
    }

    public static void OnGetItemSuccess() {
        Hintbox hintbox = Hintbox.OpenHintbox();
        hintbox.SetTitle("提示");
        hintbox.SetContent("领取成功", 14, FontOption.Arial);
        hintbox.SetOptionNum(1);
    }

    public static void VersionUpdate() {
        var updateMails = GameManager.versionData.mailData.updateMails
            .Select(x => new Mail(x){ date = DateTime.Now });
        Mail.AddRange(updateMails);
    }

    public static void DailyLogin() {
        Mail mail = new Mail(GameManager.versionData.mailData.dailyLoginMail){ date = DateTime.Now };
        Mail.Add(mail);
    }

    public void Read() {
        if (isRead)
            return;
            
        isRead = true;
    }

    public void GetItem() {
        if ((items == null) || (items.Count == 0))
            return;

        foreach (var item in items) {
            Item.Add(item);
        }
        items.Clear();
    }

}
