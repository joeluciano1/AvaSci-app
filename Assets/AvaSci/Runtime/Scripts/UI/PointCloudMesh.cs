using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// A 3D mesh that represents a point cloud.
    /// </summary>
    public class PointCloudMesh : MonoBehaviour
    {
        private Mesh _mesh;
        private int[] _indices;
        
        /// <summary>
        /// Loads the specified point cloud data to a 3D mesh.
        /// </summary>
        /// <param name="cloud">The point cloud data.</param>
        /// <param name="min">The minimum distance to show data.</param>
        /// <param name="max">The maximum distance to show data.</param>
        public void Load(PointCloud cloud, float min = 0.0f, float max = 0.0f)
        {
            if (cloud == null) return;

            int size = cloud.Size;
            var vertices = cloud.Vertices;
            var colors = cloud.Colors;

            if (min < max)
            {
                for (int i = 0; i < size; i++)
                {
                    if (vertices[i].z < min || vertices[i].z > max)
                    {
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].y, -100.0f);
                    }
                }
            }

            if (_mesh == null || _indices == null || _indices.Length != size)
            {
                _indices = new int[size];

                for (int i = 0; i < size; i++)
                {
                    _indices[i] = i;
                }

                _mesh = new Mesh
                {
                    indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
                    vertices = vertices,
                    colors32 = colors
                };

                _mesh.SetIndices(_indices, MeshTopology.Points, 0);

                gameObject.GetComponent<MeshFilter>().mesh = _mesh;
            }

            _mesh.vertices = vertices;
            _mesh.colors32 = colors;
            _mesh.RecalculateBounds();
        }
    }
}