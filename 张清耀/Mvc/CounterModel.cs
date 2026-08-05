using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CounterModel
{
    public int count = 0;
    public event Action<int> onCounterChanged;

    public void AddCount(int value)
    {
        count += value;
        onCounterChanged?.Invoke(count);
    }
}
