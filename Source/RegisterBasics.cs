namespace RegisterSystem;

public class RegisterBasics {
    /// <summary>
    /// 注册项的完整的名称
    /// 由管理系统进行反射赋值
    /// </summary>
    protected string completeName;

    /// <summary>
    /// 注册项的名称
    /// 使用此名称进行注册key
    /// 由管理系统进行反射赋值
    /// </summary>
    protected string name;

    /// <summary>
    /// 同一类型的管理管理
    /// 由管理系统进行反射赋值
    /// </summary>
    protected RegisterManage registerManage;

    /// <summary>
    /// 对应的注册管理系统
    /// 由管理系统进行反射赋值
    /// </summary>
    protected RegisterSystem registerSystem;

    /// <summary>
    /// 初始化时候的优先级
    /// </summary>
    protected int priority;

    /// <summary>
    /// 最早的初始化方法
    /// </summary>
    internal virtual void awakeInit() {
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    internal virtual void init() {
    }

    /// <summary>
    /// 初始化结束后统一调用
    /// </summary>
    internal virtual void initBack() {
    }

    /// <summary>
    /// 获取附加的注册项目
    /// </summary>
    internal IEnumerable<KeyValuePair<RegisterBasics, string>> getAdditionalRegister() {
        yield break;
    }

    public string getCompleteName() => completeName;

    public string getName() => name;

    public RegisterManage getRegisterManage() => registerManage;

    public RegisterSystem getRegisterSystem() => registerSystem;

    public int getPriority() => priority;

    public override string ToString() {
        return completeName;
    }
}