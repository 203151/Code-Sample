using System;
using System.Text;
using UnityEngine;

namespace SystemsOfLinearEquations
{
    public class Matrix
    {
        public int rowCount { get; private set; }
        public int columnCount { get; private set; }

        public Fraction[] elements;

        public bool augmented = false;

        public Fraction this[int index]
        {
            get
            {
                if (index < elements.Length)
                    return elements[index];
                throw new IndexOutOfRangeException("Invalid Matrix index!");
            }
            set
            {
                if (index < elements.Length)
                    elements[index] = value;
                else
                    throw new IndexOutOfRangeException("Invalid Matrix index!");
            }
        }

        public Fraction this[int row, int column]
        {
            get { return this[row + column * rowCount]; }
            set { this[row + column * rowCount] = value; }
        }

        public Matrix(int rowCount, int columnCount, bool augmented = false)
        {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            elements = new Fraction[rowCount * columnCount];
            for (int i = 0; i < elements.Length; i++)
                elements[i] = new Fraction();

            this.augmented = augmented;
        }

        public Matrix(Matrix m)
        {
            this.rowCount = m.rowCount;
            this.columnCount = m.columnCount;
            elements = new Fraction[rowCount * columnCount];
            for (int i = 0; i < elements.Length; i++)
                elements[i] = new Fraction(m[i]);

            this.augmented = m.augmented;
        }

        public void DebugPrint(string description = "(no description)")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Matrix {rowCount}x{columnCount} - {description}");

            for (int r = 0; r < rowCount; r++)
            {
                sb.Append("\n");
                for (int c = 0; c < columnCount; c++)
                {
                    sb.Append(this[r, c].ToString().PadLeft(3));
                    sb.Append(" ");
                }
            }

            Debug.Log(sb.ToString());
        }

        public void SwapRows(int a, int b)
        {
            Fraction tmp;
            for (int i = 0; i < columnCount; i++)
            {
                tmp = this[a, i];
                this[a, i] = this[b, i];
                this[b, i] = tmp;
            }
        }

        public int RowLeadingZerosCount(int rowIdx)
        {
            int x = 0;
            for (int i = 0; i < columnCount; i++)
            {
                if (this[rowIdx, i] != 0)
                    break;
                x++;
            }

            return x;
        }

        public bool IsZeroRow(int rowIdx)
        {
            return RowLeadingZerosCount(rowIdx) == columnCount;
        }

        public int CountNonZeroRows()
        {
            int x = 0;
            for (int i = 0; i < rowCount; i++)
                if (!IsZeroRow(i))
                    x++;

            return x;
        }

        public int GetLastNonZeroRowIdx()
        {
            for (int i = rowCount - 1; i >= 0; i--)
                if (!IsZeroRow(i))
                    return i;
            return -1;
        }

        public int rank {
            get {
                return CountNonZeroRows();
            }
        }

        // Gauss-Jordan elimination
        public void TransformToReducedRowEchelonForm()
        {
            int col = 0;
            for (int r = 0; r < rowCount; r++)
            {
                if (IsZeroRow(r))
                {
                    int idx = GetLastNonZeroRowIdx();
                    if (idx > r)
                        SwapRows(r, idx);
                    else
                        return;
                }

                int bestRowIdx = GetBestRowIdx(r, col);

                while (bestRowIdx < 0 && col < columnCount)
                {
                    col++;
                    bestRowIdx = GetBestRowIdx(r, col);
                }

                if (bestRowIdx < 0)  // should not happen, just a sanity check
                    return;

                if (bestRowIdx != r)
                    SwapRows(r, bestRowIdx);

                if (this[r, col] != 1)
                {
                    for (int c = col + 1; c < columnCount; c++)
                        this[r, c] /= this[r, col];
                    this[r, col].Set(1);
                }

                for (int r2 = 0; r2 < rowCount; r2++)
                {
                    if (r2 == r)
                        continue;

                    var x = this[r2, col];
                    for (int c = col; c < columnCount; c++)
                        this[r2, c] -= this[r, c] * x;
                }

                col++;
            }
        }

        // helper function: choose best row for the next step of Gauss-Jordan elimination
        private int GetBestRowIdx(int fromRow, int c)
        {
            for (int i = fromRow; i < rowCount; i++)
                if (this[i, c] == 1)
                    return i;

            for (int i = fromRow; i < rowCount; i++)
                if (this[i, c] == -1)
                    return i;

            if (this[fromRow, c] != 0)
                return fromRow;

            for (int i = fromRow + 1; i < rowCount; i++)
                if (this[i, c] != 0)
                    return i;

            return -1;
        }
    }
}
