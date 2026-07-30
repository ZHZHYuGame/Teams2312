using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVC.Core
{
    public interface IEventDispatcher
    {
        void AddListener(string evtName, Action callback);
        void AddListener<T>(string evtName, Action<T> callback);

        void RemoveListener(string evtName,Action callback);
        void RemoveListener<T>(string evtName, Action<T> callback);

        void Dispatch(string evtName);
        void Dispatch<T>(string evtName, T data);

        void ClearEvent();
        void RemoveAllListeners(string evtName);
    }
}
