using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIBase : MonoBehaviour
{
   public virtual void Init()
   {
      Button[] btns=GetComponentsInChildren<Button>(true);
      GetComponent<Button>();
      foreach (var button in btns)
      {
         button.onClick.AddListener(() =>
            {
               Debug.Log(button.name);
               OnBtnClick(button.name);
            }
            ); 
      }
   }

   private void OnBtnClick(string buttonName)
   {
      
   }

   public void Show()
   {
      gameObject.SetActive(true);
   }

   public void Hide()
   {
      gameObject.SetActive(false);
   }
}
