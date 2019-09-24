using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfaceComponentGeometry : RenderGeometry {
    public List<List<Halfedge>> boundaries = new List<List<Halfedge>>();
    public List<Vertex> corners {
        get { return boundaries.Select(boundary => boundary[0].vertex).ToList(); }
    }

    public SurfaceComponentGeometry() { }
    public SurfaceComponentGeometry(Mesh mesh) : base(mesh) { }
    public SurfaceComponentGeometry(RenderGeometry geometry) {
        CombineGeometry(geometry);
    }

    public SurfaceComponentGeometry DefineBoundaries(params Vertex[] corners) {
        boundaries.Clear();
        Halfedge currentEdge = corners[0].edges.Find(e => e.isBoundary);
        if (currentEdge == null) {
            throw new Exception("Corner is not on boundary!");
        }
        for (int i = 0; i < corners.Length; i++) {
            var currentBoundaries = new List<Halfedge>();
            Vertex end = corners[(i + 1) % corners.Length];
            do {
                if (currentBoundaries.Contains(currentEdge)) {
                    throw new Exception($"Cannot find corner {end} along the boundary!");
                }
                currentBoundaries.Add(currentEdge);
                currentEdge = currentEdge.prev;
            } while (currentEdge.vertex != end);
            boundaries.Add(currentBoundaries);
        }
        return this;
    }

    public SurfaceComponentGeometry SplitBoundaries() {
        boundaries = boundaries.SelectMany(boundary => boundary).Select(e => new List<Halfedge> { e }).ToList();
        return this;
    }

    public SurfaceComponentGeometry ShiftBoundaries(int shiftAmount) {
        boundaries = boundaries.Rotate(shiftAmount).ToList();
        return this;
    }

    public SurfaceComponentGeometry CombineBoundaries(params int[] boundarySizes) {
        var newBoundaries = new List<List<Halfedge>>();
        int current = 0;
        foreach (int size in boundarySizes) {
            var newBoundary = new List<Halfedge>();
            for (int i = 0; i < size; i++) {
                newBoundary.AddRange(boundaries[current++]);
            }
            newBoundaries.Add(newBoundary);
        }
        boundaries = newBoundaries;
        return this;
    }

    public SurfaceComponentGeometry AutoAdjust(Vertex[] corners) {
        var locals = new List<Vector3>();
        var targets = new List<Vector3>();
        for (int i = 0; i < corners.Length; i++) {
            var local = boundaries[i][0].vertex.p;
            var target = corners[i].p;
            if (!locals.Contains(local)) {
                locals.Add(local);
                targets.Add(target);
                // Do not use 4 points transforms since commonly the first 4 points are on the same plane.
                if (locals.Count >= 3) break;
            }
        }

        Matrix4x4 transform;
        if (locals.Count == 1) {
            transform = MatrixUtil.PointToPointTransform(locals[0], targets[0]);
        } else if (locals.Count == 2) {
            transform = MatrixUtil.PointToPointTransform(locals[0], locals[1], targets[0], targets[1]);
        } else {
            transform = MatrixUtil.PointToPointTransform(locals[0], locals[1], locals[2], targets[0], targets[1], targets[2]);
        }
        ApplyLinearTransform(transform);
        return this;
    }
}