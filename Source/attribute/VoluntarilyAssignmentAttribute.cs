using System;
using System.Diagnostics.CodeAnalysis;

namespace RegisterSystem; 

/// <summary>
/// 自动赋值
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class VoluntarilyAssignmentAttribute : Attribute {
}