using System;

namespace RegisterSystem {
    public class RegisterException : Exception {
        public readonly ExceptionOperation exceptionOperation;

        public RegisterException(string message, Exception innerException, ExceptionOperation exceptionOperation = ExceptionOperation.skip) : base(message, innerException) {
            this.exceptionOperation = exceptionOperation;
        }
    }

    public enum ExceptionOperation {
        collapse,
        abandon,
        skip
    }
}