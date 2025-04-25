using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public static class NpcHandler
{
    public static void CreateNpc(NpcController npc, NpcInfo info, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        npc.SetActive(info.active);
        npc.SetNpcInfo(info);
        npc.SetAction(npcList, infoPrompt);
        npcList.Add(info.id, npc);
    }

    public static void CreateFarm(NpcController npc, NpcInfo info, Dictionary<int, NpcController> npcList, InfoPrompt infoPrompt) {
        npc.SetActive(info.active);
        npc.SetNpcInfo(info);
        npc.SetFarmAction(npcList, infoPrompt);
        npcList.Add(info.id, npc);
    }

    public static UnityEvent GetButtonEvent(IButton button, NpcButtonHandler handler) {
        return handler.type switch {
            ButtonEventType.OnPointerClick => button.onPointerClickEvent,
            ButtonEventType.OnPointerEnter => button.onPointerEnterEvent,
            ButtonEventType.OnPointerExit => button.onPointerExitEvent,
            ButtonEventType.OnPointerOver => button.onPointerOverEvent,
            _ => null,
        };
    }

    public static Func<bool> GetNpcCondition(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList) {
        if ((handler.condition == null) || (handler.condition.Count == 0))
            return () => true;
        
        Func<bool> condition = () => true;
        for (int i = 0; i < handler.condition.Count; i++) {
            var conditionOr = handler.condition[i].Split('|');

            Func<bool> oldCondition = new Func<bool>(condition);
            Func<bool> newCondition = () => false;

            for (int j = 0;  j < conditionOr.Length; j++) {
                Func<bool> orCondition = new Func<bool>(newCondition);
                var split = Operator.SplitCondition(conditionOr[j], out var op, toHalf: false);
                newCondition = () => orCondition.Invoke() || (NpcConditionHandler.GetNpcCondition(op, split.Key, split.Value).Invoke());
            }
            condition = () => oldCondition.Invoke() && newCondition.Invoke();
        }
        return condition;
    }

    public static Action GetNpcAction(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList) {
        NpcInfo npcInfo = npc?.GetInfo();
        return handler.action switch {
            NpcAction.SetNpcParam   => () => NpcActionHandler.SetNpcParam(npc, handler, npcList),
            NpcAction.OpenHintbox   => () => NpcActionHandler.OpenHintbox(npc, handler, npcList),
            NpcAction.OpenPanel     => () => NpcActionHandler.OpenPanel(handler),
            NpcAction.OpenDialog    => () => NpcActionHandler.OpenDialog(npcInfo, handler),
            NpcAction.Teleport      => () => NpcActionHandler.Teleport(handler),
            NpcAction.SetItem       => () => NpcActionHandler.SetItem(handler),
            NpcAction.GetPet        => () => NpcActionHandler.GetPet(handler),
            NpcAction.RemovePet     => () => NpcActionHandler.RemovePet(handler),
            NpcAction.SetPet        => () => NpcActionHandler.SetPet(handler),
            NpcAction.EvolvePet     => () => NpcActionHandler.EvolvePet(handler),
            NpcAction.SetMission    => () => NpcActionHandler.SetMission(handler),
            NpcAction.SetActivity   => () => NpcActionHandler.SetActivity(handler),
            NpcAction.Battle        => () => NpcActionHandler.StartBattle(npcInfo, handler),
            NpcAction.Player        => () => NpcActionHandler.SetPlayer(handler),
            NpcAction.SetMail       => () => NpcActionHandler.SetMail(handler),
            NpcAction.Fish          => () => NpcActionHandler.Fish(),
            _ => () => handler.callback?.ForEach(x => x?.Invoke()),
        };
    }
    
    public static Action GetNpcEntity(NpcController npc, NpcButtonHandler handler, Dictionary<int, NpcController> npcList) {
        NpcInfo npcInfo = npc?.GetInfo();
        Func<bool> condition = GetNpcCondition(npc, handler, npcList);
        Action action = GetNpcAction(npc, handler, npcList); 
        return () => {  
            if (Player.instance.isShootMode ^ (handler.typeId == "shoot"))
                return;
                
            if (!condition.Invoke())
                return;

            action?.Invoke();
        };
    }
}
