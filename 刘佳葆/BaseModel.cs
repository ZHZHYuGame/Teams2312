
/// <summary>
/// Model基类
/// </summary>
public abstract class BaseModel 
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public virtual void Init() { }
    /// <summary>
    /// 清理数据
    /// </summary>
    public virtual void Clear() { }

}
/// <summary>
/// 泛型model基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseModel<T> : BaseModel where T : class, new()
{
    /// <summary>
    /// 数据对象
    /// </summary>
    public T data { get; protected set; } = new T();
}
