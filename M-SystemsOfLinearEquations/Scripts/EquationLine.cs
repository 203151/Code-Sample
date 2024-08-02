using System.Collections.Generic;
using UnityEngine;

namespace SystemsOfLinearEquations
{
    public class EquationLine
    {
        // Ax + By = C
        public float a;
        public float b;
        public float c;

        [HideInInspector] public List<Vector3> edgePoints = new List<Vector3>();

        public float this[int i]
        {
            get
            {
                if (i == 0) return a;
                if (i == 1) return b;
                if (i == 2) return c;
                return 0;
            }

            set
            {
                if (i == 0) a = value;
                if (i == 1) b = value;
                if (i == 2) c = value;
            }
        }

        public EquationLine() { }

        public EquationLine(SystemOfEquations soe, int rowIdx)
        {
            SetCoefficientsFromSystemOfEquationsRow(soe, rowIdx);
        }

        public void SetCoefficientsFromSystemOfEquationsRow(SystemOfEquations soe, int rowIdx)
        {
            a = soe[rowIdx, 0];
            b = soe[rowIdx, 1];
            c = soe[rowIdx, 2];
        }

        float ComputeX(float y)
        {
            // x = (C - By) / A
            if (a == 0) return float.MinValue;
            return (c - b * y) / a;
        }

        float ComputeY(float x)
        {
            // y = (C - Ax) / B
            if (b == 0) return float.MinValue;
            return (c - a * x) / b;
        }

        public void ComputeEdgePoints()
        {
            edgePoints.Clear();
            float r = SystemsOfEquationsManager.Instance.coordinateSystem.Range;

            CheckEdgePoint(ComputeX(-r), -r);
            CheckEdgePoint(ComputeX(r), r);
            CheckEdgePoint(-r, ComputeY(-r));
            CheckEdgePoint(r, ComputeY(r));
        }

        private void CheckEdgePoint(float x, float y, float z = 0)
        {
            var v = new Vector3(x, y, z);

            if (SystemsOfEquationsManager.Instance.coordinateSystem.IsPointInsideSystemWithTolerance(v))
                if (!edgePoints.Contains(v))
                    edgePoints.Add(v);
        }
    }
}
