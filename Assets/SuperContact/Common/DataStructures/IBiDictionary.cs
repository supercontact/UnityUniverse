using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBiDictionary<T1, T2> : IDictionary<T1, T2> {
    IBiDictionary<T2, T1> Reverse { get; }
}
