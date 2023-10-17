namespace RegisterSystem;

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