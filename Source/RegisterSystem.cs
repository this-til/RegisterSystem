using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RegisterSystem {
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

        //protected static readonly BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        //protected readonly FieldInfo registerManage_registerSystem = typeof(RegisterManage).GetField("registerSystem", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerManage_name = typeof(RegisterManage).GetField("name", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerManage_completeName = typeof(RegisterManage).GetField("completeName", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerManage_basicsRegisterManage = typeof(RegisterManage).GetField("basicsRegisterManage", bindingFlags) ?? throw new Exception();

        //protected readonly FieldInfo registerBasics_registerManage = typeof(RegisterBasics).GetField("registerManage", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerBasics_name = typeof(RegisterBasics).GetField("name", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerBasics_completeName = typeof(RegisterBasics).GetField("completeName", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerBasics_registerSystem = typeof(RegisterBasics).GetField("registerSystem", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerBasics_priority = typeof(RegisterBasics).GetField("priority", bindingFlags) ?? throw new Exception();
        //protected readonly FieldInfo registerBasics_isInit = typeof(RegisterBasics).GetField("_isInit", bindingFlags) ?? throw new Exception();

        /// <summary>
        /// 一个标志，
        /// 声明如果初始化了将禁用一些方法
        /// </summary>
        protected bool _isInit;

        protected ILogOut? log;

        /// <summary>
        /// 在类型管理被创建完成后的一个回调事件
        /// </summary>
        protected event Action<RegisterManage> registerManageAwakeInitEvent;

        protected event Action<RegisterManage> registerManageInitEvent;

        protected event Action<RegisterManage> registerManageInitSecondEvent;

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

        protected event Action<RegisterBasics> registerBasicsInitEndEvent;

        public bool isInit() => _isInit;

        public RegisterSystem() {
            initAddRegisterManageAwakeInitEvent(r => r.awakeInit());
            initAddRegisterManageAwakeInitEvent(r => getLog()?.Info($"完成构建类型管理器{r}"));
            initAddRegisterManageAwakeInitEvent(r => nameRegisterManageMap.Add(r.getName(), r));
            initAddRegisterManageInitEvent(r => r.init());
            initAddRegisterManageInitSecondEvent(r => r.initSecond());
            initAddRegisterManagePutEndEvent(voluntarilyAssignment);
            initAddRegisterManagePutEndEvent(r => r.isInitEnd = true);
            initAddRegisterManagePutEndEvent(r => r.initEnd());
            initAddRegisterBasicsAwakeInitEvent(r => r.awakeInitFieldRegister());
            initAddRegisterBasicsAwakeInitEvent(r => r.awakeInit());
            initAddRegisterBasicsInitEvent(r => r.init());
            initAddRegisterBasicsPutEvent(r => getLog()?.Info($"已经将{r}注册进系统"));
            initAddRegisterBasicsPutEvent(voluntarilyAssignment);
            initAddRegisterBasicsInitEndEvent(r => r.isInitEnd = true);
            initAddRegisterBasicsInitEndEvent(r => r.initEnd());
        }

        protected void initTest() {
            if (isInit()) {
                throw new Exception("RegisterSystem已经初始化了");
            }
        }

        public void initLog(ILogOut _log) {
            initTest();
            this.log = _log;
        }

        public void initAddRegisterManageAwakeInitEvent(Action<RegisterManage> action) {
            initTest();
            registerManageAwakeInitEvent += action;
        }

        public void initAddRegisterManageInitEvent(Action<RegisterManage> action) {
            initTest();
            registerManageInitEvent += action;
        }

        public void initAddRegisterManageInitSecondEvent(Action<RegisterManage> action) {
            initTest();
            registerManageInitSecondEvent += action;
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

        public void initAddRegisterBasicsInitEndEvent(Action<RegisterBasics> action) {
            initTest();
            registerBasicsInitEndEvent += action;
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
            initTest();
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
            foreach (var type in classRegisterManageMap.Keys.ToArray()) {
                RegisterManage registerManage = Activator.CreateInstance(type) as RegisterManage ?? throw new Exception();
                registerManage.registerSystem = this;
                registerManage.name = registerManage.GetType().Name;
                classRegisterManageMap[type] = registerManage;
            }

            //对类型管理器进行映射，并且排除冲突
            foreach (var registerManage in classRegisterManageMap.Values) {
                Type registerType = registerManage.getRegisterType();
                Type? basicsRegisterManageType = registerManage.getBasicsRegisterManageType();

                RegisterManage? basicsRegisterManage = getRegisterManageOfManageType(basicsRegisterManageType);

                if (basicsRegisterManage is not null) {
                    bool conflict = !basicsRegisterManage.getRegisterType().IsAssignableFrom(registerType);
                    if (basicsRegisterManage.getRegisterType().IsGenericType && registerType.IsGenericType) {
                        conflict |= !basicsRegisterManage.getRegisterType().GetGenericTypeDefinition().IsAssignableFrom(registerType.GetGenericTypeDefinition());
                    }
                    if (conflict) {
                        throw new Exception($"注册管理者父类型错误," +
                                            $"RegisterManage:{registerManage}" +
                                            $"BasicsRegisterManage:{basicsRegisterManage}");
                    }
                }

                registerManage.basicsRegisterManage = basicsRegisterManage;

                if (!registerManageMap.ContainsKey(registerType)) {
                    registerManageMap.Add(registerType, registerManage);
                    continue;
                }
                RegisterManage oldRegisterManage = registerManageMap[registerType];
                if (oldRegisterManage.getBasicsRegisterManage() is null) {
                    if (basicsRegisterManage is null) {
                        throw new Exception($"注册管理者冲突," +
                                            $"registerType:{registerType}," +
                                            $"RegisterManage[{oldRegisterManage},{registerManage}]");
                    }
                    continue;
                }
                if (basicsRegisterManage is null) {
                    registerManageMap[registerType] = registerManage;
                }
            }

            //对类型管理器赋值名称
            foreach (var registerManage in classRegisterManageMap.Values) {
                registerManage.completeName = Util.ofCompleteName(registerManage);
            }

            //回调
            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManageAwakeInitEvent(registerManage);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerManageAwakeInitEvent时发生错误,RegisterManage:{registerManage}");
                    getLog()?.Error(e);
                }
            }

            List<RegisterBasicsMetadata> registerBasicsMetadata = new List<RegisterBasicsMetadata>();

            //直接通过定义的静态字段获取注册项
            foreach (var keyValuePair in classRegisterManageMap) {
                foreach (var propertyInfo in keyValuePair.Key.GetProperties(BindingFlags.Static | BindingFlags.Public)) {
                    if (!Util.isEffective(propertyInfo)) {
                        continue;
                    }
                    if (!typeof(RegisterBasics).IsAssignableFrom(propertyInfo.PropertyType)) {
                        continue;
                    }

                    FieldRegisterAttribute? fieldRegisterAttribute = propertyInfo.GetCustomAttribute<FieldRegisterAttribute>();

                    string name = propertyInfo.Name;
                    if (fieldRegisterAttribute is not null && !string.IsNullOrEmpty(fieldRegisterAttribute.customName)) {
                        name = fieldRegisterAttribute.customName;
                    }

                    Type registerType = propertyInfo.PropertyType;
                    if (fieldRegisterAttribute is not null && fieldRegisterAttribute.registerType is not null) {
                        registerType = fieldRegisterAttribute.registerType;
                        if (!propertyInfo.PropertyType.IsAssignableFrom(registerType)) {
                            getLog()?.Error($"创建注册项的时候出错,类型不继承{typeof(RegisterBasics)} name:{name},PropertyInfo:{propertyInfo},type:{registerType.AssemblyQualifiedName}");
                            continue;
                        }
                    }

                    if (registerType.IsAbstract) {
                        getLog()?.Error($"创建注册项的时候出错,类型为抽象的 name:{name},PropertyInfo:{propertyInfo},type:{registerType.AssemblyQualifiedName}");
                        continue;
                    }

                    RegisterBasics registerBasics = Activator.CreateInstance(registerType) as RegisterBasics ?? throw new NullReferenceException();
                    propertyInfo.SetValue(null, registerBasics);
                    registerBasicsMetadata.Add(new RegisterBasicsMetadata() {
                        registerBasics = registerBasics,
                        name = name,
                        priority = fieldRegisterAttribute?.priority ?? 0,
                        registerManage = keyValuePair.Value
                    });
                }

                foreach (var fieldInfo in keyValuePair.Key.GetFields(BindingFlags.Static | BindingFlags.Public)) {
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
                            getLog()?.Error($"创建注册项的时候出错,类型不继承{typeof(RegisterBasics)} name:{name},FieldInfo:{fieldInfo},type:{registerType.AssemblyQualifiedName}");
                            continue;
                        }
                    }

                    if (registerType.IsAbstract) {
                        getLog()?.Error($"创建注册项的时候出错,类型为抽象的 name:{name},FieldInfo:{fieldInfo},type:{registerType.AssemblyQualifiedName}");
                        continue;
                    }
                    RegisterBasics registerBasics = Activator.CreateInstance(registerType) as RegisterBasics ?? throw new NullReferenceException();
                    fieldInfo.SetValue(null, registerBasics);
                    registerBasicsMetadata.Add(new RegisterBasicsMetadata() {
                        registerBasics = registerBasics,
                        name = name,
                        priority = (fieldRegisterAttribute?.priority ?? 0),
                        registerManage = keyValuePair.Value
                    });
                }
            }

            //添加自动注册选项
            foreach (var type in allVoluntarilyRegisterAssetMap.Keys) {
                VoluntarilyRegisterAttribute voluntarilyRegisterAttribute = type.GetCustomAttribute<VoluntarilyRegisterAttribute>() ?? throw new NullReferenceException();
                RegisterBasics registerBasics = Activator.CreateInstance(type) as RegisterBasics ?? throw new NullReferenceException();
                allVoluntarilyRegisterAssetMap[type] = registerBasics;
                registerBasicsMetadata.Add(new RegisterBasicsMetadata() {
                    registerBasics = registerBasics,
                    name = string.IsNullOrEmpty(voluntarilyRegisterAttribute.customName) ? type.Name : voluntarilyRegisterAttribute.customName,
                    priority = voluntarilyRegisterAttribute.priority,
                    registerManageType = voluntarilyRegisterAttribute.registerManageType
                });
            }

            //从类型管理器中获取默认注册项
            foreach (var registerManage in classRegisterManageMap.Values) {
                registerBasicsMetadata.AddRange(registerManage.getDefaultRegisterItem());
            }

            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManageInitEvent(registerManage);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerManageInitEvent时发生错误,RegisterManage:{registerManage}");
                    getLog()?.Error(e);
                }
            }

            foreach (var basicsMetadata in registerBasicsMetadata) {
                mateRegisterBasics(basicsMetadata);
            }

            unifyRegister(registerBasicsMetadata.Select(m => m.registerBasics).ToList());

            List<RegisterBasics> secondRegisterBasicList = new List<RegisterBasics>();
            foreach (var registerManage in classRegisterManageMap.Values) {
                foreach (var basicsMetadata in registerManage.getSecondDefaultRegisterItem()) {
                    mateRegisterBasics(basicsMetadata);
                    secondRegisterBasicList.Add(basicsMetadata.registerBasics);
                }
            }

            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManageInitSecondEvent(registerManage);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerManageInitSecondEvent时发生错误,RegisterManage:{registerManage}");
                    getLog()?.Error(e);
                }
            }

            if (secondRegisterBasicList.Count > 0) {
                unifyRegister(secondRegisterBasicList);
            }

            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManagePutEndEvent(registerManage);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerManagePutEndEvent时发生错误,RegisterManage:{registerManage}");
                    getLog()?.Error(e);
                }
            }

            foreach (var registerManage in classRegisterManageMap.Values) {
                foreach (var registerBasics in registerManage.forAll_erase()) {
                    try {
                        registerBasicsInitEndEvent(registerBasics);
                    }
                    catch (Exception e) {
                        getLog()?.Error($"registerBasicsInitEndEvent时发生错误,registerBasics:{registerBasics}");
                        getLog()?.Error(e);
                    }
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
                registerBasics.registerSystem = this;
                registerBasics.completeName = $"{registerBasics.getRegisterManage().getCompleteName()}@{registerBasics.getName()}";
                if (registerBasics.getRegisterManage() is null) {
                    getLog()?.Error(
                        $"注册{typeof(RegisterBasics)}时没有找到对应的{typeof(RegisterManage)},{typeof(RegisterBasics)}:{registerBasics.getName()},type:{registerBasics.GetType()}");
                    registerBasicsList.RemoveAt(index);
                    index--;
                }
            }

            ReverseComparer<int> comparer = new ReverseComparer<int>();
            SortedDictionary<int, SortedDictionary<int, List<RegisterBasics>>> dictionary = new SortedDictionary<int, SortedDictionary<int, List<RegisterBasics>>>(comparer);
            foreach (var registerBasics in registerBasicsList) {
                SortedDictionary<int, List<RegisterBasics>> sortedDictionary;
                if (dictionary.ContainsKey(registerBasics.registerManage.getPriority())) {
                    sortedDictionary = dictionary[registerBasics.registerManage.getPriority()];
                }
                else {
                    sortedDictionary = new SortedDictionary<int, List<RegisterBasics>>(comparer);
                    dictionary.Add(registerBasics.registerManage.getPriority(), sortedDictionary);
                }
                List<RegisterBasics> list;
                if (sortedDictionary.ContainsKey(registerBasics.getPriority())) {
                    list = sortedDictionary[registerBasics.getPriority()];
                }
                else {
                    list = new List<RegisterBasics>();
                    sortedDictionary.Add(registerBasics.getPriority(), list);
                }
                list.Add(registerBasics);
            }
            registerBasicsList = new List<RegisterBasics>(registerBasicsList.Count);
            foreach (var keyValuePair in dictionary) {
                foreach (var valuePair in keyValuePair.Value) {
                    registerBasicsList.AddRange(valuePair.Value);
                }
            }

            foreach (var registerBasics in registerBasicsList) {
                try {
                    registerBasicsAwakeInitEvent(registerBasics);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerBasicsAwakeInitEvent时发生错误,registerBasics:{registerBasics}");
                    getLog()?.Error(e);
                }
            }
            foreach (var registerBasics in registerBasicsList) {
                registerBasics.getRegisterManage().put(registerBasics, false);
                completeNameRegisterBasicsMap.Add(registerBasics.getCompleteName(), registerBasics);
            }
            foreach (var registerBasics in registerBasicsList) {
                try {
                    registerBasicsPutEvent(registerBasics);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerBasicsPutEvent时发生错误,registerBasics:{registerBasics}");
                    getLog()?.Error(e);
                }
            }
            foreach (var registerBasics in registerBasicsList) {
                try {
                    registerBasicsInitEvent(registerBasics);
                }
                catch (Exception e) {
                    getLog()?.Error($"registerBasicsInitEvent时发生错误,registerBasics:{registerBasics}");
                    getLog()?.Error(e);
                }
            }
            foreach (var registerBasics in registerBasicsList) {
                /*foreach (var keyValuePair in registerBasics.getAdditionalRegister()) {
                    needRegisterList.Add(keyValuePair.Key);
                    registerBasics_name.SetValue(keyValuePair.Key, $"{registerBasics.getName()}/{keyValuePair.Value}");
                }*/
                foreach (var registerBasicsMetadata in registerBasics.getAdditionalRegister()) {
                    mateRegisterBasics(registerBasicsMetadata);
                    needRegisterList.Add(registerBasicsMetadata.registerBasics);
                }
            }
            if (needRegisterList.Count > 0) {
                unifyRegister(needRegisterList);
            }
        }

        protected void mateRegisterBasics(RegisterBasicsMetadata registerBasicsMetadata) {
            registerBasicsMetadata.registerBasics.name = registerBasicsMetadata.name;
            registerBasicsMetadata.registerBasics.priority = registerBasicsMetadata.priority;
            RegisterManage? registerManage = registerBasicsMetadata.registerManage ??
                                             (registerBasicsMetadata.registerManageType is not null
                                                 ? getRegisterManageOfManageType(registerBasicsMetadata.registerManageType)
                                                 : getRegisterManageOfRegisterType(registerBasicsMetadata.registerBasics.GetType()));
            registerBasicsMetadata.registerBasics.registerManage = registerManage;
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

        public RegisterManage? getRegisterManageOfVoluntarilyAssignment(FieldInfo fieldInfo, VoluntarilyAssignmentAttribute? voluntarilyAssignmentAttribute) {
            VoluntarilyAssignmentType voluntarilyAssignmentType =
                voluntarilyAssignmentAttribute?.voluntarilyAssignmentType ?? VoluntarilyAssignmentType.voluntarilyRegisterAttribute;
            switch (voluntarilyAssignmentType) {
                case VoluntarilyAssignmentType.voluntarilyRegisterAttribute:
                case VoluntarilyAssignmentType.registerManage:
                    Type registerManageType = voluntarilyAssignmentAttribute?.appointType ?? fieldInfo.FieldType;
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
                    return getRegisterManageOfName(voluntarilyAssignmentAttribute!.name);
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
            foreach (var fieldInfo in (obj as Type ?? obj.GetType()).GetFields(bindingFlags)) {
                if (!Util.isEffective(fieldInfo)) {
                    continue;
                }
                VoluntarilyAssignmentAttribute? voluntarilyAssignmentAttribute = fieldInfo.GetCustomAttribute<VoluntarilyAssignmentAttribute>();
                if (!(voluntarilyAssignmentAttribute?.use ?? true)) {
                    continue;
                }
                if (typeof(RegisterBasics).IsAssignableFrom(fieldInfo.FieldType) && voluntarilyAssignmentAttribute is not null) {
                    fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, getRegisterBasicsOfVoluntarilyAssignment(fieldInfo, voluntarilyAssignmentAttribute));
                }
                if (typeof(RegisterManage).IsAssignableFrom(fieldInfo.FieldType)) {
                    fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, getRegisterManageOfVoluntarilyAssignment(fieldInfo, voluntarilyAssignmentAttribute));
                }
            }
        }

        public ILogOut? getLog() => log;
    }
}