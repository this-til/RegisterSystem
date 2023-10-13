using System;
using System.Diagnostics.CodeAnalysis;

namespace RegisterSystem;

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

public enum VoluntarilyAssignmentType {
    /// <summary>
    /// 根据自动注册类型寻找（直接通过指定类型寻找）
    /// </summary>
    voluntarilyRegisterAttribute,

    /// <summary>
    /// 根据注册管理寻找
    /// </summary>
    registerManage,

    /// <summary>
    /// 以全名称的方式进行查找
    /// </summary>
    allName
}