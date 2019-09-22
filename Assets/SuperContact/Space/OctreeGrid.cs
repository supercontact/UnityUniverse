using UnityEngine;
using System.Collections.Generic;


public class OctreeGrid<T> : IGrid<T> {

    public readonly IntBox range;
    public readonly OctreeNode<T> root;
    public readonly int rootLevel;

    public OctreeGrid(IntBox range) {
        if (range.isEmpty) throw new System.Exception("Range is empty!");
        this.range = range;
        IntVector3 size = range.size;
        rootLevel = ((uint)Mathf.Max(size.x, size.y, size.z) - 1).NumberOfBit();
        root = new OctreeNode<T>();
    }

    public long objCount {
        get { return root.objCount; }
    }

    public T this[IntVector3 coords] {
        get { return Get(coords); }
        set { Set(coords, value); }
    }

    public T Get(IntVector3 coords) {
        return GetInternal(coords, root, rootLevel, range.min);
    }

    private T GetInternal(IntVector3 coords, OctreeNode<T> node, int level, IntVector3 corner) {
        if (node.isEmpty) {
            return default(T);
        } else if (node.isFilled) {
            return node.obj;
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndex = (coords - corner) / childSize;
        var childNode = node.GetChild(childIndex);
        if (childNode == null) return default(T);

        return GetInternal(coords, childNode, level - 1, corner + childIndex * childSize);
    }

    public void Set(IntVector3 coords, T obj) {
        SetInternal(coords, obj, root, rootLevel, range.min);
    }

    public void Set(IntBox area, T obj) {
        if (area.isEmpty) return;
        SetInternal(area, obj, root, rootLevel, range.min);
    }

    // Returns whether the node is changed.
    private bool SetInternal(IntVector3 coords, T obj, OctreeNode<T> node, int level, IntVector3 corner) {
        if (node.obj.Equals(obj)) {
            return false;
        } else if (level == 0) {
            node.obj = obj;
            return true;
        } else if (node.isFilled) {
            node.Split();
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndex = (coords - corner) / childSize;
        var childNode = node.GetChild(childIndex);
        if (childNode == null) {
            childNode = node.CreateChild(childIndex);
        }

        long oldChildObjCount = childNode.objCount;
        bool changed = SetInternal(coords, obj, childNode, level - 1, corner + childIndex * childSize);
        if (changed) {
            node.objCount += childNode.objCount - oldChildObjCount;
            if (childNode.isFilled) {
                node.MergeIfPossible(obj);
            }
        }
        return changed;
    }

    // Returns whether the node is changed.
    private bool SetInternal(IntBox area, T obj, OctreeNode<T> node, int level, IntVector3 corner) {
        IntBox nodeArea = new IntBox(corner, corner + ((1 << level) - 1) * IntVector3.one);
        if (node.obj.Equals(obj)) {
            return false;
        } else if (area.Contains(nodeArea)) {
            node.obj = obj;
            node.objCount = 1 << level;
            node.RemoveChildren();
            return true;
        } else if (node.isFilled) {
            node.Split();
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndexMin = IntVector3.Max((area.min - corner) / childSize, IntVector3.zero);
        IntVector3 childIndexMax = IntVector3.Min((area.max - corner) / childSize, IntVector3.one);
        bool changed = false;
        for (int x = childIndexMin.x; x <= childIndexMax.x; x++) {
            for (int y = childIndexMin.y; y <= childIndexMax.y; y++) {
                for (int z = childIndexMin.z; z <= childIndexMax.z; z++) {
                    IntVector3 childIndex = new IntVector3(x, y, z);
                    var childNode = node.GetChild(childIndex);
                    if (childNode == null) {
                        childNode = node.CreateChild(childIndex);
                    }

                    long oldChildObjCount = childNode.objCount;
                    bool childChanged = SetInternal(area, obj, childNode, level - 1, corner + childIndex * childSize);
                    if (childChanged) {
                        changed = true;
                        node.objCount += childNode.objCount - oldChildObjCount;
                        if (childNode.isFilled) {
                            node.MergeIfPossible(obj);
                        }
                    }
                }
            }
        }
        return changed;
    }

    public void Add(IntVector3 coords, T obj) {
        AddInternal(coords, obj, root, rootLevel, range.min);
    }

    public void Add(IntBox area, T obj) {
        if (area.isEmpty) return;
        AddInternal(area, obj, root, rootLevel, range.min);
    }

    // Returns whether the node is changed.
    private bool AddInternal(IntVector3 coords, T obj, OctreeNode<T> node, int level, IntVector3 corner) {
        if (node.isFilled) {
            return false;
        } else if (level == 0) {
            node.obj = obj;
            node.objCount = 1;
            return true;
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndex = (coords - corner) / childSize;
        var childNode = node.GetChild(childIndex);
        if (childNode == null) {
            childNode = node.CreateChild(childIndex);
        }

        long oldChildObjCount = childNode.objCount;
        bool changed = AddInternal(coords, obj, childNode, level - 1, corner + childIndex * childSize);
        if (changed) {
            node.objCount += childNode.objCount - oldChildObjCount;
            if (childNode.isFilled) {
                node.MergeIfPossible(obj);
            }
        }
        return changed;
    }

    // Returns whether the node is changed.
    private bool AddInternal(IntBox area, T obj, OctreeNode<T> node, int level, IntVector3 corner) {
        IntBox nodeArea = new IntBox(corner, corner + ((1 << level) - 1) * IntVector3.one);
        if (node.isFilled) {
            return false;
        } else if (node.isEmpty && area.Contains(nodeArea)) {
            node.obj = obj;
            node.objCount = 1 << level;
            return true;
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndexMin = IntVector3.Max((area.min - corner) / childSize, IntVector3.zero);
        IntVector3 childIndexMax = IntVector3.Min((area.max - corner) / childSize, IntVector3.one);
        bool changed = false;
        for (int x = childIndexMin.x; x <= childIndexMax.x; x++) {
            for (int y = childIndexMin.y; y <= childIndexMax.y; y++) {
                for (int z = childIndexMin.z; z <= childIndexMax.z; z++) {
                    IntVector3 childIndex = new IntVector3(x, y, z);
                    var childNode = node.GetChild(childIndex);
                    if (childNode == null) {
                        childNode = node.CreateChild(childIndex);
                    }

                    long oldChildObjCount = childNode.objCount;
                    bool childChanged = AddInternal(area, obj, childNode, level - 1, corner + childIndex * childSize);
                    if (childChanged) {
                        changed = true;
                        node.objCount += childNode.objCount - oldChildObjCount;
                        if (childNode.isFilled) {
                            node.MergeIfPossible(obj);
                        }
                    }
                }
            }
        }
        return changed;
    }

    public void Remove(IntVector3 coords) {
        RemoveInternal(coords, root, rootLevel, range.min);
    }

    public void Remove(IntBox area) {
        if (area.isEmpty) return;
        RemoveInternal(area, root, rootLevel, range.min);
    }

    // Returns whether the node is changed.
    private bool RemoveInternal(IntVector3 coords, OctreeNode<T> node, int level, IntVector3 corner) {
        if (level == 0) {
            if (!node.isEmpty) {
                node.obj = default(T);
                node.objCount = 0;
                return true;
            } else {
                return false;
            }
        } else if (!node.obj.Equals(default(T))) {
            node.Split();
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndex = (coords - corner) / childSize;
        var childNode = node.GetChild(childIndex);
        if (childNode == null) return false;

        long oldChildObjCount = childNode.objCount;
        bool changed = RemoveInternal(coords, childNode, level - 1, corner + childIndex * childSize);
        if (changed) {
            node.objCount += childNode.objCount - oldChildObjCount;
            if (childNode.isEmpty) {
                node.RemoveChild(childIndex);
            }
        }
        return changed;
    }

    // Returns whether the node is changed.
    private bool RemoveInternal(IntBox area, OctreeNode<T> node, int level, IntVector3 corner) {
        IntBox nodeArea = new IntBox(corner, corner + ((1 << level) - 1) * IntVector3.one);
        if (area.Contains(nodeArea)) {
            if (!node.isEmpty) {
                node.obj = default(T);
                node.objCount = 0;
                node.RemoveChildren();
                return true;
            } else {
                return false;
            }
        } else if (!node.obj.Equals(default(T))) {
            node.Split();
        }

        int childSize = 1 << (level - 1);
        IntVector3 childIndexMin = IntVector3.Max((area.min - corner) / childSize, IntVector3.zero);
        IntVector3 childIndexMax = IntVector3.Min((area.max - corner) / childSize, IntVector3.one);
        bool changed = false;
        for (int x = childIndexMin.x; x <= childIndexMax.x; x++) {
            for (int y = childIndexMin.y; y <= childIndexMax.y; y++) {
                for (int z = childIndexMin.z; z <= childIndexMax.z; z++) {
                    IntVector3 childIndex = new IntVector3(x, y, z);
                    var childNode = node.GetChild(childIndex);
                    if (childNode != null) {
                        long oldChildObjCount = childNode.objCount;
                        bool childChanged = RemoveInternal(area, childNode, level - 1, corner + childIndex * childSize);
                        if (childChanged) {
                            changed = true;
                            node.objCount += childNode.objCount - oldChildObjCount;
                            if (childNode.isEmpty) {
                                node.RemoveChild(childIndex);
                            }
                        }
                    }
                }
            }
        }
        return changed;
    }

    public void Clear() {
        root.obj = default(T);
        root.objCount = 0;
        root.RemoveChildren();
    }
}

public class OctreeNode<T> {

    public T obj;
    public long objCount = 0;

    private OctreeNode<T>[,,] children;

    public OctreeNode() {}
    public OctreeNode(T obj, long objCount) {
        this.obj = obj;
        this.objCount = objCount;
    }

    public bool isEmpty {
        get { return children == null && obj.Equals(default(T)); }
    }

    public bool isFilled {
        get { return !obj.Equals(default(T)); }
    }

    public bool isBranched {
        get { return children != null; }
    }

    public bool HasChild(IntVector3 childIndex) {
        if (children == null) return false;
        return children[childIndex.x, childIndex.y, childIndex.z] != null;
    }

    public OctreeNode<T> GetChild(IntVector3 childIndex) {
        if (children == null) return null;
        return children[childIndex.x, childIndex.y, childIndex.z];
    }

    public OctreeNode<T> CreateChild(IntVector3 childIndex) {
        if (children == null) {
            children = new OctreeNode<T>[2, 2, 2];
        }
        var child = new OctreeNode<T>();
        children[childIndex.x, childIndex.y, childIndex.z] = child;
        return child;
    }

    public void RemoveChild(IntVector3 childIndex) {
        if (children == null) return;
        children[childIndex.x, childIndex.y, childIndex.z] = null;
        foreach (var child in children) {
            if (child != null) return;
        }
        children = null;
    }

    public void RemoveChildren() {
        children = null;
    }

    public void Split() {
        children = new OctreeNode<T>[2, 2, 2];
        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 2; j++) {
                for (int k = 0; k < 2; k++) {
                    children[i, j, k] = new OctreeNode<T>(obj, objCount / 8);
                }
            }
        }
        obj = default(T);
    }

    public void MergeIfPossible(T mergeObj) {
        if (children == null) return;
        bool allFilled = true;
        foreach (var child in children) {
            if (!(child != null && child.obj.Equals(mergeObj))) {
                allFilled = false;
                break;
            }
        }
        if (allFilled) {
            obj = mergeObj;
            RemoveChildren();
        }
    }
}

public class OctreeNodeNavigator<T> {

    private List<OctreeNode<T>> path;
    private IntBox _range;
    private int _level;

    public OctreeNodeNavigator(List<OctreeNode<T>> path, IntBox range, int level) {
        this.path = path;
        _range = range;
        _level = level;
    }

    public IntBox range {
        get { return _range; }
    }

    public int level {
        get { return _level; }
    }

    public bool isEmpty {
        get { return node.isEmpty; }
    }

    public bool isFilled {
        get { return node.isFilled; }
    }

    public bool isBranched {
        get { return node.isBranched; }
    }

    public bool HasChild(IntVector3 childIndex) {
        return node.HasChild(childIndex);
    }

    public bool GotoChild(IntVector3 childIndex) {
        if (!node.isBranched) return false;
        var child = node.GetChild(childIndex);
        if (child == null) return false;

        path.Add(child);
        _level++;
        IntVector3 min = _range.min + childIndex * (1 << _level);
        _range = new IntBox(min, min + (1 << _level) * IntVector3.one);
        return true;
    }

    private OctreeNode<T> node {
        get { return path[path.Count - 1]; }
    }
}
