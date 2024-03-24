using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace RegisterSystem {
    public class RegisterBasics {
        protected string _name;
        [IgnoreRegister] protected RegisterManage _registerManage;
        protected int _priority;
        protected internal RegisterSystem _registerSystem;
        protected internal int _index;
        protected internal bool _initEnd;

        /// <summary>
        /// 注册项的完整的名称
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        public string completeName => $"{registerManage.name}@{name}";

        /// <summary>
        /// 注册项的名称
        /// 使用此名称进行注册key
        /// 由<see cref="RegisterSystem"/>进行赋值
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
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        [IgnoreRegister]
        public RegisterManage registerManage {
            get => _registerManage;
            set {
                if (isInit()) {
                    return;
                }
                _registerManage = value;
            }
        }

        /// <summary>
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        public RegisterSystem registerSystem => _registerSystem;

        /// <summary>
        /// 初始化时候的优先级
        /// </summary>
        public int priority {
            get => _priority;
            set {
                if (isInit()) {
                    return;
                }
                _priority = value;
            }
        }

        /// <summary>
        /// 在RegisterManage最顶层的索引
        /// </summary>
        public int index => _index;

        public RegisterBasics() {
            awakeInitAdditionalRegister();
        }

        public virtual void awakeInitAdditionalRegister() {
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
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
                RegisterBasics? registerBasics = fieldInfo.GetValue(this) as RegisterBasics;

                if (registerBasics is null) {
                    registerBasics = Activator.CreateInstance(fieldInfo.FieldType) as RegisterBasics ?? throw new Exception();
                    fieldInfo.SetValue(this, registerBasics);
                }
            }
        }

        /// <summary>
        /// 最早的初始化方法
        /// </summary>
        public virtual void awakeInit() {
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        public virtual void init() {
        }

        /// <summary>
        /// 初始化结束后统一调用
        /// </summary>
        public virtual void initEnd() {
        }

        /// <summary>
        /// 获取附加的注册项目
        /// </summary>
        public virtual IEnumerable<RegisterBasics> getAdditionalRegister() {
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
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
                RegisterBasics? registerBasics = fieldInfo.GetValue(this) as RegisterBasics;

                if (registerBasics is null) {
                    continue;
                }

                registerBasics.name = $"{name}${_name}";
                registerBasics.priority = fieldRegisterAttribute.priority;
                registerBasics.registerManage = fieldRegisterAttribute.registerManageType is not null
                    ? registerSystem.getRegisterManageOfRegisterType(fieldRegisterAttribute.registerManageType)
                    : registerSystem.getRegisterManageOfRegisterType(registerBasics.GetType());

                yield return registerBasics;
            }
        }

        public virtual IEnumerable<Tag> getAdditionalTag() {
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                if (!Util.isEffective(fieldInfo)) {
                    continue;
                }
                FieldRegisterAttribute? fieldRegisterAttribute = fieldInfo.GetCustomAttribute<FieldRegisterAttribute>();
                if (fieldRegisterAttribute is null) {
                    continue;
                }
                if (!typeof(Tag).IsAssignableFrom(fieldInfo.FieldType)) {
                    continue;
                }
                Tag? tag = fieldInfo.GetValue(this) as Tag;

                if (tag is null) {
                    continue;
                }

                tag.name = $"{name}${_name}";
                tag.registerManage = fieldRegisterAttribute.registerManageType is not null 
                    ? registerSystem.getRegisterManageOfRegisterType(fieldRegisterAttribute.registerManageType) 
                    : registerSystem.getRegisterManageOfRegisterType(tag.getRegisterBasicsType());

                yield return tag;
            }
        }

        protected bool isInit() {
            if (_initEnd) {
                Util.getLog(GetType()).Error("RegisterManage已经初始化了,拒绝一些操作");
            }
            return _initEnd;
        }

        public override string ToString() => completeName;
    }
}