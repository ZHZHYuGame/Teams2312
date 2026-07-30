
using Unity.VisualScripting;

public abstract class BaseColltorl<M,V>where M:BaseModel,new() where V:BaseView 
{
    /// <summary>
    /// model实例
    /// </summary>
    public M model { get; private set; }
    /// <summary>
    /// View实例
    /// </summary>
    public V view { get; private set; }
    /// <summary>
    /// 初始化Controller
    /// </summary>
    public void Init()
    {
        model = new M();
        model.Init();
        OnInit();
    }
    /// <summary>
    /// 绑定View
    /// </summary>
    /// <param name="view"></param>
    public void BindView(V view)
    {
        this.view = view;
        this.view.Init();
        OnBindView();
    }
    /// <summary>
    /// 显示View
    /// </summary>
    public void ShowView()
    {
        view?.Show();
    }
    /// <summary>
    /// 隐藏View
    /// </summary>
    public void HideView()
    {
        view?.Hide();
    }

    /// <summary>
    /// 销毁Controller
    /// </summary>
    public void Destroy()
    {
        OnDestroyC();
        model?.Clear();
        view?.Destroy();
    }
    /// <summary>
    /// 刷新View
    /// </summary>
    protected abstract void RefreshView();

    protected virtual void OnInit() { }
    protected virtual void OnBindView() { }
    protected virtual void OnDestroyC() { }

}
