using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeSpace<T> : ISpace<T> {

    public readonly Vector3 spaceMin;
    public readonly Vector3 spaceMax;
    public readonly int maxLevel;
    public readonly int splitThreshold;
    public readonly int mergeThreshold;

    private OctreeNode root;
    private BiDictionary<T, OctreeElement> elementByKey;

    public OctreeSpace(Vector3 spaceMin, Vector3 spaceMax, int maxLevel, int splitThreshold, int mergeThreshold) {
        this.spaceMin = spaceMin;
        this.spaceMax = spaceMax;
        this.maxLevel = maxLevel;
        this.splitThreshold = splitThreshold;
        this.mergeThreshold = mergeThreshold;
        root = new OctreeNode();
        elementByKey = new BiDictionary<T, OctreeElement>();
    }

    public Vector3 GetPosition(T key) {
        return elementByKey[key].position;
    }

    public void Add(T key, Vector3 position) {
        OctreeElement element = new OctreeElement();
        elementByKey.Add(key, element);

        AddInternal(position, 0, IntVector3.zero, root, element);
    }

    private void AddInternal(Vector3 position, int level, IntVector3 coords, OctreeNode node, OctreeElement element) {
        if (node.NodeElementCount >= splitThreshold && level < maxLevel) {
            Vector3 center = GetCenter(level, coords);
            node.Split(center);
        }

        node.TotalElementCount++;
        if (!node.IsLeaf) {
            Vector3 center = GetCenter(level, coords);
            int childIndex = PositionToChildIndex(position, center);
            OctreeNode child = node.GetOrCreateChild(childIndex);
            AddInternal(position, level + 1, coords * 2 + ChildIndexToCoords(childIndex), child, element);
            return;
        }
        element.position = position;
        element.level = level;
        element.coords = coords;
        node.Put(element);
        //Debug.Log(element.ToString() + " is added at " + Time.time);
    }

    public void Remove(T key) {
        OctreeElement element = elementByKey[key];
        elementByKey.Remove(key);

        RemoveInternal(element, 0, IntVector3.zero, root);
    }

    private void RemoveInternal(OctreeElement element, int level, IntVector3 coords, OctreeNode node) {
        node.TotalElementCount--;
        if (element.level == level) {
            node.Remove(element.index);
            return;
        }

        IntVector3 coordsInChildLevel = element.coords / (1 << (element.level - level - 1));
        IntVector3 childCoords = coordsInChildLevel - coords * 2;
        int childIndex = ChildCoordsToIndex(childCoords);
        OctreeNode child = node.children[childIndex];
        RemoveInternal(element, level + 1, coordsInChildLevel, child);
        if (child.TotalElementCount == 0) {
            node.children[childIndex] = null;
        }
        if (node.TotalElementCount <= mergeThreshold) {
            node.Merge();
        }
    }

    public void Update(T key, Vector3 newPosition) {
        OctreeElement element = elementByKey[key];

        element.position = newPosition;
        if (WithinBox(newPosition, GetMin(element.level, element.coords), GetMax(element.level, element.coords))) {
            return;
        }
        RemoveInternal(element, 0, IntVector3.zero, root);
        AddInternal(newPosition, 0, IntVector3.zero, root, element);
    }

    public void Clear() {
        root = new OctreeNode();
        elementByKey.Clear();
    }

    public T FindClosest(Vector3 position, float maxDistance = float.PositiveInfinity, System.Func<T, bool> predicate = null) {
        FindClosestInternal(position, maxDistance, predicate, spaceMin, spaceMax, root, out float closestDistance, out OctreeElement closestElement);
        return closestElement == null ? default : elementByKey.Reverse[closestElement];
    }

    private void FindClosestInternal(Vector3 position, float maxDistance, System.Func<T, bool> predicate, Vector3 min, Vector3 max, OctreeNode node, out float closestDistance, out OctreeElement closestElement) {
        closestDistance = float.PositiveInfinity;
        closestElement = null;

        if (DistanceToBox(position, min, max) > maxDistance) {
            return;
        }
        if (node.IsLeaf) {
            foreach (OctreeElement element in node.elements) {
                float distance = Vector3.Distance(position, element.position);
                if (distance < closestDistance &&
                    (predicate == null || predicate(elementByKey.Reverse[element]))) {

                    closestDistance = distance;
                    closestElement = element;
                }
            }
        } else {
            Vector3 span = (max - min) / 2;
            for (int i = 0; i < 8; i++) {  // Optimize order?
                OctreeNode child = node.children[i];
                if (child != null) {
                    Vector3 childMin = min + ChildIndexToCoords(i) * span;
                    Vector3 childMax = childMin + span;
                    FindClosestInternal(position, closestDistance, predicate, childMin, childMax, child, out float childClosestDistance, out OctreeElement childClosestElement);
                    if (childClosestDistance < closestDistance) {
                        closestDistance = childClosestDistance;
                        closestElement = childClosestElement;
                    }
                }
            }
        }
    }

    public IEnumerable<T> FindInSphere(Vector3 center, float radius) {
        throw new System.Exception();
    }

    public IEnumerable<T> FindInBox(Vector3 min, Vector3 max) {
        throw new System.Exception();
    }

    private List<GameObject> visualObjectPool;
    public void ShowDebugVisualization(GameObject prefab) {
        if (visualObjectPool == null) {
            visualObjectPool = new List<GameObject>();
        }
        int currentIndex = 0;
        ShowDebugVisualizationInternal(prefab, spaceMin, spaceMax, root, ref currentIndex);
        if (visualObjectPool.Count > currentIndex) {
            for (int i = currentIndex; i < visualObjectPool.Count; i++) {
                GameObject.Destroy(visualObjectPool[i]);
            }
            visualObjectPool.RemoveRange(currentIndex, visualObjectPool.Count - currentIndex);
        }
    }

    private void ShowDebugVisualizationInternal(GameObject prefab, Vector3 min, Vector3 max, OctreeNode node, ref int visualObjectIndex) {
        GameObject obj;
        if (visualObjectPool.Count <= visualObjectIndex) {
            obj = GameObject.Instantiate(prefab);
            visualObjectPool.Add(obj);
        } else {
            obj = visualObjectPool[visualObjectIndex];
        }
        obj.transform.position = (min + max) / 2;
        obj.transform.localScale = max - min;
        visualObjectIndex++;

        if (!node.IsLeaf) {
            Vector3 span = (max - min) / 2;
            for (int i = 0; i < 8; i++) {
                OctreeNode child = node.children[i];
                if (child != null) {
                    Vector3 childMin = min + ChildIndexToCoords(i) * span;
                    Vector3 childMax = childMin + span;
                    ShowDebugVisualizationInternal(prefab, childMin, childMax, child, ref visualObjectIndex);
                }
            }
        }
    }

    public void ClearDebugVisualization() {
        foreach (GameObject obj in visualObjectPool) {
            GameObject.Destroy(obj);
        }
        visualObjectPool = null;
    }

    private Vector3 GetMin(int level, IntVector3 coords) {
        return spaceMin + coords * ((spaceMax - spaceMin) / (1 << level));
    }
    private Vector3 GetMax(int level, IntVector3 coords) {
        return spaceMin + (coords + IntVector3.one) * ((spaceMax - spaceMin) / (1 << level));
    }
    private Vector3 GetCenter(int level, IntVector3 coords) {
        return spaceMin + VectorMult(coords.ToVector3() + new Vector3(0.5f, 0.5f, 0.5f), (spaceMax - spaceMin) / (1 << level));
    }


    private class OctreeElement {

        public Vector3 position;

        public int level;
        public IntVector3 coords;
        public int index;

        public OctreeElement() { }
        public OctreeElement(Vector3 position, int level, IntVector3 coords, int index = -1) {
            this.position = position;
            this.level = level;
            this.coords = coords;
            this.index = index;
        }

        public override string ToString() {
            return $"[Position: {position}, Level: {level}, Coords: {coords}, Index: {index}]";
        }
    }


    private class OctreeNode {

        // Managed by SpaceOctree
        public int TotalElementCount = 0;
        public int NodeElementCount {
            get { return elements.Count; }
        }
        public bool IsLeaf {
            get { return children == null; }
        }

        public List<OctreeElement> elements = new List<OctreeElement>();
        public OctreeNode[] children;

        public void Put(OctreeElement element) {
            element.index = elements.Count;
            elements.Add(element);
        }

        public void Remove(int index) {
            elements[index].index = -1;
            if (index < elements.Count - 1) {
                elements[index] = elements[elements.Count - 1];
                elements[index].index = index;
            }
            elements.RemoveAt(elements.Count - 1);
        }

        public OctreeNode GetOrCreateChild(int childIndex) {
            if (children[childIndex] == null) {
                children[childIndex] = new OctreeNode();
            }
            return children[childIndex];
        }

        // Assume no children.
        public void Split(Vector3 center) {
            children = new OctreeNode[8];
            foreach (OctreeElement element in elements) {
                MoveToChild(element, PositionToChildIndex(element.position, center));
            }
            elements.Clear();
        }

        // Assume all children are leaf nodes.
        public void Merge() {
            for (int i = 0; i < 8; i++) {
                if (children[i] != null) {
                    foreach (OctreeElement element in children[i].elements) {
                        MoveFromChild(element);
                    }
                }
            }
            children = null;
        }

        private void MoveToChild(OctreeElement element, int childIndex) {
            if (children[childIndex] == null) {
                children[childIndex] = new OctreeNode();
            }

            element.level += 1;
            element.coords = element.coords * 2 + ChildIndexToCoords(childIndex);
            children[childIndex].Put(element);
            children[childIndex].TotalElementCount++;
        }

        private void MoveFromChild(OctreeElement element) {
            element.level -= 1;
            element.coords = element.coords / 2;
            Put(element);
        }
    }


    private static int PositionToChildIndex(Vector3 position, Vector3 center) {
        int index = 0;
        if (position.x >= center.x) index += 1;
        if (position.y >= center.y) index += 2;
        if (position.z >= center.z) index += 4;
        return index;
    }

    private static IntVector3 ChildIndexToCoords(int childIndex) {
        return new IntVector3(childIndex & 1, (childIndex & 2) >> 1, (childIndex & 4) >> 2);
    }

    private static int ChildCoordsToIndex(IntVector3 childCoords) {
        return childCoords.x + childCoords.y * 2 + childCoords.z * 4;
    }

    private static Vector3 VectorMult(Vector3 a, Vector3 b) {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    private static bool WithinBox(Vector3 position, Vector3 min, Vector3 max) {
        return position.x > min.x && position.x < max.x && position.y > min.y && position.y < max.y && position.z > min.z && position.z < max.z;
    }

    private static float DistanceToBox(Vector3 position, Vector3 min, Vector3 max) {
        float dx = Mathf.Max(min.x - position.x, 0, position.x - max.x);
        float dy = Mathf.Max(min.y - position.y, 0, position.y - max.y);
        float dz = Mathf.Max(min.z - position.z, 0, position.z - max.z);
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}

