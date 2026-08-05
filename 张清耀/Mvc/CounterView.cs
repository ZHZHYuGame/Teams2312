using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterView : MonoBehaviour
{
    [SerializeField] private Text countText;

    [SerializeField] private Button addButton;

    public void Init(Action onClickAddButton)
    {
        addButton.onClick.AddListener((() =>
        {
            onClickAddButton?.Invoke();
        }));
    }

    public void UpdateCountDisplay(int count)
    {
        countText.text = $"Count:{count}";
    }
}
