using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPageHandler
{
    public int page { get; }
    public int lastPage { get; }

    public void SetPage(int newPage);
    public void PrevPage();
    public void NextPage();

}
