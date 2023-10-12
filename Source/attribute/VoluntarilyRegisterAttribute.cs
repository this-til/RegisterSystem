using System;

namespace ReflexAssetsManage; 

/// <summary>
/// 自动注册
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class VoluntarilyRegisterAttribute : System.Attribute {
    public int priority;
}