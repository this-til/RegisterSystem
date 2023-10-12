using System;

namespace RegisterSystem; 

/// <summary>
/// 自动注册
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class VoluntarilyRegisterAttribute : System.Attribute {
    
    /// <summary>
    /// 优先级
    /// 通过反射
    /// </summary>
    public int priority;
    
    public string customName;
}