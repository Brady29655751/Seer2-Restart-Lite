using System.Linq;
using UnityEngine;

public class MapWildNpcBehaviourController : MonoBehaviour
{
    [SerializeField] private MapWildNpcWanderController wanderController;
    [SerializeField] private MapWildNpcTalkController talkController;

    private void Awake()
    {
        CacheReferences();
    }

    public static bool CanUseWildBehaviour(NpcInfo info)
    {
        return info?.wildInfo != null &&
               info.battleHandler != null &&
               info.battleHandler.Count > 0 &&
               info.eventHandler != null &&
               info.eventHandler.Any(handler =>
                   handler.type == ButtonEventType.OnPointerClick &&
                   handler.action == NpcAction.Battle);
    }

    public static bool RequiresNavigator(NpcInfo info)
    {
        return CanUseWildBehaviour(info) && info.wildInfo.movement?.isWander == true;
    }

    public void Init(Map map, MapNavigator navigator, NpcInfo info)
    {
        Stop();
        CacheReferences();
        if (!CanUseWildBehaviour(info))
            return;

        NpcWildMovementInfo movement = info.wildInfo.movement;
        bool shouldWander = movement?.isWander == true;

        talkController?.Init(info.wildInfo.talk, !shouldWander);
        if (wanderController != null)
        {
            wanderController.RestStarted -= HandleRestStarted;
            wanderController.RestStarted += HandleRestStarted;
            wanderController.Init(map, navigator, info, movement);
        }

        enabled = true;
    }

    public void Stop()
    {
        if (wanderController != null)
        {
            wanderController.RestStarted -= HandleRestStarted;
            wanderController.Stop();
            wanderController.enabled = false;
        }

        talkController?.Stop();
        enabled = false;
    }

    private void HandleRestStarted(float restDuration)
    {
        talkController?.OnRestStarted(restDuration);
    }

    private void CacheReferences()
    {
        if (wanderController == null)
            wanderController = GetComponent<MapWildNpcWanderController>();
        if (talkController == null)
            talkController = GetComponent<MapWildNpcTalkController>();
    }
}
