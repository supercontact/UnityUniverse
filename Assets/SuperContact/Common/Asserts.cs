using System;
using UnityEngine;

public static class Asserts {
    
    public static void AssertThat(bool condition, string message = "") {
        if (!condition) throw new InvalidOperationException(message);
        return;
    }

    public static T AssertNotNull<T>(T obj, string message = "") {
        if (obj == null) throw new ArgumentNullException();
        return obj;
    }
}