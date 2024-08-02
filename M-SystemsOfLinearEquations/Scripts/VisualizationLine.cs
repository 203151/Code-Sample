using NaughtyAttributes;
using UnityEngine;

namespace SystemsOfLinearEquations
{
    public class VisualizationLine : MonoBehaviour
    {
        private const float ONE_OVER_THIRTY_TWO = 1f / 32f;

        [HideInInspector] public Vector3 point1;
        [HideInInspector] public Vector3 point2;

        [ReadOnly] public SystemOfEquationsVisualisation.VisualizedEntity visualizedEntity = SystemOfEquationsVisualisation.VisualizedEntity.None;

        private LineRenderer line;
        private MaterialPropertyBlock propertyBlock = null;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        public void SetData(Vector3 point1, Vector3 point2, SystemOfEquationsVisualisation.VisualizedEntity visualizedEntity)
        {
            SetPoints(point1, point2);

            this.visualizedEntity = visualizedEntity;
            UpdateMaterial();
        }

        public void SetPoints(Vector3 point1, Vector3 point2)
        {
            this.point1 = point1;
            this.point2 = point2;
            line.SetPosition(0, point1);
            line.SetPosition(1, point2);
        }

        public void UpdateMaterial()
        {
            propertyBlock.SetVector("_BaseMap_ST", new Vector4(SystemOfEquationsVisualisation.Instance.lineTiling, ONE_OVER_THIRTY_TWO, 0, ONE_OVER_THIRTY_TWO * (float)visualizedEntity));
            line.SetPropertyBlock(propertyBlock);
        }
    }
}
