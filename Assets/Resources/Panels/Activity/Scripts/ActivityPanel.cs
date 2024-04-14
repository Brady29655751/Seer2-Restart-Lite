using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityPanel : Panel
{
    [SerializeField] private ActivityController activityController;
    [SerializeField] private ActivityFilterController filterController;

    protected override void Awake() {
        InitSubscriptions();
    }

    public override void Init() {
        SetStorage(Activity.GetActivityInfoList());
    }

    private void InitSubscriptions() {
        filterController.onFilterEvent += activityController.Filter;
    }

    private void SetStorage(List<ActivityInfo> activities) {
        activityController.SetStorage(activities);
    }

}
