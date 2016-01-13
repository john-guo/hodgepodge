using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTest")]

namespace CEbalance.Math
{
    public class Matrix<T>
    {
        private T[,] matrix;

        public Matrix(T[,] array)
        {
            matrix = array;
        }

        public Matrix(int row, int col)
        {
            matrix = new T[row, col];
        }

        public void SetData(T[] data)
        {
            if (data.Length != matrix.Length)
                throw new Exception();

            for (int i = 0; i < data.Length; ++i)
            {
                var row = i / Col;
                var col = i % Col;

                matrix[row, col] = data[i];
            }
        }

        public T this[int i, int j] 
        {
            get { return matrix[i, j]; }
            set { matrix[i, j] = value; }
        }

        internal int RowBegin
        {
            get { return matrix.GetLowerBound(0); }
        }

        internal int RowEnd
        {
            get { return matrix.GetUpperBound(0); }
        }

        public int Row
        {
            get
            {
                return matrix.GetUpperBound(0) + 1;
            }
        }

        public int Col
        {
            get
            {
                return matrix.GetUpperBound(1) + 1;
            }
        }

        private void mulRow(int row, T num)
        {
            dynamic n = num;
            for (int j = 0; j < Col; ++j)
            {
                dynamic v = matrix[row, j];

                matrix[row, j] = v * n;
            }
        }

        private void divRow(int row, T num)
        {
            dynamic n = num;
            for (int j = 0; j < Col; ++j)
            {
                dynamic v = matrix[row, j];

                matrix[row, j] = v / n;
            }
        }

        private void addRow(int row, int srcRow, T num)
        {
            dynamic n = num;
            for (int j = 0; j < Col; ++j)
            {
                dynamic v1 = matrix[row, j];
                dynamic v2 = matrix[srcRow, j];

                matrix[row, j] = v2 * n + v1;
            }
        }

        public void GaussElimination()
        {
            Sort(false);

            int r = System.Math.Min(Row, Col);

            for (int i = 0; i < r; ++i)
            {
                dynamic v = matrix[i, i];
                if (v == 0)
                    continue;
                eliminationRow(i, i);
            }

            for (int i = 0; i < r; ++i)
            {
                for (int j = 0; j < Col; ++j)
                {
                    dynamic v = matrix[i, j];
                    if (v == 0)
                        continue;
                    if (v == 1)
                        break;
                    divRow(i, matrix[i, j]);
                    break;
                }
            }

            Sort(false);
        }

        private void eliminationRow(int row, int col)
        {
            dynamic v1 = matrix[row, col];
            for (int j = row + 1; j < Row; ++j)
            {
                dynamic v2 = matrix[j, col];
                if (v2 == 0)
                    continue;

                dynamic n = v2 / v1;

                addRow(j, row, -n);
            }
        }

        internal void swapRow(int row1, int row2)
        {
            if (row1 == row2)
                return;

            T[] tempRow = new T[Col];
            for (int j = 0; j < Col; ++j)
            {
                tempRow[j] = matrix[row1, j];
            }
            for (int j = 0; j < Col; ++j)
            {
                matrix[row1, j] = matrix[row2, j];
            }
            for (int j = 0; j < Col; ++j)
            {
                matrix[row2, j] = tempRow[j];
            }
        }

        internal int compareRow(int row1, int row2)
        {
            for (int j = 0; j < Col; ++j)
            {
                dynamic v1 = matrix[row1, j];
                dynamic v2 = matrix[row2, j];

                if (v1 == v2)
                    continue;

                if (v1 == 0)
                    return -1;
                if (v2 == 0)
                    return 1;

                if (v1 < v2)
                    return -1;
                if (v1 > v2)
                    return 1;
            }

            return 0;
        }

        internal void Sort(bool isAsc = true)
        {
            if (Row < 10)
            {
                sortRow(isAsc);
                return;
            }

            var exchangeArray = Enumerable.Range(0, Row).ToArray();
            var indexedArray = (int[])exchangeArray.Clone();

            Array.Sort(indexedArray, (x, y) =>
                {
                    return isAsc ? compareRow(x, y) : compareRow(y, x);
                });

            for (int i = 0; i < exchangeArray.Length; ++i)
            {
                var newRow = indexedArray[i];
                var targetRow = Array.FindIndex(exchangeArray, p => p == newRow);

                exchangeArray[targetRow] = exchangeArray[i];
                exchangeArray[i] = newRow;

                swapRow(targetRow, i);
            }
        }

        private void sortRow(bool isAsc = true)
        {
            for (int i = 0; i < Row - 1; ++i)
            {
                for (int j = i + 1; j < Row; ++j)
                {
                    if (isAsc ? compareRow(i, j) > 0 : compareRow(i, j) < 0)
                        swapRow(i, j);
                }
            }
        }

        internal bool allZero(int row)
        {
            for (int i = 0; i < Col; ++i)
            {
                dynamic v = matrix[row, i];
                if (v != 0)
                    return false; 
            }

            return true;
        }

        [Conditional("DEBUG")]
        internal void Print()
        {
            for (int i = 0; i < Row; ++i)
            {
                for (int j = 0; j < Col; ++j)
                {
                    dynamic v = matrix[i, j];
                    Debug.Write(string.Format("{0} ", (v as object).ToString()));
                }

                Debug.WriteLine("");
            }
        }
    }
}
