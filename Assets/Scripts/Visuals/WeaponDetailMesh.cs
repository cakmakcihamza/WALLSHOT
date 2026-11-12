using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace WallBreaker.Visuals
{
    // Geometry only: batches small visual parts into one renderer per material/pivot.
    internal sealed class WeaponDetailMesh
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private readonly List<Vector2> uv = new List<Vector2>();

        private void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int i = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
            uv.Add(Vector2.zero); uv.Add(Vector2.right); uv.Add(Vector2.one); uv.Add(Vector2.up);
            triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
        }

        private void Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int i = vertices.Count;
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            uv.Add(Vector2.zero); uv.Add(Vector2.right); uv.Add(Vector2.one);
            triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
        }

        public void Box(Vector3 center, Vector3 size, Quaternion rotation = default)
        {
            if (rotation == default) rotation = Quaternion.identity;
            // Octagonal extrusion: clipped corners give long metal edges a beveled silhouette.
            float x = size.x * 0.5f, y = size.y * 0.5f, z = size.z * 0.5f;
            float b = Mathf.Min(size.x, size.y) * 0.18f;
            Vector2[] ring = { new Vector2(-x+b,-y), new Vector2(x-b,-y), new Vector2(x,-y+b),
                new Vector2(x,y-b), new Vector2(x-b,y), new Vector2(-x+b,y), new Vector2(-x,y-b), new Vector2(-x,-y+b) };
            for (int i = 0; i < ring.Length; i++)
            {
                Vector2 a = ring[i], n = ring[(i + 1) % ring.Length];
                Vector3 p = new Vector3(a.x,a.y,-z), q = new Vector3(n.x,n.y,-z);
                Vector3 r = new Vector3(n.x,n.y,z), s = new Vector3(a.x,a.y,z);
                Quad(center+rotation*p, center+rotation*q, center+rotation*r, center+rotation*s);
                Triangle(center+rotation*new Vector3(0,0,-z), center+rotation*q, center+rotation*p);
                Triangle(center+rotation*new Vector3(0,0,z), center+rotation*s, center+rotation*r);
            }
        }

        public void Ring(Vector3 center, float outer, float inner, float length, Quaternion rotation = default)
        {
            if (rotation == default) rotation = Quaternion.identity;
            const int segments = 12;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments, b = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 ao = new Vector3(Mathf.Cos(a)*outer, Mathf.Sin(a)*outer, 0f);
                Vector3 bo = new Vector3(Mathf.Cos(b)*outer, Mathf.Sin(b)*outer, 0f);
                Vector3 ai = ao * (inner / outer), bi = bo * (inner / outer);
                Vector3 back = Vector3.back * length * 0.5f, front = -back;
                Quad(center+rotation*(ao+back),center+rotation*(bo+back),center+rotation*(bo+front),center+rotation*(ao+front));
                Quad(center+rotation*(bi+back),center+rotation*(ai+back),center+rotation*(ai+front),center+rotation*(bi+front));
                Quad(center+rotation*(ai+back),center+rotation*(bi+back),center+rotation*(bo+back),center+rotation*(ao+back));
                Quad(center+rotation*(ao+front),center+rotation*(bo+front),center+rotation*(bi+front),center+rotation*(ai+front));
            }
        }

        public Mesh Build(Transform parent, string name, Material material)
        {
            Mesh mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            return mesh;
        }

        public static Texture2D Surface(bool wood)
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, true);
            texture.name = wood ? "Runtime_WalnutGrain" : "Runtime_GripCheckering";
            texture.wrapMode = TextureWrapMode.Repeat;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float value = wood ? 0.9f - 0.18f * Mathf.Pow(Mathf.Abs(Mathf.Sin(x * 0.7f + Mathf.Sin(y * 0.16f))), 10f)
                    : (((x / 4 + y / 4) % 2 == 0) ? 0.7f : 1f);
                pixels[y*size+x] = new Color(value, value, value);
            }
            texture.SetPixels(pixels); texture.Apply(true, true); return texture;
        }
    }
}
