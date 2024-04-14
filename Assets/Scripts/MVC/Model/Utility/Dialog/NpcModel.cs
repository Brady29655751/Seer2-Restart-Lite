using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcModel : Module
{
    public NpcInfo npcInfo { get; protected set; }

    public void SetNpcInfo(NpcInfo info) {
        npcInfo = info;
    }
}
