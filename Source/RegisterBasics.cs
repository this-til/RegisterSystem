using System;
using System.Collections.Generic;
using System.Reflection;

namespace RegisterSystem {
    public class RegisterBasics {
        /// <summary>
        /// 注册项的完整的名称
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal string completeName;

        /// <summary>
        /// 注册项的名称
        /// 使用此名称进行注册key
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal string name;

        /// <summary>
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        [VoluntarilyAssignment(use = false)] protected internal RegisterManage registerManage;

        /// <summary>
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal RegisterSystem registerSystem;

        /// <summary>
        /// 初始化时候的优先级
        /// </summary>
        protected internal int priority;

        /// <summary>
        /// 初始化结束了
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        protected internal bool isInitEnd;

        /// <summary>
        /// 在RegisterManage最顶层的索引
        /// </summary>
        protected internal int index;

        public void awakeInitFieldRegister() {
            foreach (var keyValuePair in FieldRegisterCache.getCache(this.GetType())) {
                RegisterBasics? registerBasics = keyValuePair.Key.GetValue(this) as RegisterBasics;
                if (registerBasics is null && keyValuePair.Value.automaticCreate) {
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
        public virtual IEnumerable<RegisterBasicsMetadata> getAdditionalRegister() {
            int priorityOffset = 0;
            foreach (var keyValuePair in FieldRegisterCache.getCache(this.GetType())) {
                RegisterBasics? registerBasics = keyValuePair.Key.GetValue(this) as RegisterBasics;
                if (registerBasics is null) {
                    continue;
                }
                string _name = keyValuePair.Key.Name;
                if (!string.IsNullOrEmpty(keyValuePair.Value.customName)) {
                    _name = keyValuePair.Value.customName;
                }
                yield return new RegisterBasicsMetadata() {
                    registerBasics = registerBasics,
                    name = $"{name}${_name}",
                    registerManageType = keyValuePair.Value.registerManageType,
                    priority = keyValuePair.Value.priority + priorityOffset
                };
                priorityOffset++;
            }
        }

        public string getCompleteName() => completeName;

        public string getName() => name;

        public RegisterManage getRegisterManage() => registerManage;

        public RegisterSystem getRegisterSystem() => registerSystem;

        public int getPriority() => priority;

        protected void initTest() {
            if (isInitEnd) {
                throw new Exception("RegisterManage已经初始化了,拒绝一些操作");
            }
        }

        public int getIndex() => index;

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
}