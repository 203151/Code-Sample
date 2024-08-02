using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SystemsOfLinearEquations
{
    public class EquationPlane : MonoBehaviour
    {
        // Ax + By + Cz = D
        [OnValueChanged("UpdateVisuals")] public float a;
        [OnValueChanged("UpdateVisuals")] public float b;
        [OnValueChanged("UpdateVisuals")] public float c;
        [OnValueChanged("UpdateVisuals")] public float d;

        [SerializeField] private bool drawBorderLine = false;

        public int renderOrder = 0;

        protected Mesh mesh;
        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        protected LineRenderer lineRenderer;


        [HideInInspector] public List<Vector3> edgePoints = new List<Vector3>();
        Vector3 avgPoint;

        public Vector3 planeNormal { get { return new Vector3(a, b, c).normalized; } }

        [HorizontalLine]
        [SerializeField] bool debug = false;
        public bool drawEndpoints = true;
        public bool drawPlaneSpheres = false;
        public bool drawPlaneNormal = false;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.enabled = drawBorderLine;

            meshRenderer.rendererPriority = renderOrder;

            mesh = new Mesh() { name = "EquationPlane mesh" };
        }

        public void SetCoefficients(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public void SetCoefficientsFromSystemOfEquationsRow(SystemOfEquations soe, int rowIdx)
        {
            a = soe[rowIdx, 0];
            b = soe[rowIdx, 1];
            c = soe[rowIdx, 2];
            d = soe[rowIdx, 3];
        }

        float ComputeX(float y, float z)
        {
            // x = (D - By - Cz) / A
            if (a == 0) return float.MinValue;
            return (d - b * y - c * z) / a;
        }

        float ComputeY(float x, float z)
        {
            // y = (D - Ax - Cz) / B
            if (b == 0) return float.MinValue;
            return (d - a * x - c * z) / b;
        }

        float ComputeZ(float x, float y)
        {
            // z = (D - Ax - By) / C
            if (c == 0) return float.MinValue;
            return (d - a * x - b * y) / c;
        }

        public void UpdateVisuals()
        {
            UpdateEdgePoints();
            UpdatePlaneMesh();
            
            if (drawBorderLine)
                UpdateBorderLine();
        }

        void UpdateEdgePoints()
        {
            edgePoints.Clear();
            float r = SystemsOfEquationsManager.Instance.coordinateSystem.Range;

            CheckEdgePoint(ComputeX(-r, -r), -r, -r);
            CheckEdgePoint(ComputeX(-r, r), -r, r);
            CheckEdgePoint(ComputeX(r, -r), r, -r);
            CheckEdgePoint(ComputeX(r, r), r, r);

            CheckEdgePoint(-r, ComputeY(-r, -r), -r);
            CheckEdgePoint(-r, ComputeY(-r, r), r);
            CheckEdgePoint(r, ComputeY(r, -r), -r);
            CheckEdgePoint(r, ComputeY(r, r), r);

            CheckEdgePoint(-r, -r, ComputeZ(-r, -r));
            CheckEdgePoint(-r, r, ComputeZ(-r, r));
            CheckEdgePoint(r, -r, ComputeZ(r, -r));
            CheckEdgePoint(r, r, ComputeZ(r, r));

            avgPoint = Vector3.zero;

            if (edgePoints.Count > 0)
            {
                foreach (var p in edgePoints)
                    avgPoint += p;
                avgPoint /= edgePoints.Count;

                Vector3 firstPoint = edgePoints[0];
                edgePoints.Sort((a, b) =>
                {
                    return Vector3.SignedAngle(firstPoint, a, planeNormal).CompareTo(Vector3.SignedAngle(firstPoint, b, planeNormal));
                });
            }
        }

        private void CheckEdgePoint(float x, float y, float z = 0)
        {
            var v = new Vector3(x, y, z);
            if (SystemsOfEquationsManager.Instance.coordinateSystem.IsPointInsideSystemWithTolerance(v))
                edgePoints.Add(v);
        }

        void UpdatePlaneMesh()
        {
            mesh.Clear();

            if (edgePoints.Count >= 3)
            {
                int[] triangles = new int[3 * (edgePoints.Count - 2)];
                for (int i = 0; i < edgePoints.Count - 2; i++)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }


                List<Vector2> meshUVs = new List<Vector2>();

                for (int i = 0; i < edgePoints.Count; i++)
                {
                    float angle = Vector3.SignedAngle(edgePoints[0] - avgPoint, edgePoints[i] - avgPoint, planeNormal) * Mathf.Deg2Rad;
                    float dist = Vector3.Distance(avgPoint, edgePoints[i]);

                    float u = Mathf.Sin(angle) * dist;
                    float v = Mathf.Cos(angle) * dist;

                    meshUVs.Add(new Vector2(u, v));
                }

                mesh.SetVertices(edgePoints.Select(p => SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(p + planeNormal.normalized * 0.001f * renderOrder)).ToArray());
                mesh.SetNormals(edgePoints.Select(p => SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(planeNormal)).ToArray());
                mesh.SetUVs(0, meshUVs);

                mesh.subMeshCount = 1;
                mesh.SetTriangles(triangles, 0);
            }

            meshFilter.mesh = mesh;
        }

        void UpdateBorderLine()
        {
            if (edgePoints.Count < 2)
            {
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;

            lineRenderer.positionCount = edgePoints.Count;
            lineRenderer.SetPositions(edgePoints.Select(p => SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(p)).ToArray());
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawPlaneSpheres)
            {
                int half = (int)SystemsOfEquationsManager.Instance.coordinateSystem.Range;
                Gizmos.color = Color.white;
                for (int x = -half; x <= half; x++)
                    for (int z = -half; z <= half; z++)
                    {
                        var v = new Vector3(x, ComputeY(x, z), z);

                        if (SystemsOfEquationsManager.Instance.coordinateSystem.IsPointInsideSystemWithTolerance(v))
                            Gizmos.DrawSphere(SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(v), 0.01f);
                    }
            }

            if (drawPlaneNormal)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(avgPoint), 0.03f);
                Gizmos.DrawLine(SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(avgPoint), SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(avgPoint) + planeNormal * 2f);
            }

            if (drawEndpoints)
            {
                int idx = 0;
                foreach (var p in edgePoints)
                {
                    float angle = Vector3.SignedAngle(edgePoints[0] - avgPoint, p - avgPoint, planeNormal);

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(p), 0.03f);
                    UnityEditor.Handles.Label(SystemsOfEquationsManager.Instance.coordinateSystem.SystemToWorldPosition(p), $"#{idx}@{angle}");

                    idx++;
                }
            }
        }
#endif
    }
}
