/// <summary>
/// 通用泛型单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    protected static T instance;
    private static readonly object locker = new object();

    protected Singleton()
    {
    }

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new T();
                    }
                }
            }

            return instance;
        }
    }
}