using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrid<T> {
    T this[IntVector3 coords] { get; set; }

    T Get(IntVector3 coords);
    void Set(IntVector3 coords, T obj);
    void Set(IntBox area, T obj);
    void Add(IntVector3 coords, T obj);
    void Add(IntBox area, T obj);
    void Remove(IntVector3 coords);
    void Remove(IntBox area);
    void Clear();
}
