using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Logging : MonoBehaviour {

    public static Logging instance;

    public enum LogLevel {
        Info,
        Warning,
        Error,
    }

    public class LogMessage {
        public LogLevel logLevel;
        public string message;

        public LogMessage(LogLevel logLevel, string message) {
            this.logLevel = logLevel;
            this.message = message;
        }
    }

    [Serializable]
    public class LogUnityEvent : UnityEvent<string> { }
    public LogUnityEvent OnLog;
    public LogUnityEvent OnLogWarning;
    public LogUnityEvent OnLogError;

    public bool logInfoEnabled = true;
    public bool logWarningEnabled = true;
    public bool logErrorEnabled = true;

    private ConcurrentQueue<LogMessage> logQueue = new ConcurrentQueue<LogMessage>();

    public static void Log(object obj) {
        if (instance.logInfoEnabled) {
            instance.logQueue.Enqueue(new LogMessage(LogLevel.Info, obj.ToString()));
        }
    }

    public static void LogWarning(object obj) {
        if (instance.logWarningEnabled) {
            instance.logQueue.Enqueue(new LogMessage(LogLevel.Warning, obj.ToString()));
        }
    }

    public static void LogError(object obj) {
        if (instance.logErrorEnabled) {
            instance.logQueue.Enqueue(new LogMessage(LogLevel.Error, obj.ToString()));
        }
    }

    private void Awake() {
        instance = this;
    }

    private void OnDestroy() {
        instance = null;
    }

    private void Update() {
        while (logQueue.TryDequeue(out LogMessage log)) {
            if (log.logLevel == LogLevel.Info) {
                if (OnLog != null && OnLog.GetPersistentEventCount() > 0) {
                    OnLog.Invoke(log.message);
                } else {
                    Debug.Log(log);
                }
            } else if (log.logLevel == LogLevel.Warning) {
                if (OnLogWarning != null && OnLogWarning.GetPersistentEventCount() > 0) {
                    OnLogWarning.Invoke(log.message);
                } else {
                    Debug.LogWarning(log);
                }
            } else if (log.logLevel == LogLevel.Error) {
                if (OnLogError != null && OnLogError.GetPersistentEventCount() > 0) {
                    OnLogError.Invoke(log.message);
                } else {
                    Debug.LogError(log);
                }
            }
        }
    }


}
