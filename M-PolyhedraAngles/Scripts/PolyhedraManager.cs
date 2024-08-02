using Math3dVR;
using NaughtyAttributes;
using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PolyhedraAngles
{
    public class PolyhedraManager : UnitySingleton<PolyhedraManager>
    {
        protected PolyhedraManager() { }  // prevent a non-singleton constructor

        [Header("Polyhedra")]
        [HideInInspector] public Polyhedron.PolyhedronType[] availablePolyhedraTypes;
        public PolyhedraGroup mainPolyhedraGroup;
        [ReadOnly] public ExamplePolyhedronObject[] exampleObjects;
        [ReadOnly] public bool showPolyhedraOnExampleObjects = false;

        [ReadOnly] public Polyhedron activePolyhedron = null;
        [ReadOnly] public Polyhedron secondaryPolyhedron = null;  // grabbed with 2nd hand

        [Header("Settings")]
        public float visibilityToggleTime = 2f;
        public float vertexModelRadius = 0.025f;
        [Range(0, 3)] public int vertexSubdivisions = 0;
        public float edgeModelRadius = 0.01f;
        [Range(3, 10)] public int edgeWalls = 5;
        public float helperLineWidth = 0.005f;
        public float interactionPointModelRadius = 0.025f;
        public Color angleColor = Color.red;
        public float angleEdgeModelRadius = 0.005f;
        public float flyBackDelay = 0.5f;
        public AnimationCurve flyBackAnimCurve = Tween.EaseInStrong;

        [Header("Prefabs")]
        public Polyhedron polyhedronPrefab;
        public InteractablePoint interactablePointPrefab;
        public Angle anglePrefab;

        [Header("Materials")]
        public Material vertexMaterial;
        public Material edgeMaterial;
        public Material faceMaterial;
        public Material helperLineDashedMaterial;

        [Header("Scene")]
        public Transform tablePrimary;
        public Transform tableSecondary;

        public enum DrawingAngleState
        {
            Disabled,
            Selecting1stPoint,
            Selecting2ndPoint,
            Selecting3rdPoint,
            AngleCompleted,
        }

        [Header("Angles")]
        [ReadOnly] public DrawingAngleState drawingAngleState = DrawingAngleState.Disabled;

        public bool SelectingAnglePoint { get { return drawingAngleState == DrawingAngleState.Selecting1stPoint || drawingAngleState == DrawingAngleState.Selecting2ndPoint || drawingAngleState == DrawingAngleState.Selecting3rdPoint; } }

        [ReadOnly] public bool interactablePointHovered = false;

        // caching
        private SimpleMesh _vertexSimpleMesh;
        private SimpleMesh _edgeSimpleMesh;

        public void Init()
        {
            // which polyhedra should be available in current module?
            if (GameManger.Instance.activeModule == GameManger.ModuleType.AnglesInPrisms)
                availablePolyhedraTypes = new[] {
                    Polyhedron.PolyhedronType.GraniastoslupPrawidlowyTrojkatny,
                    Polyhedron.PolyhedronType.GraniastoslupPrawidlowyCzworokatny,
                    Polyhedron.PolyhedronType.GraniastoslupPrawidlowySzesciokatny
                };
            else if (GameManger.Instance.activeModule == GameManger.ModuleType.AnglesInPyramids)
                availablePolyhedraTypes = new[] {
                    Polyhedron.PolyhedronType.OstroslupPrawidlowyTrojkatny,
                    Polyhedron.PolyhedronType.OstroslupPrawidlowyCzworokatny,
                    Polyhedron.PolyhedronType.OstroslupPrawidlowySzesciokatny
                };
            else   // all types from enum
                availablePolyhedraTypes = System.Enum.GetValues(typeof(Polyhedron.PolyhedronType)).Cast<Polyhedron.PolyhedronType>().Except(new[] { Polyhedron.PolyhedronType.None }).ToArray();

            // spawn main polyhedra
            Debug.Log("[PolyhedraManager] Spawning main polyhedra...");
            foreach (var polyhedronType in availablePolyhedraTypes)
                mainPolyhedraGroup.AddPolyhedron(polyhedronType);
            mainPolyhedraGroup.FinishAddingPolyhedra();

            // init example polyhedra objects
            exampleObjects = FindObjectsOfType<ExamplePolyhedronObject>();
            Vector3 betweenTablesRotation = (Quaternion.Inverse(tablePrimary.rotation) * tableSecondary.rotation).eulerAngles;
            foreach (var epo in exampleObjects)
            {
                if (!availablePolyhedraTypes.Contains(epo.polyhedron.type))
                {
                    epo.transform.position = tableSecondary.TransformPoint(tablePrimary.InverseTransformPoint(epo.transform.position));
                    epo.transform.Rotate(betweenTablesRotation, Space.World);

                    epo.GetComponent<GrabbableObject>().RegisterDefaultLocation();
                }

                epo.Init();
            }
        }

        private void Update()
        {
            AngleDrawingUpdate();

            if ((OVRInput.GetDown(OVRInput.RawButton.A) || OVRInput.GetDown(OVRInput.RawButton.X)) && !interactablePointHovered)
                ResetAngle();
        }

        public SimpleMesh GetVertexSimpleMesh()
        {
            if (_vertexSimpleMesh == null)
                _vertexSimpleMesh = new Icosahedron(vertexSubdivisions);
            return _vertexSimpleMesh;
        }

        public SimpleMesh GetEdgeSimpleMesh()
        {
            if (_edgeSimpleMesh == null)
            {
                var sm = new SimpleMesh();

                int n = edgeWalls;
                float angle = 360f / (float)n * Mathf.Deg2Rad;
                float radius = 1f;
                float startAngle = 0;

                sm.vertices = new List<Vector3>();
                sm.normals = new List<Vector3>();
                sm.triangles = new List<int>();

                // bottom vertices
                for (int i = 0; i < n; i++)
                {
                    Vector3 v = radius * (Vector3.up * Mathf.Cos(startAngle + i * angle) + Vector3.right * Mathf.Sin(startAngle + i * angle));
                    sm.vertices.Add(v);
                    sm.normals.Add(v.normalized);
                }

                for (int i = 0; i < n; i++)
                {
                    sm.vertices.Add(sm.vertices[i] + Vector3.forward);
                    sm.normals.Add(sm.normals[i]);
                }

                for (int i = 0; i < n; i++)
                {
                    sm.triangles.Add((i + 1) % n);
                    sm.triangles.Add(i);
                    sm.triangles.Add(i + n);

                    sm.triangles.Add((i + 1) % n);
                    sm.triangles.Add(i + n);
                    sm.triangles.Add((i + 1) % n + n);
                }

                sm.uvs = new List<Vector2>();
                foreach (var v in sm.vertices)
                    sm.uvs.Add(Vector2.zero);

                _edgeSimpleMesh = sm;
            }

            return _edgeSimpleMesh;
        }


        public void SetMainPolyhedronAndAngle(Polyhedron.PolyhedronType targetPolyhedronType = Polyhedron.PolyhedronType.None, Angle.AngleType targetAngleType = Angle.AngleType.None, bool randomAngleVariation = false, System.Action callback = null)
        {
            StartCoroutine(SetMainPolyhedronAndAngleCoroutine(targetPolyhedronType, targetAngleType, randomAngleVariation, callback));
        }

        private IEnumerator SetMainPolyhedronAndAngleCoroutine(Polyhedron.PolyhedronType targetPolyhedronType, Angle.AngleType targetAngleType, bool randomAngleVariation, System.Action callback)
        {
            Debug.Log($"[PolyhedraManager] Set main POLYHEDRON={targetPolyhedronType} (current: {mainPolyhedraGroup.ActivePolyhedronType}) and ANGLE={targetAngleType}");

            if (mainPolyhedraGroup.ActivePolyhedronType != Polyhedron.PolyhedronType.None)
            {
                if (mainPolyhedraGroup.ActivePolyhedronType == targetPolyhedronType)
                {
                    mainPolyhedraGroup.ActivePolyhedron.SetAngle(targetAngleType, randomAngleVariation);

                    if (!mainPolyhedraGroup.ActivePolyhedron.isVisible)
                    {
                        mainPolyhedraGroup.ActivePolyhedron.Show();
                        yield return new WaitForSeconds(visibilityToggleTime);
                    }
                    callback?.Invoke();
                    yield break;
                }

                mainPolyhedraGroup.ActivePolyhedron.Hide();
                yield return new WaitForSeconds(visibilityToggleTime);
                mainPolyhedraGroup.ActivePolyhedronType = targetPolyhedronType;
            }

            if (targetPolyhedronType != Polyhedron.PolyhedronType.None)
            {
                mainPolyhedraGroup.ActivePolyhedronType = targetPolyhedronType;
                mainPolyhedraGroup.ActivePolyhedron.SetAngle(targetAngleType);
                mainPolyhedraGroup.ActivePolyhedron.Show();

                yield return new WaitForSeconds(visibilityToggleTime);
            }

            callback?.Invoke();
        }



        [Button(null, EButtonEnableMode.Playmode)]
        public void EnableDrawingAngle()
        {
            if (activePolyhedron != null)
                activePolyhedron.activeAngle.ClearPoints();

            drawingAngleState = DrawingAngleState.Selecting1stPoint;
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void DisableDrawingAngle()
        {
            if (drawingAngleState == DrawingAngleState.Disabled)
                return;

            CancelDrawingAngle();
            drawingAngleState = DrawingAngleState.Disabled;
        }

        public void InteractablePointSelected(InteractablePoint interactablePoint)
        {
            if (activePolyhedron == null)
                return;

            if (drawingAngleState == DrawingAngleState.Selecting1stPoint)
            {
                activePolyhedron.activeAngle.AddPoint(interactablePoint.pointName, false);
                activePolyhedron.activeAngle.AddPoint(GetDrawingAngleAnchor(), true);
                drawingAngleState = DrawingAngleState.Selecting2ndPoint;
            }
            else if (drawingAngleState == DrawingAngleState.Selecting2ndPoint)
            {
                activePolyhedron.activeAngle.SetPoint(1, interactablePoint.pointName, false);
                activePolyhedron.activeAngle.AddPoint(GetDrawingAngleAnchor(), true);
                drawingAngleState = DrawingAngleState.Selecting3rdPoint;
            }
            else if (drawingAngleState == DrawingAngleState.Selecting3rdPoint)
            {
                activePolyhedron.activeAngle.SetPoint(2, interactablePoint.pointName, true);
                drawingAngleState = DrawingAngleState.AngleCompleted;
            }
        }

        private void AngleDrawingUpdate()
        {
            if (drawingAngleState == DrawingAngleState.Disabled)
                return;

            if (activePolyhedron != null)
            {
                if (drawingAngleState == DrawingAngleState.Selecting2ndPoint)
                {
                    activePolyhedron.activeAngle.SetPoint(1, GetDrawingAngleAnchor(), true);
                }
                else if (drawingAngleState == DrawingAngleState.Selecting3rdPoint)
                {
                    activePolyhedron.activeAngle.SetPoint(2, GetDrawingAngleAnchor(), true);
                }
            }
        }

        public void ResetAngle()
        {
            if (drawingAngleState == DrawingAngleState.Disabled)
                return;

            if (activePolyhedron == null)
                return;

            if (drawingAngleState == DrawingAngleState.Selecting2ndPoint || drawingAngleState == DrawingAngleState.Selecting3rdPoint || drawingAngleState == DrawingAngleState.AngleCompleted)
            {
                activePolyhedron?.CancelDrawingAngle();
            }

            activePolyhedron?.activeAngle.ClearPoints();
            drawingAngleState = DrawingAngleState.Selecting1stPoint;
        }

        public void CancelDrawingAngle()
        {
            if (drawingAngleState == DrawingAngleState.Selecting2ndPoint || drawingAngleState == DrawingAngleState.Selecting3rdPoint)
            {
                if (activePolyhedron != null)
                {
                    activePolyhedron.CancelDrawingAngle();
                    activePolyhedron.activeAngle.ClearPoints();
                }
                drawingAngleState = DrawingAngleState.Selecting1stPoint;
            }
        }

        public System.Func<Vector3> DrawingAngleAnchorPosFunc = () => { return Vector3.zero; };
        public Vector3 GetDrawingAngleAnchor()
        {
            if (activePolyhedron != null)
                return activePolyhedron.transform.InverseTransformPoint(DrawingAngleAnchorPosFunc.Invoke());
            return DrawingAngleAnchorPosFunc.Invoke();
        }
    }
}
