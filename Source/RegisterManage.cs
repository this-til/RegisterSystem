using System.Reflection;

namespace RegisterSystem;

/// <summary>
/// 类型管理
/// 这是一个泛型擦除的
/// </summary>
public abstract class RegisterManage {
    /// <summary>
    /// 对应的注册管理系统
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected RegisterSystem registerSystem;

    /// <summary>
    /// 类管理的完整的名称
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected string completeName;

    /// <summary>
    /// 类管理的名称
    /// 使用此名称进行注册key
    /// 由<see cref="RegisterSystem"/>统进行反射赋值
    /// </summary>
    protected string name;

    /// <summary>
    /// 作为基础的类管理类型
    /// 由管理系统进行反射赋值
    /// </summary>
    protected RegisterManage? basicsRegisterManage;

    public string getCompleteName() => completeName;

    public string getName() => name;

    /// <summary>
    /// 获取当前管理的类型
    /// </summary>
    public abstract Type getRegisterType();

    /// <summary>
    /// 获取上一级的管理类的类型
    /// </summary>
    public virtual Type? getBasicsRegisterManageType() => null;

    /// <summary>
    /// 注册操作
    /// </summary>
    internal abstract void put(RegisterBasics register, bool fromSon);

    /// <summary>
    /// 通过名称获取注册项
    /// </summary>
    public abstract RegisterBasics? get_erase(string key);

    /// <summary>
    /// 输出所有的注册项
    /// </summary>
    public abstract IEnumerable<KeyValuePair<string, RegisterBasics>> forAll_erase();

    /// <summary>
    /// 获取初始化的优先级
    /// 高于RegisterBasics自己的优先级
    /// </summary>
    /// <returns></returns>
    public virtual int getPriority() => 0;

    public RegisterSystem getRegisterSystem() => registerSystem;

    public RegisterManage? getBasicsRegisterManage() => basicsRegisterManage;

    /// <summary>
    /// 获取默认的注册选项
    /// </summary>
    internal virtual IEnumerable<RegisterBasics> getDefaultRegisterItem() {
        yield break;
    }

    public override string ToString() {
        return $"{nameof(completeName)}: {completeName}, {nameof(name)}: {name}, type:{getRegisterType()}";
    }
}

public class RegisterManage<T> : RegisterManage where T : RegisterBasics {
    protected Dictionary<string, T> registerMap = new Dictionary<string, T>();

    public override Type getRegisterType() => typeof(T);

    internal override void put(RegisterBasics register, bool fromSon) {
        basicsRegisterManage?.put(register, true);
        registerMap.Add(register.getName(), (T)register);
    }

    public override RegisterBasics? get_erase(string key) {
        if (registerMap.ContainsKey(key)) {
            return registerMap[key];
        }
        return null;
    }

    public T? get(string key) {
        return get_erase(key) as T;
    }

    public virtual IEnumerable<KeyValuePair<string, T>> forAll() {
        foreach (var keyValuePair in registerMap) {
            yield return keyValuePair;
        }
    }

    public override IEnumerable<KeyValuePair<string, RegisterBasics>> forAll_erase() {
        foreach (var keyValuePair in registerMap) {
            yield return new KeyValuePair<string, RegisterBasics>(keyValuePair.Key, keyValuePair.Value);
        }
    }
}