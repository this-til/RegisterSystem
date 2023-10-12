using System;
using System.Diagnostics.CodeAnalysis;

namespace ReflexAssetsManage; 

/// <summary>
/// 自动赋值
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class VoluntarilyAssignmentAttribute : Attribute {
}