using System.Reflection;

namespace RegisterSystem;

public class RegisterSystem {
    /// <summary>
    /// 所有接受管理的程序集
    /// </summary>
    protected readonly HashSet<Assembly> allManagedAssembly = new HashSet<Assembly>();

    /// <summary>
    ///  根据RegisterManage的类型映射
    /// </summary>
    protected readonly Dictionary<Type, RegisterManage> classRegisterManageMap = new Dictionary<Type, RegisterManage>();

    /// <summary>
    /// 根据RegisterManage的管理类型映射
    /// </summary>
    protected readonly Dictionary<Type, RegisterManage> registerManageMap = new Dictionary<Type, RegisterManage>();

    /// <summary>
    /// 自动注册的类型
    /// </summary>
    protected readonly Dictionary<Type, RegisterBasics> allVoluntarilyRegisterAssetMap = new Dictionary<Type, RegisterBasics>();

    protected readonly List<Type> allType = new List<Type>();

    protected readonly FieldInfo registerManage_registerSystem = typeof(RegisterManage).GetField("registerSystem") ?? throw new Exception();
    protected readonly FieldInfo registerManage_name = typeof(RegisterManage).GetField("name") ?? throw new Exception();
    protected readonly FieldInfo registerManage_completeName = typeof(RegisterManage).GetField("completeName") ?? throw new Exception();
    protected readonly FieldInfo registerManage_basicsRegisterManage = typeof(RegisterManage).GetField("basicsRegisterManage") ?? throw new Exception();

    protected readonly FieldInfo registerBasics_registerManage = typeof(RegisterBasics).GetField("registerManage") ?? throw new Exception();
    protected readonly FieldInfo registerBasics_name = typeof(RegisterBasics).GetField("name") ?? throw new Exception();
    protected readonly FieldInfo registerBasics_completeName = typeof(RegisterBasics).GetField("completeName") ?? throw new Exception();
    protected readonly FieldInfo registerBasics_registerSystem = typeof(RegisterBasics).GetField("registerSystem") ?? throw new Exception();
    protected readonly FieldInfo registerBasics_priority = typeof(RegisterBasics).GetField("priority") ?? throw new Exception();

    /// <summary>
    /// 一个标志，
    /// 声明如果初始化了将禁用一些方法
    /// </summary>
    protected bool _isInit;

    /// <summary>
    /// 在类型管理被创建完成后的一个回调事件
    /// </summary>
    protected event Action<RegisterManage> registerManageBuildBack = r => {
        // TODO log
    };

    protected event Action<RegisterBasics> registerBasicsBuildBack = r => {
        // TODO log
    };

    public bool isInit() => _isInit;

    public void initAddAllManagedAssembly(params Assembly[]? assemblies) {
        if (isInit()) {
            throw new Exception("ReflexAssetsManage已经初始化了");
        }
        if (assemblies is null) {
            return;
        }
        foreach (var assembly in assemblies) {
            allManagedAssembly.Add(assembly);
        }
    }

    public void initRegisterSystem() {
        if (isInit()) {
            throw new Exception("RegisterSystem已经初始化过了");
        }
        _isInit = true;

        foreach (var assembly in allManagedAssembly) {
            foreach (var type in assembly.GetTypes()) {
                if (!Util.isEffective(type)) {
                    continue;
                }
                allType.Add(type);
            }
        }

        foreach (var type in allType) {
            if (typeof(RegisterManage).IsAssignableFrom(type)) {
                classRegisterManageMap.Add(type, null);
            }
            if (typeof(RegisterBasics).IsAssignableFrom(type)) {
                if (type.GetCustomAttribute<VoluntarilyRegisterAttribute>() is null) {
                    continue;
                }
                allVoluntarilyRegisterAssetMap.Add(type, null);
            }
        }

        foreach (var type in classRegisterManageMap.Keys) {
            RegisterManage registerManage = Activator.CreateInstance(type) as RegisterManage ?? throw new Exception();
            registerManage_registerSystem.SetValue(registerManage, this);
            registerManage_name.SetValue(registerManage, Util.ofPath(registerManage.GetType()));
            classRegisterManageMap[type] = registerManage;
        }

        foreach (var registerManage in classRegisterManageMap.Values) {
            Type registerType = registerManage.getRegisterType();
            Type? basicsRegisterManageType = registerManage.getBasicsRegisterManageType();

            RegisterManage? basicsRegisterManage = getRegisterManageOfManageType(basicsRegisterManageType);

            if (basicsRegisterManage is not null) {
                registerManage_basicsRegisterManage.SetValue(registerManage, basicsRegisterManage);
            }
            
            if (!registerManageMap.ContainsKey(registerType)) {
                registerManageMap.Add(registerType, registerManage);
                continue;
            }
            RegisterManage oldRegisterManage = registerManageMap[registerType];
            if (oldRegisterManage.getBasicsRegisterManage() is null) {
                if (basicsRegisterManage is null) {
                    throw new Exception($"注册管理者冲突，注册类型[{registerType}]，冲突管理者[{oldRegisterManage},{registerManage}]");
                }
                continue;
            }
            if (basicsRegisterManage is null) {
                registerManageMap[registerType] = registerManage;
            }
            
        }

        foreach (var registerManage in classRegisterManageMap.Values) {
            registerManage_completeName.SetValue(registerManage, Util.ofCompleteName(registerManage));
        }

        foreach (var keyValuePair in classRegisterManageMap) {
            registerManageBuildBack(keyValuePair.Value);
        }
        
        

    }

    public RegisterManage? getRegisterManageOfManageType(Type? registerManageClass) {
        if (registerManageClass is null) {
            return null;
        }
        if (!classRegisterManageMap.ContainsKey(registerManageClass)) {
            return null;
        }
        return classRegisterManageMap[registerManageClass];
    }

    public T? getRegisterManageOfManageType<T>() where T : RegisterManage {
        return getRegisterManageOfManageType(typeof(T)) as T;
    }

    public RegisterManage getRegisterManageOfRegisterType(Type type) {
        Type? basType = type;
        while (basType is not null) {
            if (registerManageMap.ContainsKey(basType)) {
                return registerManageMap[basType];
            }
            basType = basType.BaseType;
        }
        throw new Exception($"未找到目标类型为[{type}]注册管理器");
    }

    public RegisterManage getRegisterManageOfRegisterType<T>() {
        return getRegisterManageOfRegisterType(typeof(T));
    }
}