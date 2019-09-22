using System;
using System.Collections;
using System.Collections.Generic;

public class BiDictionary<T1, T2> : IBiDictionary<T1, T2> {

    private Dictionary<T1, T2> _forwardDict = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> _reverseDict = new Dictionary<T2, T1>();

    public BiDictionary<T2, T1> Reverse { get; private set; }

    IBiDictionary<T2, T1> IBiDictionary<T1, T2>.Reverse => Reverse;

    public ICollection<T1> Keys => _forwardDict.Keys;

    public ICollection<T2> Values => _reverseDict.Keys;

    public int Count => _forwardDict.Count;

    public bool IsReadOnly => false;

    public BiDictionary() {
        Reverse = new BiDictionary<T2, T1>(_reverseDict, _forwardDict, this);
    }

    public BiDictionary(Dictionary<T1, T2> dictionary) {
        foreach (KeyValuePair<T1, T2> entry in dictionary) {
            _forwardDict.Add(entry.Key, entry.Value);
            _reverseDict.Add(entry.Value, entry.Key);
        }
        Reverse = new BiDictionary<T2, T1>(_reverseDict, _forwardDict, this);
    }

    private BiDictionary(Dictionary<T1, T2> forwardDict, Dictionary<T2, T1> reverseDict, BiDictionary<T2, T1> reverse) {
        _forwardDict = forwardDict;
        _reverseDict = reverseDict;
        Reverse = reverse;
    }

    public T2 this[T1 key] {
        get { return _forwardDict[key]; }
        set {
            if (key == null || value == null) {
                throw new ArgumentNullException("Both key and value should be non null!");
            }
            if (_forwardDict.TryGetValue(key, out T2 existingValue)) {
                if (existingValue.Equals(value)) return;
                if (_reverseDict.ContainsKey(value)) {
                    throw new ArgumentException("The value already exists in the map!");
                }
                _forwardDict[key] = value;
                _reverseDict.Remove(existingValue);
                _reverseDict.Add(value, key);
            } else {
                if (_reverseDict.ContainsKey(value)) {
                    throw new ArgumentException("The value already exists in the map!");
                }
                _forwardDict.Add(key, value);
                _reverseDict.Add(value, key);
            }
        }
    }

    public void Add(T1 key, T2 value) {
        if (key == null || value == null) {
            throw new ArgumentNullException("Both key and value should be non null!");
        }
        if (_forwardDict.ContainsKey(key)) {
            throw new ArgumentException("The key already exists in the map!");
        }
        if (_reverseDict.ContainsKey(value)) {
            throw new ArgumentException("The value already exists in the map!");
        }
        _forwardDict.Add(key, value);
        _reverseDict.Add(value, key);
    }

    public void Clear() {
        _forwardDict.Clear();
        _reverseDict.Clear();
    }

    public bool ContainsKey(T1 key) {
        return _forwardDict.ContainsKey(key);
    }

    public bool ContainsValue(T2 value) {
        return _reverseDict.ContainsKey(value);
    }

    public bool TryGetValue(T1 key, out T2 value) {
        return _forwardDict.TryGetValue(key, out value);
    }

    public bool Remove(T1 key) {
        if (_forwardDict.TryGetValue(key, out T2 value)) {
            _forwardDict.Remove(key);
            _reverseDict.Remove(value);
            return true;
        }
        return false;
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() {
        return _forwardDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    void ICollection<KeyValuePair<T1, T2>>.Add(KeyValuePair<T1, T2> item) {
        Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<T1, T2>>.Contains(KeyValuePair<T1, T2> item) {
        return _forwardDict.ContainsKey(item.Key) && _forwardDict[item.Key].Equals(item.Value);
    }

    bool ICollection<KeyValuePair<T1, T2>>.Remove(KeyValuePair<T1, T2> item) {
        if (_forwardDict.TryGetValue(item.Key, out T2 value)) {
            if (!value.Equals(item.Value)) {
                return false;
            }
            _forwardDict.Remove(item.Key);
            _reverseDict.Remove(item.Value);
            return true;
        }
        return false;
    }

    void ICollection<KeyValuePair<T1, T2>>.CopyTo(KeyValuePair<T1, T2>[] array, int arrayIndex) {
        ((IDictionary<T1, T2>)_forwardDict).CopyTo(array, arrayIndex);
    }
}