using System;
using System.Collections.Generic;


namespace RegisterSystem {
    /// <summary>
    /// 类型管理
    /// 这是一个泛型擦除的
    /// </summary>
    public abstract class RegisterManage {
        protected string _name;
        protected internal RegisterSystem _registerSystem;
        protected internal bool _initEnd;

        /// <summary>
        /// 类管理的名称
        /// 使用此名称进行注册key
        /// </summary>
        public string name {
            get => _name;
            set {
                if (isInit()) {
                    return;
                }
                _name = value;
            }
        }

        /// <summary>
        /// 完整的名称
        /// </summary>
        public string completeName => basicsRegisterManage == null ? name : $"{basicsRegisterManage.completeName}/{name}";

        /// <summary>
        /// 对应的注册管理系统
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        public RegisterSystem registerSystem => _registerSystem;

        /// <summary>
        /// 初始化时候的优先级
        /// </summary>
        public virtual int priority => 0;

        /// <summary>
        /// 作为基础的类管理类型
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        [VoluntarilyAssignment(use = false)] protected internal RegisterManage? basicsRegisterManage;

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
        public abstract void put(RegisterBasics register, bool fromSon);

        /// <summary>
        /// 注册Tag
        /// </summary>
        public abstract void put(Tag tag);

        /// <summary>
        /// 通过名称获取注册项
        /// </summary>
        public abstract RegisterBasics? get_erase(string key);

        /// <summary>
        /// 通过名称获取tag
        /// </summary>
        public abstract Tag? getTag_erase(string key);

        /// <summary>
        /// 通过索引获取注册项
        /// </summary>
        public abstract RegisterBasics? getAt_erase(int index);

        /// <summary>
        /// 输出所有的注册项
        /// </summary>
        public abstract IEnumerable<RegisterBasics> forAll_erase();

        /// <summary>
        /// 循环所有的tag
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Tag> forAllTag_erase();

        /// <summary>
        /// 获取注册的数量
        /// </summary>
        /// <returns></returns>
        public abstract int getCount();

        public RegisterSystem getRegisterSystem() => registerSystem;

        public RegisterManage? getBasicsRegisterManage() => basicsRegisterManage;

        protected bool isInit() {
            if (_initEnd) {
                Util.getLog(GetType()).Error("RegisterManage已经初始化了,拒绝一些操作");
            }
            return _initEnd;
        }

        /// <summary>
        /// 获取默认的注册选项
        /// </summary>
        public virtual IEnumerable<RegisterBasics> getDefaultRegisterItem() {
            yield break;
        }

        /// <summary>
        /// 获取默认的Tag
        /// </summary>
        public virtual IEnumerable<Tag> getDefaultTag() {
            yield break;
        }

        /// <summary>
        /// 获取第二波默认的注册选项
        /// </summary>
        public virtual IEnumerable<RegisterBasics> getSecondDefaultRegisterItem() {
            yield break;
        }
    }

    public class RegisterManage<T> : RegisterManage where T : RegisterBasics {
        protected List<T> _registerList = new List<T>();
        protected Dictionary<string, T> _map = new Dictionary<string, T>();

        protected List<Tag> _tagList = new List<Tag>();
        protected Dictionary<string, Tag> _tagMap = new Dictionary<string, Tag>();

        protected Tag<T> _registerManageTag;

        public Tag<T> registerManageTag => _registerManageTag;

        public sealed override RegisterBasics get_erase(string key) => get(key);

        public virtual T? get(string key) {
            if (_map.ContainsKey(key)) {
                return _map[key];
            }
            return null;
        }

        public sealed override RegisterBasics getAt_erase(int index) => getAt(index);

        public Tag? getTag(string key) {
            if (_tagMap.ContainsKey(key)) {
                return _tagMap[key];
            }
            return null;
        }

        public override Tag getTag_erase(string key) => getTag(key);

        public virtual T? getAt(int index) {
            if (!_initEnd) {
                return null;
            }
            if (basicsRegisterManage is not null) {
                return (T)basicsRegisterManage.getAt_erase(index);
            }
            if (index >= 0 && index < _registerList.Count) {
                return _registerList[index];
            }
            return null;
        }

        public override void put(RegisterBasics register, bool fromSon) {
            if (basicsRegisterManage is null) {
                register._index = getCount();
            }
            basicsRegisterManage?.put(register, true);
            _map.Add(register.name, (T)register);
            _registerList.Add((T)register);
            _registerManageTag.addRegisterItem((T)register);
        }

        public override void put(Tag tag) {
            _tagMap.Add(tag.name, tag);
            _tagList.Add(tag);
        }

        public override int getCount() => _registerList.Count;

        public override Type getRegisterType() => typeof(T);

        /// <summary>
        /// 遍历所有的注册项
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<T> forAll() => _registerList;

        public override IEnumerable<RegisterBasics> forAll_erase() {
            foreach (var registerBasics in _registerList) {
                yield return registerBasics;
            }
        }

        public override IEnumerable<Tag> forAllTag_erase() => _tagList;

        public override IEnumerable<Tag> getDefaultTag() {
            foreach (var tag in base.getDefaultTag()) {
                yield return tag;
            }
            _registerManageTag = new Tag<T>();
            _registerManageTag.name = name;
            _registerManageTag.registerManage = this;
            yield return _registerManageTag;
        }
    }
}