using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Logging : MonoBehaviour {

    public static Logging instance;

    [Serializable]
    public class LogUnityEvent : UnityEvent<string> { }
    public LogUnityEvent OnLogged;

    public bool logEnabled = true;
    
    private ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();

    public static void Log(object obj) {
        if (instance.logEnabled) {
            instance.logQueue.Enqueue(obj.ToString());
        }
    }

    public static bool GetLogEnabled() {
        return instance.logEnabled;
    }

    public static void SetLogEnabled(bool isEnabled) {
        instance.logEnabled = isEnabled;
        instance.enabled = isEnabled;
    }

    private void Awake() {
        instance = this;
        enabled = logEnabled;
    }

    private void OnDestroy() {
        instance = null;
    }

    private void Update() {
        while (logQueue.TryDequeue(out string log)) {
            if (OnLogged != null && OnLogged.GetPersistentEventCount() > 0) {
                OnLogged.Invoke(log);
            } else {
                Debug.Log(log);
            }
        }
    }
}
