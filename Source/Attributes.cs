using System;

namespace RegisterSystem {
    /// <summary>
    /// 给RegisterManage提供默认注册项的一些元数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Field |  AttributeTargets.Property)]
    public class FieldRegisterAttribute : Attribute {
        /// <summary>
        /// 优先级
        /// </summary>
        public int priority;
        
        /// <summary>
        /// 给定一个类型，指向要注册进的管理器
        /// </summary>
        public Type? registerManageType;
    }

    /// <summary>
    /// 自动注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class VoluntarilyRegisterAttribute : System.Attribute {
        /// <summary>
        /// 优先级
        /// </summary>
        public int priority;

        /// <summary>
        /// 自定义的名称
        /// </summary>
        public string customName = String.Empty;

        /// <summary>
        /// 给定一个类型，指向要注册进的管理器
        /// </summary>
        public Type? registerManageType = null;
    }

    /// <summary>
    /// 自动赋值
    /// 作用于RegisterBasics和RegisterManage
    /// 在RegisterManage中voluntarilyRegisterAttribute和registerManage是等同的，寻找逻辑都是根据类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
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

    /// <summary>
    /// 忽略注册字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    public class IgnoreRegisterAttribute : Attribute {
    }
}