using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MVC.Core
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<string,Delegate> _eventDict = new Dictionary<string, Delegate>();
        public void AddListener(string evtName, Action callback)
        {
            Register(evtName,callback);
        }
        public void AddListener<T>(string evtName, Action<T> callback)
        {
            Register(evtName, callback);
        }
        public void RemoveListener(string evtName, Action callback)
        {
            UnRegister(evtName,callback);
        }
        public void RemoveListener<T>(string evtName, Action<T> callback)
        {
            UnRegister(evtName, callback);
        }
        public void Dispatch(string evtName)
        {
            if (!_eventDict.TryGetValue(evtName, out var del)) return;
            if (del is Action act) act.Invoke();
        }

        public void Dispatch<T>(string evtName, T data)
        {
            if (!_eventDict.TryGetValue(evtName, out var del)) return;
            if (del is Action<T> act) act.Invoke(data);
        }
        public void ClearEvent()
        {
            _eventDict.Clear();
        }
        public void RemoveAllListeners(string evtName)
        {
            if(_eventDict.ContainsKey(evtName))
                _eventDict.Remove(evtName);
        }
        private void Register(string evtName, Delegate callback)
        {
            if (!_eventDict.ContainsKey(evtName))
            {
                _eventDict[evtName] = callback;
                return;
            }
            _eventDict[evtName] = Delegate.Combine(_eventDict[evtName],callback);
        }

        private void UnRegister(string evtName, Delegate callback)
        {
            if(!_eventDict.TryGetValue(evtName,out var del)) return;
            Delegate newDel = Delegate.Remove(del,callback);
            if(newDel == null)
                _eventDict.Remove(evtName);
            else
                _eventDict[evtName] = newDel;
        }
    }
}