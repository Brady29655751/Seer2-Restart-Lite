using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetRecord
{
    public int winFightNum = 0;
    public int loseFightNum = 0;

    public PetRecord() {}

    public PetRecord(Dictionary<string, object> record) {
        winFightNum = (int)record.Get("winFightNum", 0);
        loseFightNum = (int)record.Get("loseFightNum", 0);
    }

    public PetRecord(PetRecord rhs) {
        winFightNum = rhs.winFightNum;
        loseFightNum = rhs.loseFightNum;
    }
}
