using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterController : MonoBehaviour
{
    [SerializeField] private CounterView view;
    CounterModel model;
    // Start is called before the first frame update
    void Start()
    {
        //初始化model
        model = new CounterModel();//构造modle会调用里面的构造函数
        //初始化View
        view.Init(OnClickAddButton);
        model.onCounterChanged += view.UpdateCountDisplay;
        view.UpdateCountDisplay(model.count);
        
    }

    private void OnClickAddButton()
    {
       model.AddCount(1);
    }

 
}
