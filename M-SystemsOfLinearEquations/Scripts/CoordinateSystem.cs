using NaughtyAttributes;
using UnityEngine;

namespace SystemsOfLinearEquations
{
    public class CoordinateSystem : MonoBehaviour
    {
#if UNITY_EDITOR
        [OnValueChanged("UpdateRange")]
#endif
        [SerializeField]
        private float _range = 5f;
        public float Range
        {
            get { return _range; }
            set { _range = value; UpdateRange(); }
        }

        public float Size { get { return Range * 2f; } }

        public GameObject[] thirdDimensionElements;

        float scale = 1.0f;

        //NOTE: Don't modify these in runtime
        private Matrix4x4 localToWorldMatrix;
        private Matrix4x4 worldToLocalMatrix;

        [SerializeField] private Material[] gridMaterials;

        [HorizontalLine]
        [SerializeField] bool debug = false;
        public bool drawBounds = false;

        void OnEnable()
        {
            localToWorldMatrix = transform.localToWorldMatrix;
            worldToLocalMatrix = transform.worldToLocalMatrix;
            UpdateRange();
        }

        public Vector3 GetOrigin()
        {
            return transform.position;
        }

        //Thread safe
        public Vector3 SystemToWorldPosition(Vector3 systemPosition)
        {
            return localToWorldMatrix.MultiplyPoint3x4(systemPosition * scale);
        }

        //Thread safe
        public Vector3 WorldToSystemPosition(Vector3 worldPosition)
        {
            return worldToLocalMatrix.MultiplyPoint3x4(worldPosition) / scale;
        }

        //Thread safe
        public bool IsPointInsideSystem(Vector3 systemPosition)
        {
            return systemPosition.x >= -_range && systemPosition.x <= _range &&
                systemPosition.y >= -_range && systemPosition.y <= _range &&
                systemPosition.z >= -_range && systemPosition.z <= _range;
        }

        //Thread safe
        public bool IsPointInsideSystemWithTolerance(Vector3 systemPosition, float tolerance = 1e-4f)
        {
            return systemPosition.x >= -_range - tolerance && systemPosition.x <= _range + tolerance &&
                systemPosition.y >= -_range - tolerance && systemPosition.y <= _range + tolerance &&
                systemPosition.z >= -_range - tolerance && systemPosition.z <= _range + tolerance;
        }

        private void UpdateGridMaterials()
        {
            foreach (var m in gridMaterials)
                m.mainTextureScale = Vector2.one * Size;
        }

        [Button]
        private void UpdateRange()
        {
            scale = 1 / Range;
            UpdateGridMaterials();
        }

        [Button]
        public void Show3drDimension() { Toggle3rdDimension(true); }

        [Button]
        public void Hide3drDimension() { Toggle3rdDimension(false); }

        public void Toggle3rdDimension(bool enabled)
        {
            foreach (var go in thirdDimensionElements)
                go.SetActive(enabled);
        }

        private void OnDrawGizmos()
        {
            if (drawBounds)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(GetOrigin(), Vector3.one * 2f * transform.localScale.x);
            }
        }
    }
}
