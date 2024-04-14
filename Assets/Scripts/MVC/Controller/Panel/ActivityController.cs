using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityController : Module
{
    [SerializeField] private ActivityModel activityModel;
    [SerializeField] private ActivityListView activityListView;
    [SerializeField] private ActivityContentView activityContentView;
    [SerializeField] private PageView pageView;

    public void SetStorage(List<ActivityInfo> storage) {
        activityModel.SetStorage(storage);
        OnSetActivityList();
    }
    protected void OnSetActivityList() {
        activityListView.SetSelections(activityModel.selections);
        pageView.SetPage(activityModel.page, activityModel.lastPage);
        Select(0);
    }

    public void Select(int index) {
        activityModel.Select(index);
        activityContentView?.SetActivity(activityModel.currentSelectedItems.FirstOrDefault());
    }

    public void Filter(Func<ActivityInfo, bool> filter) {
        activityModel.Filter(filter);
        OnSetActivityList();
    }

    public void Link() {
        ActivityInfo activity = activityModel.currentSelectedItems.FirstOrDefault();
        if (activity == null)
            return;
        
        Activity.Link(activity.id);
    }

    public void OnActivityPrevPage() {
        activityModel.PrevPage();
        OnSetActivityList();
    }

    public void OnActivityNextPage() {
        activityModel.NextPage();
        OnSetActivityList();
    }
}
