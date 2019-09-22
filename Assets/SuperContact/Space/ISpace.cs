using System.Collections.Generic;
using UnityEngine;

public interface ISpace<T> {
    Vector3 GetPosition(T key);
    void Add(T key, Vector3 position);
    void Remove(T key);
    void Update(T key, Vector3 newPosition);
    void Clear();

    T FindClosest(Vector3 position, float maxDistance = float.PositiveInfinity, System.Func<T, bool> predicate = null);
    IEnumerable<T> FindInSphere(Vector3 center, float radius);
    IEnumerable<T> FindInBox(Vector3 min, Vector3 max);
}
