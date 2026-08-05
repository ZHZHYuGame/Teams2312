using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameText : MonoBehaviour
{
  public SelectHScrollow Scrollow;

  private void Awake()
  {
    if (Scrollow!=null)
    {
      int index;
      int num = 100;
      string[] names = new string[num];
      for (int i = 0; i < num; i++)
      {
        names[i]=(i+1).ToString();
        index = i;
      }
      Scrollow.SetItemsInfo(names);
      Scrollow.SelectAction += (index) =>
      {
        print(index);//通过下标用来处理逻辑
        Debug.Log(index);
      };
    }
  }
}
