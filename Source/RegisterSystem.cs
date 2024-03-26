using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

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
        /// </summary>
        protected readonly Dictionary<string, RegisterManage> nameRegisterManageMap = new Dictionary<string, RegisterManage>();

        /// <summary>
        /// 标记有<see cref="VoluntarilyRegisterAttribute"/>属性的注册项类型的映射表
        /// </summary>
        protected readonly Dictionary<Type, RegisterBasics> allVoluntarilyRegisterAssetMap = new Dictionary<Type, RegisterBasics>();

        /// <summary>
        /// 根据<see cref="RegisterBasics.completeName"/>的映射表
        /// </summary>
        protected readonly Dictionary<string, RegisterBasics> completeNameRegisterBasicsMap = new Dictionary<string, RegisterBasics>();

        /// <summary>
        /// 所有tag的查询表
        /// </summary>
        protected readonly Dictionary<string, Tag> tagMap = new Dictionary<string, Tag>();

        protected readonly List<Type> allType = new List<Type>();

        /// <summary>
        /// 声明如果初始化了将禁用一些方法  
        /// </summary>
        protected bool init;

        protected ILog? log;

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

        protected event Action<Tag> tagPutEvent;
        protected event Action<Tag> tagInitEndEvent;

        public bool isInit() {
            if (init) {
                log?.Error("RegisterSystem已经初始化了，所以拒绝了一些操作");
            }
            return init;
        }

        public RegisterSystem() {
            log = LogManager.GetLogger(GetType());
            initAddRegisterManageAwakeInitEvent(r => r.awakeInit());
            initAddRegisterManageInitEvent(r => r.init());
            initAddRegisterManageInitSecondEvent(r => r.initSecond());
            initAddRegisterManagePutEndEvent(voluntarilyAssignment);
            initAddRegisterManagePutEndEvent(r => r._initEnd = true);
            initAddRegisterManagePutEndEvent(r => r.initEnd());
            initAddRegisterBasicsAwakeInitEvent(r => r.awakeInit());
            initAddRegisterBasicsInitEvent(r => r.init());
            initAddRegisterBasicsPutEvent(voluntarilyAssignment);
            initAddRegisterBasicsInitEndEvent(r => r._initEnd = true);
            initAddRegisterBasicsInitEndEvent(r => r.initEnd());
            initAddTagPutEvent(voluntarilyAssignment);
            initAddTagInitEndEvent(r => r._isInitEnd = true);
        }

        public void initAddRegisterManageAwakeInitEvent(Action<RegisterManage> action) {
            if (isInit()) {
                return;
            }
            registerManageAwakeInitEvent += action;
        }

        public void initAddRegisterManageInitEvent(Action<RegisterManage> action) {
            if (isInit()) {
                return;
            }
            registerManageInitEvent += action;
        }

        public void initAddRegisterManageInitSecondEvent(Action<RegisterManage> action) {
            if (isInit()) {
                return;
            }
            registerManageInitSecondEvent += action;
        }

        public void initAddRegisterManagePutEndEvent(Action<RegisterManage> action) {
            if (isInit()) {
                return;
            }
            registerManagePutEndEvent += action;
        }

        public void initAddRegisterBasicsAwakeInitEvent(Action<RegisterBasics> action) {
            if (isInit()) {
                return;
            }
            registerBasicsAwakeInitEvent += action;
        }

        public void initAddRegisterBasicsInitEvent(Action<RegisterBasics> action) {
            if (isInit()) {
                return;
            }
            registerBasicsInitEvent += action;
        }

        public void initAddRegisterBasicsPutEvent(Action<RegisterBasics> action) {
            if (isInit()) {
                return;
            }
            registerBasicsPutEvent += action;
        }

        public void initAddRegisterBasicsInitEndEvent(Action<RegisterBasics> action) {
            if (isInit()) {
                return;
            }
            registerBasicsInitEndEvent += action;
        }

        public void initAddTagPutEvent(Action<Tag> action) {
            if (isInit()) {
                return;
            }
            tagPutEvent += action;
        }

        public void initAddTagInitEndEvent(Action<Tag> action) {
            if (isInit()) {
                return;
            }
            tagInitEndEvent += action;
        }

        public void initAddAllManagedAssembly(params Assembly[]? assemblies) {
            if (isInit()) {
                return;
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
                return;
            }
            init = true;

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

            log?.Info("开始RegisterManage的实例化");

            //创建类型管理器
            foreach (var type in classRegisterManageMap.Keys.ToArray()) {
                try {
                    RegisterManage registerManage = (RegisterManage)Activator.CreateInstance(type);
                    registerManage._registerSystem = this;
                    registerManage.name = registerManage.GetType().Name;
                    classRegisterManageMap[type] = registerManage;

                    while (true) {
                        if (nameRegisterManageMap.ContainsKey(registerManage.name)) {
                            log?.Warn($"{registerManage}的名称冲突，已更新为:{registerManage.name}_");
                            registerManage.name += "_";
                            continue;
                        }
                        break;
                    }

                    nameRegisterManageMap.Add(registerManage.name, registerManage);

                    log?.Info($"完成{type}的实例化");
                }
                catch (Exception e) {
                    log?.Error($"{type}的实例化出现错误，我们将移除它", e);
                    classRegisterManageMap.Remove(type);
                }
            }

            log?.Info($"完成RegisterManage的实例化，统计：{string.Join(',', classRegisterManageMap.Values)}");

            log?.Info("开始RegisterManage的关系绑定");

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
                        throw new Exception("注册管理者父类型错误," +
                                            $"RegisterManage:{registerManage}" +
                                            $"BasicsRegisterManage:{basicsRegisterManage}");
                    }
                }

                registerManage._basicsRegisterManage = basicsRegisterManage;

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

            log?.Info("完成RegisterManage的关系绑定");

            log?.Info($"开始{nameof(registerManageAwakeInitEvent)}的事件回调");
            //回调
            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManageAwakeInitEvent(registerManage);
                }
                catch (Exception e) {
                    log?.Error($"RegisterManage:{registerManage}在registerManageAwakeInitEvent时发生错误", e);
                }
            }
            log?.Info($"完成{nameof(registerManageAwakeInitEvent)}的事件回调");

            List<RegisterBasics> registerBasicsList = new List<RegisterBasics>();
            List<Tag> tagList = new List<Tag>();

            log?.Info("开始从registerManage静态字段中获取注册项和Tag");
            List<RegisterBasics> nextAdd = new List<RegisterBasics>();
            List<Tag> nextAddTag = new List<Tag>();

            foreach (var keyValuePair in classRegisterManageMap) {
                foreach (var propertyInfo in keyValuePair.Key.GetProperties(BindingFlags.Static | BindingFlags.Public)) {
                    _(propertyInfo);
                }
                foreach (var fieldInfo in keyValuePair.Key.GetFields(BindingFlags.Static | BindingFlags.Public)) {
                    _(fieldInfo);
                }

                void _(MemberInfo memberInfo) {
                    if (!Util.isEffective(memberInfo)) {
                        return;
                    }

                    Type type;

                    switch (memberInfo) {
                        case PropertyInfo propertyInfo:
                            type = propertyInfo.PropertyType;
                            break;
                        case FieldInfo fieldInfo:
                            type = fieldInfo.FieldType;
                            break;
                        default:
                            throw new Exception();
                    }

                    if (typeof(RegisterBasics).IsAssignableFrom(type)) {
                        if (type.IsAbstract) {
                            log?.Error($"字段{type}的类型是抽象的，它将会被忽视");
                            return;
                        }

                        FieldRegisterAttribute? fieldRegisterAttribute = memberInfo.GetCustomAttribute<FieldRegisterAttribute>();

                        RegisterBasics registerBasics = Activator.CreateInstance(type) as RegisterBasics ?? throw new NullReferenceException();
                        registerBasics.name = memberInfo.Name;
                        registerBasics.priority = fieldRegisterAttribute?.priority ?? 0;
                        registerBasics.registerManage = keyValuePair.Value;

                        switch (memberInfo) {
                            case PropertyInfo propertyInfo:
                                propertyInfo.SetValue(null, registerBasics);
                                break;
                            case FieldInfo fieldInfo:
                                fieldInfo.SetValue(null, registerBasics);
                                break;
                            default:
                                throw new Exception();
                        }

                        nextAdd.Add(registerBasics);
                    }

                    if (typeof(Tag).IsAssignableFrom(type)) {
                        Tag tag = Activator.CreateInstance(type) as Tag ?? throw new NullReferenceException();

                        if (!keyValuePair.Value.getRegisterType().IsAssignableFrom(tag.getRegisterBasicsType())) {
                            log?.Error($"Tag字段{type}类型不是{keyValuePair.Value.getRegisterType()}的派生");
                            return;
                        }

                        RegisterManage registerManage = getRegisterManageOfRegisterType(tag.getRegisterBasicsType());
                        if (registerManage is null) {
                            log?.Error($"没有找到{tag.getRegisterBasicsType()}对应的RegisterManage");
                            return;
                        }

                        tag.name = memberInfo.Name;
                        tag.registerManage = registerManage;

                        switch (memberInfo) {
                            case PropertyInfo propertyInfo:

                                propertyInfo.SetValue(null, tag);
                                break;
                            case FieldInfo fieldInfo:
                                fieldInfo.SetValue(null, tag);
                                break;
                            default:
                                throw new Exception();
                        }

                        nextAddTag.Add(tag);
                    }
                }

                if (nextAdd.Count > 0) {
                    registerBasicsList.AddRange(nextAdd);
                    log?.Info($"从{keyValuePair.Key.Name}中获取到RegisterBasics：{string.Join(',', nextAdd)}");
                }
                if (nextAdd.Count > 0) {
                    tagList.AddRange(nextAddTag);
                    log?.Info($"从{keyValuePair.Key.Name}中获取到Tag：{string.Join(',', nextAdd)}");
                }

                nextAdd.Clear();
                nextAddTag.Clear();
            }

            log?.Info("完成从registerManage静态字段中获取注册项和Tag");
            log?.Info("开始从自动注册中获取注册项");
            //添加自动注册选项
            foreach (var type in allVoluntarilyRegisterAssetMap.Keys.ToArray()) {
                VoluntarilyRegisterAttribute voluntarilyRegisterAttribute = type.GetCustomAttribute<VoluntarilyRegisterAttribute>() ?? throw new NullReferenceException();
                RegisterBasics registerBasics = Activator.CreateInstance(type) as RegisterBasics ?? throw new NullReferenceException();
                registerBasics.name = string.IsNullOrEmpty(voluntarilyRegisterAttribute.customName) ? type.Name : voluntarilyRegisterAttribute.customName;
                registerBasics.priority = voluntarilyRegisterAttribute.priority;
                registerBasics.registerManage = voluntarilyRegisterAttribute.registerManageType is null ? getRegisterManageOfRegisterType(registerBasics.GetType()) : getRegisterManageOfManageType(voluntarilyRegisterAttribute.registerManageType);

                allVoluntarilyRegisterAssetMap[type] = registerBasics;
                registerBasicsList.Add(registerBasics);

                nextAdd.Add(registerBasics);
            }
            log?.Info($"完成从自动注册中获取注册项，总获取到：{string.Join(',', nextAdd)}");
            nextAdd.Clear();

            log?.Info("开始从registerManage中获取动态注册项");
            //从类型管理器中获取默认注册项

            foreach (var registerManage in classRegisterManageMap.Values) {
                nextAdd.AddRange(registerManage.getDefaultRegisterItem());
                nextAddTag.AddRange(registerManage.getDefaultTag());
                if (nextAdd.Count > 0) {
                    registerBasicsList.AddRange(nextAdd);
                    log?.Info($"从{registerManage.name}中获取到RegisterBasics：{string.Join(',', nextAdd)}");
                }
                if (nextAdd.Count > 0) {
                    tagList.AddRange(nextAddTag);
                    log?.Info($"从{registerManage.name}中获取到Tag：{string.Join(',', nextAdd)}");
                }
                nextAdd.Clear();
                nextAddTag.Clear();
            }
            log?.Info("完成从registerManage中获取动态注册项");

            log?.Info($"开始{nameof(registerManageInitEvent)}回调");
            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManageInitEvent(registerManage);
                }
                catch (Exception e) {
                    log?.Error($"{registerManage}进行{nameof(registerManageInitEvent)}时发生错误", e);
                }
            }
            log?.Info($"完成{nameof(registerManageInitEvent)}回调");

            unifyRegister(registerBasicsList);
            unifyTag(tagList);

            List<RegisterBasics> secondRegisterBasicList = new List<RegisterBasics>();

            log?.Info("开始从registerManage中获延时的动态注册项");
            foreach (var registerManage in classRegisterManageMap.Values) {
                nextAdd.AddRange(registerManage.getSecondDefaultRegisterItem());
                if (nextAdd.Count > 0) {
                    registerBasicsList.AddRange(nextAdd);
                    log?.Info($"从{registerManage.name}中获取到RegisterBasics：{string.Join(',', nextAdd)}");
                }
                nextAdd.Clear();
            }
            log?.Info("完成从registerManage中获延时的动态注册项");

            log?.Info($"开始{nameof(registerManageInitSecondEvent)}回调");
            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManageInitSecondEvent?.Invoke(registerManage);
                }
                catch (Exception e) {
                    log?.Error($"{registerManage}进行{nameof(registerManageInitSecondEvent)}时发生错误", e);
                }
            }

            log?.Info($"完成{nameof(registerManageInitSecondEvent)}回调");

            if (secondRegisterBasicList.Count > 0) {
                unifyRegister(secondRegisterBasicList);
            }

            log?.Info($"开始{nameof(registerManagePutEndEvent)}回调");
            foreach (var registerManage in classRegisterManageMap.Values) {
                try {
                    registerManagePutEndEvent?.Invoke(registerManage);
                }
                catch (Exception e) {
                    log?.Error($"{registerManage}进行{nameof(registerManagePutEndEvent)}时发生错误", e);
                }
            }
            log?.Info($"完成{nameof(registerManagePutEndEvent)}回调");

            log?.Info($"开始{nameof(registerBasicsInitEndEvent)}回调");
            foreach (var registerManage in classRegisterManageMap.Values) {
                foreach (var registerBasics in registerManage.forAll_erase()) {
                    try {
                        registerBasicsInitEndEvent?.Invoke(registerBasics);
                    }
                    catch (Exception e) {
                        log?.Error($"{registerManage}进行{nameof(registerBasicsInitEndEvent)}时发生错误", e);
                    }
                }
            }
            log?.Info($"完成{nameof(registerBasicsInitEndEvent)}回调");

            log?.Info($"开始{nameof(tagInitEndEvent)}回调");
            foreach (var tag in tagMap.Values) {
                try {
                    tagInitEndEvent?.Invoke(tag);
                }
                catch (Exception e) {
                    log?.Error($"{tag}进行{nameof(tagInitEndEvent)}时发生错误", e);
                }
            }
            log?.Info($"完成{nameof(tagInitEndEvent)}回调");
        }

        protected void unifyRegister(List<RegisterBasics> registerBasicsList) {
            registerBasicsList = new List<RegisterBasics>(registerBasicsList);
            List<RegisterBasics> needRegisterList = new List<RegisterBasics>();
            List<Tag> needRegisterTag = new List<Tag>();
            log?.Info("开始进行统一注册");
            for (var index = 0; index < registerBasicsList.Count; index++) {
                var registerBasics = registerBasicsList[index];
                registerBasics._registerSystem = this;
                if (registerBasics.registerManage is null) {
                    log?.Error($"注册{registerBasics}时没有找到对应的{typeof(RegisterManage)}");
                    registerBasicsList.RemoveAt(index);
                    index--;
                }
            }

            ReverseComparer<int> comparer = new ReverseComparer<int>();
            SortedDictionary<int, SortedDictionary<int, List<RegisterBasics>>> dictionary = new SortedDictionary<int, SortedDictionary<int, List<RegisterBasics>>>(comparer);
            foreach (var registerBasics in registerBasicsList) {
                SortedDictionary<int, List<RegisterBasics>> sortedDictionary;
                if (dictionary.ContainsKey(registerBasics.registerManage.priority)) {
                    sortedDictionary = dictionary[registerBasics.registerManage.priority];
                }
                else {
                    sortedDictionary = new SortedDictionary<int, List<RegisterBasics>>(comparer);
                    dictionary.Add(registerBasics.registerManage.priority, sortedDictionary);
                }
                List<RegisterBasics> list;
                if (sortedDictionary.ContainsKey(registerBasics.priority)) {
                    list = sortedDictionary[registerBasics.priority];
                }
                else {
                    list = new List<RegisterBasics>();
                    sortedDictionary.Add(registerBasics.priority, list);
                }
                list.Add(registerBasics);
            }
            registerBasicsList = new List<RegisterBasics>(registerBasicsList.Count);
            foreach (var keyValuePair in dictionary) {
                foreach (var valuePair in keyValuePair.Value) {
                    registerBasicsList.AddRange(valuePair.Value);
                }
            }

            log?.Info($"开始{nameof(registerBasicsAwakeInitEvent)}回调");
            foreach (var registerBasics in registerBasicsList) {
                try {
                    registerBasicsAwakeInitEvent?.Invoke(registerBasics);
                }
                catch (Exception e) {
                    log?.Error($"{registerBasics}进行{nameof(registerBasicsAwakeInitEvent)}回调时发生异常", e);
                }
            }
            log?.Info($"完成{nameof(registerBasicsAwakeInitEvent)}回调");

            log?.Info($"开始注册registerBasics");
            foreach (var registerBasics in registerBasicsList) {
                registerBasics.registerManage.put(registerBasics, false);
                completeNameRegisterBasicsMap.Add(registerBasics.completeName, registerBasics);
            }
            log?.Info($"完成注册registerBasics");

            log?.Info($"开始{nameof(registerBasicsPutEvent)}回调");
            foreach (var registerBasics in registerBasicsList) {
                try {
                    registerBasicsPutEvent?.Invoke(registerBasics);
                }
                catch (Exception e) {
                    log?.Error($"{registerBasics}进行{nameof(registerBasicsPutEvent)}回调时发生异常", e);
                }
            }
            log?.Info($"完成{nameof(registerBasicsPutEvent)}回调");

            log?.Info($"完成{nameof(registerBasicsInitEvent)}回调");
            foreach (var registerBasics in registerBasicsList) {
                try {
                    registerBasicsInitEvent?.Invoke(registerBasics);
                }
                catch (Exception e) {
                    log?.Error($"{registerBasics}进行{nameof(registerBasicsInitEvent)}回调时发生异常", e);
                }
            }

            List<RegisterBasics> cache = new List<RegisterBasics>();
            List<Tag> cacheTag = new List<Tag>();
            foreach (var registerBasics in registerBasicsList) {
                cache.AddRange(registerBasics.getAdditionalRegister());
                cacheTag.AddRange(registerBasics.getAdditionalTag());

                if (cache.Count > 0) {
                    needRegisterList.AddRange(cache);
                    log.Info($"从{registerBasics}中获取到注册项：{string.Join(',', cache)}");
                    cache.Clear();
                }
                if (cacheTag.Count > 0) {
                    needRegisterTag.AddRange(cacheTag);
                }
            }
            log?.Info("完成进行统一注册");

            if (needRegisterList.Count > 0) {
                unifyRegister(needRegisterList);
            }
            if (needRegisterTag.Count > 0) {
                unifyTag(needRegisterTag);
            }
        }

        protected void unifyTag(List<Tag> tags) {
            tags = new List<Tag>(tags);
            log?.Info("开始进行统一注册tag");
            for (var index = 0; index < tags.Count; index++) {
                var registerBasics = tags[index];
                registerBasics._registerSystem = this;
                if (registerBasics.registerManage is null) {
                    log?.Error($"注册{registerBasics}时没有找到对应的{typeof(RegisterManage)}");
                    tags.RemoveAt(index);
                    index--;
                }
            }
            foreach (var tag in tags) {
                tag.registerManage.put(tag);
                tagMap.Add(tag.completeName, tag);
            }

            log?.Info($"开始{nameof(tagPutEvent)}回调");

            foreach (var tag in tags) {
                try {
                    tagPutEvent?.Invoke(tag);
                }
                catch (Exception e) {
                    log?.Error($"{tag}进行{nameof(tagPutEvent)}时发生错误", e);
                }
            }

            log?.Info($"完成{nameof(tagPutEvent)}回调");

            log?.Info($"完成进行统一注册tag：{string.Join(',', tags)}");
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

        public RegisterManage? getRegisterManageOfVoluntarilyAssignment(MemberInfo memberInfo, VoluntarilyAssignmentAttribute? voluntarilyAssignmentAttribute) {
            VoluntarilyAssignmentType voluntarilyAssignmentType =
                voluntarilyAssignmentAttribute?.voluntarilyAssignmentType ?? VoluntarilyAssignmentType.voluntarilyRegisterAttribute;
            switch (voluntarilyAssignmentType) {
                case VoluntarilyAssignmentType.voluntarilyRegisterAttribute:
                case VoluntarilyAssignmentType.registerManage:
                    Type registerManageType = voluntarilyAssignmentAttribute?.appointType ?? (memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType;
                    RegisterManage? registerManage;
                    if (registerManageType!.IsGenericType) {
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

        public RegisterBasics? getRegisterBasicsOfVoluntarilyAssignment(MemberInfo memberInfo, VoluntarilyAssignmentAttribute voluntarilyAssignmentAttribute) {
            switch (voluntarilyAssignmentAttribute.voluntarilyAssignmentType) {
                case VoluntarilyAssignmentType.voluntarilyRegisterAttribute:
                    return getRegisterBasicsOfVoluntarilyRegister(voluntarilyAssignmentAttribute.appointType ?? (memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType);
                case VoluntarilyAssignmentType.registerManage:
                    RegisterManage? registerManage = getRegisterManageOfManageType(voluntarilyAssignmentAttribute.appointType ?? (memberInfo as FieldInfo)?.FieldType ?? (memberInfo as PropertyInfo)?.PropertyType);
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
                if (typeof(RegisterBasics).IsAssignableFrom(fieldInfo.FieldType) && voluntarilyAssignmentAttribute is not null) {
                    fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, getRegisterBasicsOfVoluntarilyAssignment(fieldInfo, voluntarilyAssignmentAttribute));
                }
                if (typeof(RegisterManage).IsAssignableFrom(fieldInfo.FieldType)) {
                    fieldInfo.SetValue(fieldInfo.IsStatic ? null : obj, getRegisterManageOfVoluntarilyAssignment(fieldInfo, voluntarilyAssignmentAttribute));
                }
            }

            foreach (var propertyInfo in (obj as Type ?? obj.GetType()).GetProperties(bindingFlags)) {
                if (!Util.isEffective(propertyInfo)) {
                    continue;
                }
                VoluntarilyAssignmentAttribute? voluntarilyAssignmentAttribute = propertyInfo.GetCustomAttribute<VoluntarilyAssignmentAttribute>();
                if (typeof(RegisterBasics).IsAssignableFrom(propertyInfo.PropertyType) && voluntarilyAssignmentAttribute is not null) {
                    propertyInfo.SetValue((propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic ? null : obj, getRegisterBasicsOfVoluntarilyAssignment(propertyInfo, voluntarilyAssignmentAttribute));
                }
                if (typeof(RegisterManage).IsAssignableFrom(propertyInfo.PropertyType)) {
                    propertyInfo.SetValue((propertyInfo.GetMethod ?? propertyInfo.SetMethod).IsStatic ? null : obj, getRegisterManageOfVoluntarilyAssignment(propertyInfo, voluntarilyAssignmentAttribute));
                }
            }
        }

        public ILog? getLog() => log;
    }
}