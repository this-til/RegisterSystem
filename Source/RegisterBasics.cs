using System;
using System.Collections.Generic;
using System.Reflection;

namespace RegisterSystem;

public class RegisterBasics {
    /// <summary>
    /// 注册项的完整的名称
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected string completeName;

    /// <summary>
    /// 注册项的名称
    /// 使用此名称进行注册key
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected string name;

    /// <summary>
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected RegisterManage registerManage;

    /// <summary>
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected RegisterSystem registerSystem;

    /// <summary>
    /// 初始化时候的优先级
    /// </summary>
    protected int priority;

    /// <summary>
    /// 已经初始化了
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected bool _isInit;

    public void awakeInitFieldRegister() {
        foreach (var keyValuePair in FieldRegisterCache.getCache(this.GetType())) {
            RegisterBasics? registerBasics = keyValuePair.Key.GetValue(this) as RegisterBasics;
            if (registerBasics is null) {
                Type type = keyValuePair.Value.registerType ?? keyValuePair.Key.FieldType;
                registerBasics = Activator.CreateInstance(type) as RegisterBasics ?? throw new Exception();
                keyValuePair.Key.SetValue(this, registerBasics);
            }
        }
    }

    /// <summary>
    /// 最早的初始化方法
    /// </summary>
    public virtual void awakeInit() {
        foreach (var keyValuePair in FieldRegisterCache.getCache(this.GetType())) {
            RegisterBasics? registerBasics = keyValuePair.Key.GetValue(this) as RegisterBasics;
            if (registerBasics is null) {
                Type type = keyValuePair.Value.registerType ?? keyValuePair.Key.FieldType;
                registerBasics = Activator.CreateInstance(type) as RegisterBasics ?? throw new Exception();
                keyValuePair.Key.SetValue(this, registerBasics);
            }
        }
    }

    /// <summary>
    /// 初始化方法
    /// </summary>
    public virtual void init() {
    }

    /// <summary>
    /// 初始化结束后统一调用
    /// </summary>
    public virtual void initBack() {
    }

    /// <summary>
    /// 获取附加的注册项目
    /// </summary>
    public virtual IEnumerable<KeyValuePair<RegisterBasics, string>> getAdditionalRegister() {
        foreach (var keyValuePair in FieldRegisterCache.getCache(this.GetType())) {
            RegisterBasics registerBasics = keyValuePair.Key.GetValue(this) as RegisterBasics ?? throw new Exception();
            string _name = keyValuePair.Key.Name;
            if (!string.IsNullOrEmpty(keyValuePair.Value.customName)) {
                _name = keyValuePair.Value.customName;
            }
            yield return new KeyValuePair<RegisterBasics, string>(registerBasics, _name);
        }
    }

    public string getCompleteName() => completeName;

    public string getName() => name;

    public RegisterManage getRegisterManage() => registerManage;

    public RegisterSystem getRegisterSystem() => registerSystem;

    public int getPriority() => priority;

    public bool isInit() => _isInit;

    protected void initTest() {
        if (isInit()) {
            throw new Exception("RegisterManage已经初始化了,拒绝一些操作");
        }
    }

    public override string ToString() {
        return completeName;
    }

    protected class FieldRegisterCache {
        protected static Dictionary<Type, List<KeyValuePair<FieldInfo, FieldRegisterAttribute>>> cache
            = new Dictionary<Type, List<KeyValuePair<FieldInfo, FieldRegisterAttribute>>>();

        public static List<KeyValuePair<FieldInfo, FieldRegisterAttribute>> getCache(Type type) {
            if (cache.ContainsKey(type)) {
                return cache[type];
            }
            List<KeyValuePair<FieldInfo, FieldRegisterAttribute>> fieldInfos = new List<KeyValuePair<FieldInfo, FieldRegisterAttribute>>();
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (!Util.isEffective(fieldInfo)) {
                    continue;
                }
                FieldRegisterAttribute? fieldRegisterAttribute = fieldInfo.GetCustomAttribute<FieldRegisterAttribute>();
                if (fieldRegisterAttribute is null) {
                    continue;
                }
                if (!typeof(RegisterBasics).IsAssignableFrom(fieldInfo.FieldType)) {
                    continue;
                }
                fieldInfos.Add(new KeyValuePair<FieldInfo, FieldRegisterAttribute>(fieldInfo, fieldRegisterAttribute));
            }
            cache.Add(type, fieldInfos);
            return fieldInfos;
        }
    }
}