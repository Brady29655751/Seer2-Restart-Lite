using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActivityListView : Module
{
    [SerializeField] List<Text> activityTexts;
    public void SetSelections(ActivityInfo[] selections) {
        for (int i = 0; i < activityTexts.Count; i++) {
            ActivityInfo activity = (i < selections.Length) ? selections[i] : null;
            activityTexts[i]?.transform.parent?.gameObject.SetActive(activity != null);
            activityTexts[i]?.SetText(activity?.name);
        }
    }
}
