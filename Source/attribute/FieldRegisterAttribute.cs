namespace RegisterSystem;

/// <summary>
/// 给RegisterManage提供默认注册项的一些元数据
/// </summary>
public class FieldRegisterAttribute : Attribute {
    /// <summary>
    /// 自定义的名称
    /// </summary>
    public string customName;

    /// <summary>
    /// 注册项的类型
    /// </summary>
    public Type? registerType;
}