using Math3dVR;
using NaughtyAttributes;
using UnityEngine;

namespace PolyhedraAngles
{
    [RequireComponent(typeof(GrabbableObject))]
    public class ExamplePolyhedronObject : MonoBehaviour
    {
        [HideInInspector] public Polyhedron polyhedron;
        [HideInInspector] public ExamplePolyhedronObjectModel exampleModel;

        public string localizationKeyObjectName;

        public float zoomFactor = 1f;
        Vector3 initialScale = Vector3.one;

        private void OnEnable()
        {
            initialScale = transform.localScale;

            polyhedron = GetComponentInChildren<Polyhedron>();
            exampleModel = GetComponentInChildren<ExamplePolyhedronObjectModel>();
        }

        public void Init()
        {
            polyhedron.generationScale = 1f / zoomFactor;
            polyhedron.Generate();

            GetComponent<GrabbableObject>()?.FinishAddingPolyhedra();
        }

        public string GetName()
        {
            return Localization.Instance.GetString(GetLocalizationKey());
        }

        public LocalizedString GetNameLocalizedString()
        {
            return new LocalizedString(GetLocalizationKey());
        }

        public string GetLocalizationKey()
        {
            return $"realLifeObject.{localizationKeyObjectName}";
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void Grab()
        {
            ZoomIn();
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void Release()
        {
            ZoomOut();
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void ZoomIn()
        {
            Pixelplacement.Tween.LocalScale(transform, initialScale * zoomFactor, PolyhedraManager.Instance.visibilityToggleTime, 0, Pixelplacement.Tween.EaseInOut);
        }

        [Button(null, EButtonEnableMode.Playmode)]
        public void ZoomOut()
        {
            Pixelplacement.Tween.LocalScale(transform, initialScale, PolyhedraManager.Instance.visibilityToggleTime, 0, Pixelplacement.Tween.EaseInOut);
        }

        public void ShowPolyhedron(System.Action callback = null)
        {
            exampleModel.BecomeTransparent();
            polyhedron.Show(callback);
        }

        public void HidePolyhedron(System.Action callback = null)
        {
            exampleModel.ResetToNormal();
            polyhedron.Hide(callback);
        }

        #region Measurements
#if UNITY_EDITOR
        [Button(null, EButtonEnableMode.Editor)]
        void MeasureHeight()
        {
            if (GetComponentInChildren<ExamplePolyhedronObjectModel>().TryGetComponent(out MeshRenderer meshRenderer))
            {
                GetComponentInChildren<Polyhedron>().height = meshRenderer.bounds.size.y;
            }
        }

        [Button(null, EButtonEnableMode.Editor)]
        void MeasureHeightToObject()
        {
            const string MEASUREMENT_OBJECT_NAME = "###-measurement-###";
            var measurementAnchor = GameObject.Find(MEASUREMENT_OBJECT_NAME);
            if (measurementAnchor == null)
            {

                if (UnityEditor.EditorUtility.DisplayDialog("Measure height to object", $"Measurement object (with name: '{MEASUREMENT_OBJECT_NAME}') not found. Create it?", "Yes", "No"))
                {
                    exampleModel = GetComponentInChildren<ExamplePolyhedronObjectModel>();
                    measurementAnchor = new GameObject(MEASUREMENT_OBJECT_NAME);
                    measurementAnchor.transform.SetPositionAndRotation(exampleModel.transform.position, exampleModel.transform.rotation);
                }

                return;
            }

            Vector3 p = measurementAnchor.transform.position;

            polyhedron = GetComponentInChildren<Polyhedron>();
            polyhedron.height = Vector3.Distance(polyhedron.transform.TransformPoint(Vector3.down * (polyhedron.height * 0.5f)), p);
        }

        [Button(null, EButtonEnableMode.Editor)]
        void MeasureRadius()
        {
            const string MEASUREMENT_OBJECT_NAME = "###-measurement-###";
            var measurementAnchor = GameObject.Find(MEASUREMENT_OBJECT_NAME);
            if (measurementAnchor == null)
            {

                if (UnityEditor.EditorUtility.DisplayDialog("Measure radius", $"Measurement object (with name: '{MEASUREMENT_OBJECT_NAME}') not found. Create it?", "Yes", "No"))
                {
                    exampleModel = GetComponentInChildren<ExamplePolyhedronObjectModel>();
                    measurementAnchor = new GameObject(MEASUREMENT_OBJECT_NAME);
                    measurementAnchor.transform.SetPositionAndRotation(exampleModel.transform.position, exampleModel.transform.rotation);
                }

                return;
            }

            Vector3 p = measurementAnchor.transform.position;

            polyhedron = GetComponentInChildren<Polyhedron>();
            polyhedron.useRadius = true;
            polyhedron.radius = Vector3.Distance(polyhedron.transform.TransformPoint(Vector3.down * (polyhedron.height * 0.5f)), p);
        }

        private void Reset()
        {
            MeasureHeight();
        }
#endif
        #endregion

    }
}
