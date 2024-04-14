using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageView : Module
{
    [SerializeField] private Text currentPageText;
    [SerializeField] private Text lastPageText;
    [SerializeField] private IButton prevButton;
    [SerializeField] private IButton nextButton;

    public void SetPage(int page, int lastPage, int firstPage = 0) {
        if (currentPageText != null) {
            currentPageText.text = (page + 1).ToString();
        }
        if (lastPageText != null) {
            lastPageText.text = (lastPage + 1).ToString();
        }
        prevButton?.SetInteractable(page > firstPage);
        nextButton?.SetInteractable(page < lastPage);
    }

}
