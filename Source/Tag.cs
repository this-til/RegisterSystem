using System;
using System.Collections.Generic;
using log4net;

namespace RegisterSystem {
    public abstract class Tag {
        protected string _name;
        [IgnoreRegister] protected RegisterManage _registerManage;
        protected internal RegisterSystem _registerSystem;
        protected bool _isInitEnd;

        /// <summary>
        /// 注册项的完整的名称
        /// 由<see cref="RegisterSystem"/>进行赋值
        /// </summary>
        public string completeName => $"{registerManage.name}~{name}";

        public string name {
            get => _name;
            set {
                if (isInit()) {
                    return;
                }
                _name = value;
            }
        }

        public RegisterManage registerManage {
            get => _registerManage;
            set {
                if (isInit()) {
                    return;
                }
                _registerManage = value;
            }
        }

        public RegisterSystem registerSystem => _registerSystem;

        protected bool isInit() {
            if (_isInitEnd) {
                Util.getLog(GetType()).Error("RegisterManage已经初始化了,拒绝一些操作");
            }
            return _isInitEnd;
        }

        public abstract void addRegisterItem(RegisterBasics registerBasics);

        public abstract bool hasRegisterItem(RegisterBasics registerBasics);

        public abstract IEnumerable<RegisterBasics> forAllRegisterItem_erase();

        public abstract Type getRegisterBasicsType();
    }

    public class Tag<R> : Tag where R : RegisterBasics {
        protected HashSet<R> _has = new HashSet<R>();

        public override void addRegisterItem(RegisterBasics registerBasics) => _has.Add((R)registerBasics);

        public override bool hasRegisterItem(RegisterBasics registerBasics) => _has.Contains(registerBasics as R);

        public bool hasRegisterItem(R registerBasics) => _has.Contains(registerBasics);

        public override IEnumerable<RegisterBasics> forAllRegisterItem_erase() {
            foreach (var registerBasics in forAllRegisterItem()) {
                yield return registerBasics;
            }
        }

        public IEnumerable<R> forAllRegisterItem() => _has;

        public override Type getRegisterBasicsType() => typeof(R);
    }
}