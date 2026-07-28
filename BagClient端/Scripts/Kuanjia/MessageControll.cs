using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MessageControll:Singleton<MessageControll>
{
    Dictionary<int, Action<object>> dict = new Dictionary<int, Action<object>>();

    public void AddListener(int id, Action<object> act)
    {
        if (!dict.ContainsKey(id))
        {
            dict.Add(id, act);
        }
    }

    public void Dispach(int id, params object[] par)
    {
        if (dict.ContainsKey(id))
        {
            dict[id]?.Invoke(par);
        }
    }
}

