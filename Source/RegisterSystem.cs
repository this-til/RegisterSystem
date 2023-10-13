using System.Reflection;
using log4net;

namespace RegisterSystem;

public class RegisterSystem {
    /// <summary>
    /// 所有接受管理的程序集
    /// </summary>
    protected readonly HashSet<Assembly> allManagedAssembly = new HashSet<Assembly>();

    /// <summary>
    /// 根据<see cref="RegisterManage"/>类型的映射表
    /// </summary>
    protected readonly Dictionary<Type, RegisterManage> classRegisterManageMap = new Dictionary<Type, RegisterManage>();

    /// <summary>
    /// 根据<see cref="RegisterBasics"/>类型的映射表
    /// </summary>
    protected readonly Dictionary<Type, RegisterManage> registerManageMap = new Dictionary<Type, RegisterManage>();

    /// <summary>
    /// 根据<see cref="RegisterManage.name"/>的映射表
    /// 这里不使用全名称只是因为类管理器的全名称只是给人看的
    /// </summary>
    protected readonly Dictionary<String, RegisterManage> nameRegisterManageMap = new Dictionary<string, RegisterManage>();

    /// <summary>
    /// 标记有<see cref="VoluntarilyRegisterAttribute"/>属性的注册项类型的映射表
    /// </summary>
    protected readonly Dictionary<Type, RegisterBasics> allVoluntarilyRegisterAssetMap = new Dictionary<Type, RegisterBasics>();

    /// <summary>
    /// 根据<see cref="RegisterBasics.completeName"/>的映射表
    /// </summary>
    protected readonly Dictionary<string, RegisterBasics> completeNameRegisterBasicsMap = new Dictionary<string, RegisterBasics>();

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

    protected ILog log = LogManager.GetLogger(typeof(RegisterSystem));

    /// <summary>
    /// 在类型管理被创建完成后的一个回调事件
    /// </summary>
    protected event Action<RegisterManage> registerManageBuildEvent;

    /// <summary>
    /// 类型管理注册完所有的注册项后
    /// </summary>
    protected event Action<RegisterManage> registerManagePutEndEvent;

    protected event Action<RegisterBasics> registerBasicsAwakeInitEvent;

    protected event Action<RegisterBasics> registerBasicsInitEvent;

    /// <summary>
    /// 注册项被注册后时调用
    /// </summary>
    protected event Action<RegisterBasics> registerBasicsPutEvent;

    protected event Action<RegisterBasics> registerBasicsInitBackEvent;

    public bool isInit() => _isInit;

    public RegisterSystem() {
        initAddRegisterManageBuildEvent(r => log.Info($"完成构建类型管理器{r}"));
        initAddRegisterManageBuildEvent(r => nameRegisterManageMap.Add(r.getName(), r));
        initAddRegisterManagePutEndEvent(voluntarilyAssignment);
        initAddRegisterBasicsAwakeInitEvent(r => r.awakeInit());
        initAddRegisterBasicsInitEvent(r => r.init());
        initAddRegisterBasicsPutEvent(r => log.Info($"已经将{r}注册进系统"));
        initAddRegisterBasicsPutEvent(voluntarilyAssignment);
        initAddRegisterBasicsInitBackEvent(r => r.initBack());
    }

    protected void initTest() {
        if (isInit()) {
            throw new Exception("RegisterSystem已经初始化了");
        }
    }

    public void initLog(ILog _log) {
        initTest();
        this.log = _log;
    }

    public void initAddRegisterManageBuildEvent(Action<RegisterManage> action) {
        initTest();
        registerManageBuildEvent += action;
    }

    public void initAddRegisterManagePutEndEvent(Action<RegisterManage> action) {
        initTest();
        registerManagePutEndEvent += action;
    }

    public void initAddRegisterBasicsAwakeInitEvent(Action<RegisterBasics> action) {
        initTest();
        registerBasicsAwakeInitEvent += action;
    }

    public void initAddRegisterBasicsInitEvent(Action<RegisterBasics> action) {
        initTest();
        registerBasicsInitEvent += action;
    }

    public void initAddRegisterBasicsPutEvent(Action<RegisterBasics> action) {
        initTest();
        registerBasicsPutEvent += action;
    }

    public void initAddRegisterBasicsInitBackEvent(Action<RegisterBasics> action) {
        initTest();
        registerBasicsInitBackEvent += action;
    }

    public void initAddAllManagedAssembly(params Assembly[]? assemblies) {
        initTest();
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

        //从程序集中获取所有的类型
        foreach (var assembly in allManagedAssembly) {
            foreach (var type in assembly.GetTypes()) {
                if (!Util.isEffective(type)) {
                    continue;
                }
                allType.Add(type);
            }
        }

        //根据类型进行操作分类
        foreach (var type in allType) {
            if (typeof(RegisterManage).IsAssignableFrom(type)) {
                classRegisterManageMap.Add(type, null!);
            }
            if (typeof(RegisterBasics).IsAssignableFrom(type)) {
                if (type.GetCustomAttribute<VoluntarilyRegisterAttribute>() is null) {
                    continue;
                }
                allVoluntarilyRegisterAssetMap.Add(type, null!);
            }
        }

        //创建类型管理器
        foreach (var type in classRegisterManageMap.Keys) {
            RegisterManage registerManage = Activator.CreateInstance(type) as RegisterManage ?? throw new Exception();
            registerManage_registerSystem.SetValue(registerManage, this);
            //registerManage_name.SetValue(registerManage, Util.ofPath(registerManage.GetType()));
            registerManage_name.SetValue(registerManage, registerManage.GetType().Name);
            classRegisterManageMap[type] = registerManage;
        }

        //对类型管理器进行映射，并且排除冲突
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
                    throw new Exception($"注册管理者冲突,注册类型[{registerType}],冲突管理者[{oldRegisterManage},{registerManage}]");
                }
                continue;
            }
            if (basicsRegisterManage is null) {
                registerManageMap[registerType] = registerManage;
            }
        }

        //对类型管理器赋值名称
        foreach (var registerManage in classRegisterManageMap.Values) {
            registerManage_completeName.SetValue(registerManage, Util.ofCompleteName(registerManage));
        }

        //回调
        foreach (var registerManage in classRegisterManageMap.Values) {
            registerManageBuildEvent(registerManage);
        }

        List<RegisterBasics> registerBasicsList = new List<RegisterBasics>();

        //直接通过定义的静态字段获取注册项
        foreach (var keyValuePair in classRegisterManageMap) {
            foreach (var fieldInfo in keyValuePair.Key.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (!Util.isEffective(fieldInfo)) {
                    continue;
                }
                if (!typeof(RegisterBasics).IsAssignableFrom(fieldInfo.FieldType)) {
                    continue;
                }
                if (fieldInfo.GetCustomAttribute<VoluntarilyAssignmentAttribute>() is not null) {
                    continue;
                }

                FieldRegisterAttribute? fieldRegisterAttribute = fieldInfo.GetCustomAttribute<FieldRegisterAttribute>();

                string name = fieldInfo.Name;
                if (fieldRegisterAttribute is not null && string.IsNullOrEmpty(fieldRegisterAttribute.customName)) {
                    name = fieldRegisterAttribute.customName;
                }

                Type registerType = fieldInfo.FieldType;
                if (fieldRegisterAttribute is not null && fieldRegisterAttribute.registerType is not null) {
                    registerType = fieldRegisterAttribute.registerType;
                    if (!fieldInfo.FieldType.IsAssignableFrom(registerType)) {
                        log.Error($"创建注册项的时候出错,类型不继承{typeof(RegisterBasics)} name:{name},FieldInfo:{fieldInfo},type:{registerType.AssemblyQualifiedName}");
                        continue;
                    }
                }

                if (registerType.IsAbstract) {
                    log.Error($"创建注册项的时候出错,类型为抽象的 name:{name},FieldInfo:{fieldInfo},type:{registerType.AssemblyQualifiedName}");
                    continue;
                }

                RegisterBasics registerBasics = Activator.CreateInstance(fieldInfo.FieldType) as RegisterBasics ?? throw new NullReferenceException();
                fieldInfo.SetValue(fieldInfo.IsStatic ? null : keyValuePair.Value, registerBasics);
                registerBasics_name.SetValue(registerBasics, name);
                registerBasicsList.Add(registerBasics);
            }
        }

        //添加自动注册选项
        foreach (var type in allVoluntarilyRegisterAssetMap.Keys) {
            VoluntarilyRegisterAttribute voluntarilyRegisterAttribute = type.GetCustomAttribute<VoluntarilyRegisterAttribute>() ?? throw new NullReferenceException();
            RegisterBasics registerBasics = Activator.CreateInstance(type) as RegisterBasics ?? throw new NullReferenceException();
            allVoluntarilyRegisterAssetMap[type] = registerBasics;
            string name = type.Name;
            if (string.IsNullOrEmpty(voluntarilyRegisterAttribute.customName)) {
                name = voluntarilyRegisterAttribute.customName;
            }
            registerBasics_name.SetValue(registerBasics, name);
            registerBasics_priority.SetValue(registerBasics, voluntarilyRegisterAttribute.priority);
            registerBasicsList.Add(registerBasics);
        }

        //从类型管理器中获取默认注册项
        foreach (var registerManage in classRegisterManageMap.Values) {
            registerBasicsList.AddRange(registerManage.getDefaultRegisterItem());
        }

        unifyRegister(registerBasicsList);

        foreach (var registerManage in classRegisterManageMap.Values) {
            registerManagePutEndEvent(registerManage);
        }

        foreach (var registerManage in classRegisterManageMap.Values) {
            foreach (var keyValuePair in registerManage.forAll_erase()) {
                registerBasicsInitBackEvent(keyValuePair.Value);
            }
        }
    }

    /// <summary>
    /// 统一注册
    /// </summary>
    protected void unifyRegister(List<RegisterBasics> registerBasicsList) {
        registerBasicsList = new List<RegisterBasics>(registerBasicsList);
        List<RegisterBasics> needRegisterList = new List<RegisterBasics>();
        for (var index = 0; index < registerBasicsList.Count; index++) {
            var registerBasics = registerBasicsList[index];
            registerBasics_registerSystem.SetValue(registerBasics, this);
            RegisterManage? registerManage = getRegisterManageOfRegisterType(registerBasics.GetType());
            if (registerManage is null) {
                log.Error($"注册{typeof(RegisterBasics)}时没有找到对应的{typeof(RegisterManage)},{typeof(RegisterBasics)}:{registerBasics.getName()},type:{registerBasics.GetType()}");
                registerBasicsList.RemoveAt(index);
                index--;
                continue;
            }
            registerBasics_registerManage.SetValue(registerBasics, registerManage);
            registerBasics_completeName.SetValue(registerBasics, $"{registerManage.getCompleteName()}@{registerBasics.getName()}");
        }
        registerBasicsList.Sort((a, b) => {
            if (!a.getRegisterManage().Equals(b.getRegisterManage())) {
                return a.getRegisterManage().getPriority() - b.getRegisterManage().getPriority();
            }
            return a.getPriority() - b.getPriority();
        });
        foreach (var registerBasics in registerBasicsList) {
            registerBasicsAwakeInitEvent(registerBasics);
        }
        foreach (var registerBasics in registerBasicsList) {
            registerBasics.getRegisterManage().put(registerBasics, false);
            completeNameRegisterBasicsMap.Add(registerBasics.getCompleteName(), registerBasics);
        }
        foreach (var registerBasics in registerBasicsList) {
            registerBasicsPutEvent(registerBasics);
        }
        foreach (var registerBasics in registerBasicsList) {
            registerBasicsInitEvent(registerBasics);
        }
        foreach (var registerBasics in registerBasicsList) {
            foreach (var keyValuePair in registerBasics.getAdditionalRegister()) {
                needRegisterList.Add(keyValuePair.Key);
                registerBasics_name.SetValue(keyValuePair.Value, $"{registerBasics.getName()}/{keyValuePair.Value}");
            }
        }
        if (needRegisterList.Count > 0) {
            unifyRegister(registerBasicsList);
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

    public RegisterManage? getRegisterManageOfRegisterType(Type type) {
        Type? basType = type;
        while (basType is not null) {
            if (registerManageMap.ContainsKey(basType)) {
                return registerManageMap[basType];
            }
            basType = basType.BaseType;
        }
        return null;
        //throw new Exception($"未找到目标类型为[{type}]注册管理器");
    }

    public RegisterManage? getRegisterManageOfRegisterType<T>() {
        return getRegisterManageOfRegisterType(typeof(T));
    }

    public RegisterManage? getRegisterManageOfName(string name) {
        if (nameRegisterManageMap.ContainsKey(name)) {
            return nameRegisterManageMap[name];
        }
        return null;
    }

    public RegisterManage? getRegisterManageOfVoluntarilyAssignment(FieldInfo fieldInfo, VoluntarilyAssignmentAttribute voluntarilyAssignmentAttribute) {
        switch (voluntarilyAssignmentAttribute.voluntarilyAssignmentType) {
            case VoluntarilyAssignmentType.voluntarilyRegisterAttribute:
            case VoluntarilyAssignmentType.registerManage:
                Type registerManageType = voluntarilyAssignmentAttribute.appointType ?? fieldInfo.FieldType;
                RegisterManage? registerManage;
                if (registerManageType.IsGenericType) {
                    Type registerBasicsType = registerManageType.GenericTypeArguments[0];
                    registerManage = getRegisterManageOfRegisterType(registerBasicsType);
                }
                else {
                    registerManage = getRegisterManageOfManageType(registerManageType);
                }
                return registerManage;
            case VoluntarilyAssignmentType.allName:
                return getRegisterManageOfName(voluntarilyAssignmentAttribute.name);
            default:
                return null;
        }
    }

    public RegisterBasics? getRegisterBasicsOfVoluntarilyRegister(Type type) {
        if (allVoluntarilyRegisterAssetMap.ContainsKey(type)) {
            return allVoluntarilyRegisterAssetMap[type];
        }
        return null;
    }

    public T? getRegisterBasicsOfVoluntarilyRegister<T>() where T : RegisterBasics {
        return getRegisterBasicsOfVoluntarilyRegister(typeof(T)) as T;
    }

    public RegisterBasics? getRegisterBasicsOfCompleteName(string name) {
        if (completeNameRegisterBasicsMap.ContainsKey(name)) {
            return completeNameRegisterBasicsMap[name];
        }
        return null;
    }

    public RegisterBasics? getRegisterBasicsOfVoluntarilyAssignment(FieldInfo fieldInfo, VoluntarilyAssignmentAttribute voluntarilyAssignmentAttribute) {
        switch (voluntarilyAssignmentAttribute.voluntarilyAssignmentType) {
            case VoluntarilyAssignmentType.voluntarilyRegisterAttribute:
                return getRegisterBasicsOfVoluntarilyRegister(voluntarilyAssignmentAttribute.appointType ?? fieldInfo.FieldType);
            case VoluntarilyAssignmentType.registerManage:
                RegisterManage? registerManage = getRegisterManageOfManageType(voluntarilyAssignmentAttribute.appointType ?? fieldInfo.FieldType);
                if (registerManage is null) {
                    return null;
                }
                return registerManage.get_erase(voluntarilyAssignmentAttribute.name);
            case VoluntarilyAssignmentType.allName:
                return getRegisterBasicsOfCompleteName(voluntarilyAssignmentAttribute.name);
            default:
                return null;
        }
    }

    /// <summary>
    /// 自动填补对象中的自动赋值属性
    /// </summary>
    public void voluntarilyAssignment(object obj) {
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
        bindingFlags |= obj is Type ? BindingFlags.Static : BindingFlags.Instance;
        foreach (var fieldInfo in obj.GetType().GetFields(bindingFlags)) {
            if (!Util.isEffective(fieldInfo)) {
                continue;
            }
            VoluntarilyAssignmentAttribute? voluntarilyAssignmentAttribute = fieldInfo.GetCustomAttribute<VoluntarilyAssignmentAttribute>();
            if (voluntarilyAssignmentAttribute is null) {
                continue;
            }
            if (typeof(RegisterBasics).IsAssignableFrom(obj.GetType())) {
                fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, getRegisterBasicsOfVoluntarilyAssignment(fieldInfo, voluntarilyAssignmentAttribute));
                continue;
            }
            if (typeof(RegisterManage).IsAssignableFrom(obj.GetType())) {
                fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, getRegisterManageOfVoluntarilyAssignment(fieldInfo, voluntarilyAssignmentAttribute));
            }
        }
    }

    public ILog getLog() => log;
}