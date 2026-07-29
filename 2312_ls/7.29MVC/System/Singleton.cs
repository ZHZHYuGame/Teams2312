using System;
using System.Threading;

/// <summary>
/// 单例基类 - 线程安全的懒加载单例
/// </summary>
public class Singleton<T> where T : class, new()
{
    private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());

    public static T Ins
    {
        get { return _instance.Value; }
    }
}
