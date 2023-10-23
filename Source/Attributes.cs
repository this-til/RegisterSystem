﻿using System;

namespace RegisterSystem; 

/// <summary>
/// 给RegisterManage提供默认注册项的一些元数据
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class FieldRegisterAttribute : Attribute {
    /// <summary>
    /// 自定义的名称
    /// </summary>
    public string customName = String.Empty;

    /// <summary>
    /// 注册项的类型
    /// </summary>
    public Type? registerType;
}

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

    /// <summary>
    /// 自定义的名称
    /// </summary>
    public string customName = String.Empty;
}

/// <summary>
/// 自动赋值
/// 作用于RegisterBasics和RegisterManage
/// 在RegisterManage中voluntarilyRegisterAttribute和registerManage是等同的，寻找逻辑都是根据类型
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class VoluntarilyAssignmentAttribute : Attribute {
    /// <summary>
    /// 使用指定类型寻找
    /// </summary>
    public Type? appointType;

    /// <summary>
    /// 使用名称寻找
    /// </summary>
    public string name = String.Empty;

    public VoluntarilyAssignmentType voluntarilyAssignmentType = VoluntarilyAssignmentType.voluntarilyRegisterAttribute;
}
