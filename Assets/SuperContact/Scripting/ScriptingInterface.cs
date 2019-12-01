using System;
using System.Threading.Tasks;

public abstract class ScriptingInterface {

    public delegate void LogHandler(string log);
    public event LogHandler OnLog;
    public event LogHandler OnLogError;
    public delegate void ExceptionHandler(Exception exception);
    public event ExceptionHandler OnException;

    public bool isAsync { get; set; } = false;

    public abstract Task SubmitCode(String name, String code);
    public abstract Task<object> Execute(String code, Boolean throwException);

    public void SendLog(String log) {
        OnLog?.Invoke(log);
    }

    public void SendLogError(String logError) {
        OnLogError?.Invoke(logError);
    }

    public void SendException(Exception e) {
        OnException?.Invoke(e);
    }
}
