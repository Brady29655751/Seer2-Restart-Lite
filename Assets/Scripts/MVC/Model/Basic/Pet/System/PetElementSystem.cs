using System.Xml.Serialization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PetElementSystem {

    public static int elementNum => elementDefenseRelation.Count;

    public const float N = 1.0f;
    public const float R = 0.5f;
    public const float W = 2.0f;
    public const float O = 0.0f;

    public static List<Element> elementList => new List<Element>() {
        Element.普通,   Element.草,     Element.水,     Element.火, 
        Element.风,     Element.虫,     Element.飞行,   Element.电,     
        Element.地面,   Element.冰,     Element.超能,   Element.战斗,
        Element.特质,   Element.光,     Element.神秘,   Element.暗影,
        Element.龙,     Element.沙漠,   Element.圣灵,   Element.精灵王,
    };

    public static Dictionary<Element, List<float>> elementDefenseRelation = new Dictionary<Element, List<float>>() {
        { Element.普通, new List<float>() { N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N, N } },
        { Element.草, new List<float>()   { N, R, R, W, N, W, W, R, R, N, N, N, R, O, N, N, N, R, W, W } },
        { Element.水, new List<float>()   { N, W, R, R, N, N, N, W, N, R, N, R, R, N, N, R, W, N, W, W } },
        { Element.火, new List<float>()   { N, R, W, R, N, R, N, N, W, R, N, N, R, N, N, N, N, W, W, W } },
        { Element.风, new List<float>()   { N, N, N, W, R, N, R, R, R, W, W, R, R, N, N, N, N, R, N, W } },
        { Element.虫, new List<float>()   { N, R, N, W, N, N, W, N, N, R, R, W, W, R, W, R, N, N, R, W } },
        { Element.飞行, new List<float>() { N, R, N, N, R, R, N, W, O, W, N, N, R, N, W, N, R, R, N, W } },
        { Element.电, new List<float>()   { N, N, N, N, N, N, R, R, W, N, R, N, R, N, N, N, R, W, W, W } },
        { Element.地面, new List<float>() { N, N, W, N, W, N, W, O, N, N, R, N, W, N, R, R, N, N, N, W } },
        { Element.冰, new List<float>()   { N, R, N, W, O, R, N, N, N, R, N, W, R, R, N, N, W, R, W, W } },
        { Element.超能, new List<float>() { N, N, N, N, N, W, N, N, R, N, R, R, R, W, N, W, R, N, N, W } },
        { Element.战斗, new List<float>() { N, N, N, R, W, R, N, N, N, R, W, R, R, N, R, N, N, N, R, W } },
        { Element.特质, new List<float>() { N, R, R, R, R, W, R, R, W, R, R, R, R, W, R, W, W, W, W, W } },
        { Element.光, new List<float>()   { N, W, N, N, N, N, R, N, W, N, R, N, W, R, N, R, N, N, N, W } },
        { Element.神秘, new List<float>() { N, N, R, N, R, N, N, R, R, N, W, N, R, W, R, N, W, R, R, W } },
        { Element.暗影, new List<float>() { N, N, W, N, N, R, R, W, R, N, N, N, W, W, N, W, R, N, N, W } },
        { Element.龙, new List<float>()   { N, N, N, R, R, W, R, N, R, N, N, W, W, N, R, W, N, N, R, W } },
        { Element.沙漠, new List<float>() { N, W, R, R, W, N, N, N, N, W, N, N, W, R, W, N, N, R, N, W } },
        { Element.圣灵, new List<float>() { N, R, R, R, N, W, N, R, R, R, N, W, W, R, W, R, W, R, N, W } },
        { Element.精灵王, new List<float>() { N, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, R, W } },
    };

    public static float GetElementRelation(Skill lhs, BattlePet rhs) {
        var defenseRelation = elementDefenseRelation.Get(rhs.element);
        return defenseRelation[lhs.elementId];
    }

    public static List<Element> GetAttackRelation(Element element, float relation) {
        List<Element> attackRelation = new List<Element>();
        for (int i = 0; i < elementNum; i++) {
            Element e = (Element)i;
            var defenseRelation = elementDefenseRelation.Get(e);
            if (defenseRelation[(int)element] == relation)
                attackRelation.Add(e);
        }
        return attackRelation;
    }

}

public enum Element {
    全部 = -1,
    普通 = 0, 草 = 1, 水 = 2, 火 = 3, 风 = 4, 虫 = 5, 
    飞行 = 6, 电 = 7, 地面 = 8, 冰 = 9, 超能 = 10, 
    战斗 = 11, 特质 = 12, 光 = 13, 神秘 = 14, 暗影 = 15, 
    龙 = 16, 沙漠 = 17, 圣灵 = 18, 精灵王 = 19, // 上古 = 20, 
    // 神迹 = 21, 神遁 = 22,
}

