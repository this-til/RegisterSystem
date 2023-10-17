namespace RegisterSystem;

public interface ILogOut {
    void Debug(object message);
    void Debug(object message, Exception exception);
    void Info(object message);
    void Info(object message, Exception exception);
    void Warn(object message);
    void Warn(object message, Exception exception);
    void Error(object message);
    void Error(object message, Exception exception);
}public class ConsoleLog : ILogOut {
    public void Debug(object message) {
        Console.WriteLine(message);
    }

    public void Debug(object message, Exception exception) {
        Console.WriteLine(message);
        Console.WriteLine(exception);
    }

    public void Info(object message) {
        Console.WriteLine(message);
    }

    public void Info(object message, Exception exception) {
        Console.WriteLine(message);
        Console.WriteLine(exception);
    }

    public void Warn(object message) {
        Console.Write(message);
    }

    public void Warn(object message, Exception exception) {
        Console.WriteLine(message);
        Console.WriteLine(exception);
    }

    public void Error(object message) {
        Console.WriteLine(message);
    }

    public void Error(object message, Exception exception) {
        Console.WriteLine(message);
        Console.WriteLine(exception);
    }
}