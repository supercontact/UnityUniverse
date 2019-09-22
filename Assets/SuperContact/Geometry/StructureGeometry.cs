using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class StructureGeometry : RenderGeometry {
    private Dictionary<Face, SurfaceComponentGeometry> surfaceComponents = new Dictionary<Face, SurfaceComponentGeometry>();
    private Dictionary<Halfedge, List<Halfedge>> connections = new Dictionary<Halfedge, List<Halfedge>>();
    private Dictionary<Vertex, List<Vertex>> structureToBuiltGeometryVertexMapping = new Dictionary<Vertex, List<Vertex>>();

    public StructureGeometry() {}
    public StructureGeometry(Mesh mesh) : base(mesh) {}
    public StructureGeometry(RenderGeometry geometry) {
        CombineGeometry(geometry);
    }

    public Vertex CreateVertex() {
        return CreateVertex(Vector3.zero);
    }

    public void CreateFace(SurfaceComponentGeometry surfaceComponent, bool useAutoAdjust, params Vertex[] verts) {
        if (surfaceComponent.boundaries.Count != verts.Length) throw new Exception("Edge count doesn't match!");
        var t = FindBoundaryHalfedge(vertices[0], vertices[0]);
        Face newFace = CreateFace(verts);
        SetFaceComponent(newFace, surfaceComponent, useAutoAdjust);
    }

    public void CreateFace(SurfaceComponentGeometry surfaceComponent, bool useAutoAdjust, bool inverse, params Vertex[] verts) {
        CreateFace(surfaceComponent, useAutoAdjust, inverse ? verts.Reverse().ToArray() : verts);
    }

    public void SetFaceComponent(Face face, SurfaceComponentGeometry surfaceComponent, bool useAutoAdjust) {
        surfaceComponents.Add(face, surfaceComponent);
        int i = 0;
        foreach (Halfedge e in face.edges) {
            AddVertexMapping(e.prev.vertex, surfaceComponent.boundaries[i][0].vertex);
            connections.Add(e, surfaceComponent.boundaries[i++]);
        }
        if (useAutoAdjust) {
            surfaceComponent.AutoAdjust(face.vertices.ToArray());
        }
    }

    public override void ApplyLinearTransform(Matrix4x4 transform, bool enablePerspective = false) {
        base.ApplyLinearTransform(transform, enablePerspective);
        foreach (SurfaceComponentGeometry surfaceComponent in surfaceComponents.Values) {
            surfaceComponent.ApplyLinearTransform(transform, enablePerspective);
        }
    }

    public override void ApplyPositionTransform(PositionTransform transform) {
        base.ApplyPositionTransform(transform);
        foreach (SurfaceComponentGeometry surfaceComponent in surfaceComponents.Values) {
            surfaceComponent.ApplyPositionTransform(transform);
        }
    }

    public override void ApplySpaceWarp(SpaceWarp warp) {
        base.ApplySpaceWarp(warp);
        foreach (SurfaceComponentGeometry surfaceComponent in surfaceComponents.Values) {
            surfaceComponent.ApplySpaceWarp(warp);
        }
    }

    // Should only build once!
    public RenderGeometry Build() {
        RenderGeometry g = new RenderGeometry();
        foreach (Face f in faces) {
            g.CombineGeometry(surfaceComponents[f]);
        }
        foreach (Halfedge e in halfedges) {
            if (connections.ContainsKey(e) && connections.ContainsKey(e.opposite)) {
                List<Halfedge> list1 = connections[e];
                List<Halfedge> list2 = connections[e.opposite];
                if (list1.Count != list2.Count) throw new Exception("Cannot connect two surface blocks, segment count doesn't match.");
                for (int i = 0; i < list1.Count; i++) {
                    g.ConnectEdges(list1[i], list2[list1.Count - 1 - i]);
                }
                connections.Remove(e);
                connections.Remove(e.opposite);
            }
        }
        return g;
    }

    // Should be used after build is called!
    public Vertex GetBuiltVertex(Vertex structureGeometryVertex) {
        return structureToBuiltGeometryVertexMapping[structureGeometryVertex].Find(v => v.index != -1);
    }

    private void AddVertexMapping(Vertex structureGeometryVertex, Vertex builtGeometryVertex) {
        if (!structureToBuiltGeometryVertexMapping.ContainsKey(structureGeometryVertex)) {
            structureToBuiltGeometryVertexMapping[structureGeometryVertex] = new List<Vertex> { builtGeometryVertex };
        } else {
            structureToBuiltGeometryVertexMapping[structureGeometryVertex].Add(builtGeometryVertex);
        }
    }
}