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
    [XmlElement("resourceVersion")] public string resourceVersion;
    
    public static List<string> versionType => new List<string>() { "alpha", "beta", "lite" };
    public static string DefaultVersion => "alpha_0.1";

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

    public static int Compare(string lhsVersion, string rhsVersion, int compareBits = int.MaxValue) {
        int lhsVersionIndex = versionType.FindIndex(x => lhsVersion.StartsWith(x));
        int rhsVersionIndex = versionType.FindIndex(x => rhsVersion.StartsWith(x));

        if ((lhsVersionIndex == -1) || (rhsVersionIndex == -1))
            return 0;

        if (lhsVersionIndex != rhsVersionIndex)
            return lhsVersionIndex.CompareTo(rhsVersionIndex);

        var lhsVersionSplit = (lhsVersion.TrimStart(versionType[lhsVersionIndex] + "_")).Split('.').Select(int.Parse).ToArray();
        var rhsVersionSplit = (rhsVersion.TrimStart(versionType[rhsVersionIndex] + "_")).Split('.').Select(int.Parse).ToArray();
        var length = Mathf.Max(lhsVersionSplit.Length, rhsVersionSplit.Length);

        for (int i = 0; i < length; i++) {
            if (i >= compareBits)
                break;

            var lhs = (i < lhsVersionSplit.Length) ? lhsVersionSplit[i] : -1;
            var rhs = (i < rhsVersionSplit.Length) ? rhsVersionSplit[i] : -1;

            if (lhs != rhs)
                return lhs.CompareTo(rhs);
        }

        return 0;
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

    [XmlIgnore] public List<Pet> petAllWithMod => PetInfo.database.Select(x => Pet.GetExamplePet(x.id )).Where(x => x != null).ToList();
    [XmlIgnore] public List<Pet> petDictionary => petAllWithMod.Where(x => x.id.IsWithin(minPetId, maxPetId)).ToList();

    [XmlIgnore] public List<Pet> petTopic => topicPetIds.Select(Pet.GetExamplePet).Where(x => (x != null)).ToList();
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
