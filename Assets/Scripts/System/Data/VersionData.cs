using System;
using System.Linq;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[XmlRoot("versionData")]
public class VersionData
{
    [XmlElement("gameVersion")] public string gameVersion;
    [XmlElement("buildVersion")] public string buildVersion;
    
    public static List<string> versionType => new List<string>() { "alpha", "beta", "lite " };

    public DateTime releaseDate;
    public string releaseNote;

    public VersionMailData mailData;
    public VersionPetData petData;
    public VersionSkillData skillData;
    public VersionMissionData missionData;

    public bool IsEmpty() {
        return string.IsNullOrEmpty(gameVersion);
    }

    public static bool IsNullOrEmpty(VersionData versionData) {
        return (versionData == null) || (versionData.IsEmpty());
    }

    public static int Compare(string lhsVersion, string rhsVersion) {
        int lhsVersionIndex = versionType.FindIndex(x => lhsVersion.StartsWith(x));
        int rhsVersionIndex = versionType.FindIndex(x => rhsVersion.StartsWith(x));

        if ((lhsVersionIndex == -1) || (rhsVersionIndex == -1))
            return 0;

        if (lhsVersionIndex != rhsVersionIndex)
            return lhsVersionIndex.CompareTo(rhsVersionIndex);
        
        float lhsVersionNumber = float.Parse(lhsVersion.TrimStart(versionType[lhsVersionIndex] + "_"));
        float rhsVersionNumber = float.Parse(lhsVersion.TrimStart(versionType[rhsVersionIndex] + "_"));
        return lhsVersionNumber.CompareTo(rhsVersionNumber);
    }
}

public class VersionMailData {

    [XmlArray("updateMail"), XmlArrayItem(typeof(Mail), ElementName = "mail")]
    public List<Mail> updateMails;

    public Mail dailyLoginMail;
}

public class VersionPetData {
    public int petNum;
    public int minPetId, maxPetId;
    [XmlElement("topicPetId")] public string topicPets;
    [XmlIgnore] public List<int> topicPetIds => topicPets.ToIntList();

    [XmlIgnore] public List<Pet> petAllWithMod => Database.instance.petInfoDict.Select(entry => Pet.GetExamplePet(entry.Key))
        .OrderByDescending((Pet x) => (x.id > 0) ? (int.MaxValue - x.id) : (x.id)).ToList();
    [XmlIgnore] public List<Pet> petDictionary => Enumerable.Range(minPetId, maxPetId - minPetId + 1).Select(x => Pet.GetExamplePet(x)).Where(x => x != null)
        .OrderByDescending((Pet x) => (x.id > 0) ? (int.MaxValue - x.id) : (x.id)).ToList();

    [XmlIgnore] public List<Pet> petTopic => topicPetIds.Select(x => Pet.GetExamplePet(x)).Where(x => (x != null)).ToList();
}

public class VersionSkillData {
    public int skillNum;
    public int minSkillId, maxSkillId;
}

public class VersionMissionData {
    [XmlElement("mainCount")] public int mainMissionCount;
    [XmlElement("sideCount")] public int sideMissionCount;
    [XmlElement("dailyCount")] public int dailyMissionCount;
    [XmlElement("eventCount")] public int eventMissionCount;
    [XmlIgnore] public int totalMissionCount => mainMissionCount + sideMissionCount + dailyMissionCount + eventMissionCount;
}
