/// <summary>
/// 控制器基类 - 协调View和Model
/// </summary>
public class ControllerBase
{
    // 初始化
    public virtual void Init() { }

    // 处理View转发的用户操作
    public virtual void HandleAction(string actionName, object param = null) { }
}
