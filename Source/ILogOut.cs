using System;

namespace RegisterSystem {
    public interface ILogOut {
        void Info(object message);
        void Warn(object message);
        void Error(object message);
    }

    public class ConsoleLog : ILogOut {
        public void Info(object message) {
            Console.WriteLine(message);
        }

        public void Warn(object message) {
            Console.Write(message);
        }

        public void Error(object message) {
            Console.WriteLine(message);
        }
    }
}