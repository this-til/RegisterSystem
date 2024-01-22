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
        protected List<RegisterBasics>? registerList;

        /// <summary>
        /// 对应的注册管理系统
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal RegisterSystem registerSystem;

        /// <summary>
        /// 类管理的完整的名称
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal string completeName;

        /// <summary>
        /// 类管理的名称
        /// 使用此名称进行注册key
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal string name;

        /// <summary>
        /// 作为基础的类管理类型
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        [VoluntarilyAssignment(use = false)] protected internal RegisterManage? basicsRegisterManage;

        /// <summary>
        /// 初始化结束了
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal bool isInitEnd;

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
            if (basicsRegisterManage is null) {
                register.index = getCount();
            }
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
        /// 通过索引获取注册项
        /// </summary>
        public virtual RegisterBasics? getAt_erase(int index) {
            if (basicsRegisterManage is not null) {
                registerSystem.getLog()?.Error($"{this}不支持通过索引获取注册项");
            }
            if (!isInitEnd) {
                return null;
            }
            registerList ??= new List<RegisterBasics>(registerMap.Values);
            if (index < 0 || index >= registerList.Count) {
                return null;
            }
            return registerList[index];
        }

        /// <summary>
        /// 输出所有的注册项
        /// </summary>
        public virtual IEnumerable<RegisterBasics> forAll_erase() {
            if (!isInitEnd) {
                return registerMap.Values;
            }
            registerList ??= new List<RegisterBasics>(registerMap.Values);
            return registerList;
        }

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
        public virtual IEnumerable<RegisterBasicsMetadata> getDefaultRegisterItem() {
            yield break;
        }

        /// <summary>
        /// 获取第二波默认的注册选项
        /// </summary>
        public virtual IEnumerable<RegisterBasicsMetadata> getSecondDefaultRegisterItem() {
            yield break;
        }

        public override string ToString() {
            return $"{nameof(completeName)}: {completeName}, {nameof(name)}: {name}, type:{getRegisterType()}";
        }
    }

    public abstract class RegisterManage<T> : RegisterManage where T : RegisterBasics {
        protected List<T>? registerGenericityList;

        public override Type getRegisterType() => typeof(T);

        public T? get(string key) {
            return get_erase(key) as T;
        }

        public virtual T? getAt(int index) {
            return getAt_erase(index) as T;
        }

        /// <summary>
        /// 遍历所有的注册项
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> forAll() {
            if (!isInitEnd) {
                return _forAll();

                IEnumerable<T> _forAll() {
                    foreach (var registerMapValue in registerMap.Values) {
                        yield return (T)registerMapValue;
                    }
                }
            }
            if (registerGenericityList is null) {
                registerGenericityList = new List<T>(registerMap.Count);
                foreach (var registerMapValue in registerMap.Values) {
                    registerGenericityList.Add((T)registerMapValue);
                }
            }
            return registerGenericityList;
        }
    }
}