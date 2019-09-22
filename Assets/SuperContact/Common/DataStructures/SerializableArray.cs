using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableArray<T> {
    [SerializeField]
    public T[] data;
}

/*[Serializable]
public class Serializable2DArray<T> {
    [SerializeField]
    private SerializableArray<T>[] data;

    public void SetArray(T[][] array) {
        if (array == null) {
            data = null;
            return;
        }

        data = new SerializableArray<T>[array.Length];
        for (int i = 0; i < array.Length; i++) {
            data[i] = new SerializableArray<T>();
            data[i].SetArray(array[i]);
        }
    }

    public T[][] GetArray() {
        if (data == null) return null;

        T[][] result = new T[data.Length][];
        for (int i = 0; i < data.Length; i++) {
            result[i] = data[i].GetArray();
        }
        return result;
    }
}

[Serializable]
public class Serializable3DArray<T> {
    [SerializeField]
    private Serializable2DArray<T>[] data;

    public void SetArray(T[][][] array) {
        if (array == null) {
            data = null;
            return;
        }

        data = new Serializable2DArray<T>[array.Length];
        for (int i = 0; i < array.Length; i++) {
            data[i] = new Serializable2DArray<T>();
            data[i].SetArray(array[i]);
        }
    }

    public T[][][] GetArray() {
        if (data == null) return null;

        T[][][] result = new T[data.Length][][];
        for (int i = 0; i < data.Length; i++) {
            result[i] = data[i].GetArray();
        }
        return result;
    }
}*/