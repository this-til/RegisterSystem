using System;
using System.Collections.Generic;
using System.Reflection;

namespace RegisterSystem {
    /// <summary>
    /// 类型管理
    /// 这是一个泛型擦除的
    /// </summary>
    public abstract class RegisterManage {
        protected Dictionary<string, RegisterBasics> registerMap = new Dictionary<string, RegisterBasics>();

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

        /// <summary>
        /// 最早的初始化方法
        /// 在初始化时并没有填充注册字段
        /// </summary>
        public virtual void awakeInit() {
        }

        /// <summary>
        /// 初始化
        /// 在填充注册项后调用
        /// </summary>
        public virtual void init() {
        }

        /// <summary>
        /// 给SecondDefaultRegister提供的注册项配置默认数据
        /// </summary>
        public virtual void initSecond() {
        }

        /// <summary>
        /// 注册结束时调用
        /// </summary>
        public virtual void initEnd() {
        }

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
        public virtual void put(RegisterBasics register, bool fromSon) {
            basicsRegisterManage?.put(register, true);
            registerMap.Add(register.getName(), register);
        }

        /// <summary>
        /// 通过名称获取注册项
        /// </summary>
        public virtual RegisterBasics? get_erase(string key) {
            if (registerMap.ContainsKey(key)) {
                return registerMap[key];
            }
            return null;
        }

        /// <summary>
        /// 输出所有的注册项
        /// </summary>
        public virtual IEnumerable<KeyValuePair<string, RegisterBasics>> forAll_erase() => registerMap;

        /// <summary>
        /// 获取注册的数量
        /// </summary>
        /// <returns></returns>
        public int getCount() => registerMap.Count;

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
        public virtual IEnumerable<KeyValuePair<RegisterBasics, string>> getDefaultRegisterItem() {
            yield break;
        }

        /// <summary>
        /// 获取第二波默认的注册选项
        /// </summary>
        public virtual IEnumerable<KeyValuePair<RegisterBasics, string>> getSecondDefaultRegisterItem() {
            yield break;
        }

        public override string ToString() {
            return $"{nameof(completeName)}: {completeName}, {nameof(name)}: {name}, type:{getRegisterType()}";
        }
    }

    public abstract class RegisterManage<T> : RegisterManage where T : RegisterBasics {
        public override Type getRegisterType() => typeof(T);

        public T? get(string key) {
            return get_erase(key) as T;
        }

        public virtual IEnumerable<KeyValuePair<string, T>> forAll() {
            foreach (var keyValuePair in forAll_erase()) {
                yield return new KeyValuePair<string, T>(keyValuePair.Key, keyValuePair.Value as T ?? throw new Exception());
            }
        }
    }
}