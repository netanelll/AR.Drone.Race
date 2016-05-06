using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCMAPI
	{
   public class Vector3
    {
        public static Vector3 Zero = NewZero();
        public static Vector3 One = NewOne();

        public double x;
        public double y;
        public double z;

        public Vector3()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(double xyz)
        {
            this.x = xyz;
            this.y = xyz;
            this.z = xyz;
        }

        public Vector3(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public static Vector3 NewZero()
        {
            return new Vector3(0.0f);
        }

        public static Vector3 NewOne()
        {
            return new Vector3(1.0f);
        }

        public double DotProduct(Vector3 other)
        {
            return x * other.x + y * other.y + z * other.z;
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
        }

        public static Vector3 operator -(Vector3 v)
        {
            return new Vector3(-v.x, -v.y, -v.z);
        }

        public static Vector3 operator *(Vector3 v, double scalar)
        {
            return new Vector3(v.x * scalar, v.y * scalar, v.z * scalar);
        }

        public static Vector3 operator /(Vector3 v, double scalar)
        {
            return new Vector3(v.x / scalar, v.y / scalar, v.z / scalar);
        }

        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return v1.x != v2.x || v1.y != v2.y || v1.z != v2.z;
        }

        public static Vector3 CrossProduct(Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public Vector3 Add(Vector3 v)
        {
            x += v.x;
            y += v.y;
            z += v.z;
            return this;
        }

        public double DistanceTo(Vector3 v)
        {
            double dx = this.x - v.x;
            double dy = this.y - v.y;
            double dz = this.z - v.z;
            return (double)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public double Size()
        {
            return DistanceTo(Vector3.Zero);
        }

        public Vector3 Normalize()
        {
            double size = Size();
            this.x /= size;
            this.y /= size;
            this.z /= size;
            return this;
        }

        public Vector3 Clone()
        {
            return new Vector3(this);
        }

        public override string ToString()
        {
            return "(" + x + ", " + y + ", " + z + ")";
        }
    }


   public class Matrix
    {
        public double[,] matrix;
        public int rows;
        public int cols;

        public Matrix(int rows, int cols)
        {
            this.matrix = new double[rows, cols];
            this.rows = rows;
            this.cols = cols;
        }

        public Matrix(double[,] matrix)
        {
            this.matrix = matrix;
            this.rows = matrix.GetLength(0);
            this.cols = matrix.GetLength(1);
        }

        protected static double[,] Multiply(Matrix matrix, double scalar)
        {
            int rows = matrix.rows;
            int cols = matrix.cols;
            double[,] m1 = matrix.matrix;
            double[,] m2 = new double[rows, cols];
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    m2[i, j] = m1[i, j] * scalar;
                }
            }
            return m2;
        }

        protected static double[,] Multiply(Matrix matrix1, Matrix matrix2)
        {
            int m1rows = matrix1.rows;
            int m1cols = matrix1.cols;
            int m2rows = matrix2.rows;
            int m2cols = matrix2.cols;
            if (m1cols != m2rows)
            {
                throw new ArgumentException();
            }
            double[,] m1 = matrix1.matrix;
            double[,] m2 = matrix2.matrix;
            double[,] m3 = new double[m1rows, m2cols];
            for (int i = 0; i < m1rows; ++i)
            {
                for (int j = 0; j < m2cols; ++j)
                {
                    double sum = 0;
                    for (int it = 0; it < m1cols; ++it)
                    {
                        sum += m1[i, it] * m2[it, j];
                    }
                    m3[i, j] = sum;
                }
            }
            return m3;
        }

        public static Matrix operator *(Matrix m, double scalar)
        {
            return new Matrix(Multiply(m, scalar));
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            return new Matrix(Multiply(m1, m2));
        }

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < rows; ++i)
            {
                if (i > 0)
                {
                    res += "|";
                }
                for (int j = 0; j < cols; ++j)
                {
                    if (j > 0)
                    {
                        res += ",";
                    }
                    res += matrix[i, j];
                }
            }
            return "(" + res + ")";
        }
    }

   public class Matrix3 : Matrix
    {
        public Matrix3()
            : base(3, 3)
        {
        }

        public Matrix3(double[,] matrix)
            : base(matrix)
        {
            if (rows != 3 || cols != 3)
            {
                throw new ArgumentException();
            }
        }

        public static Matrix3 I()
        {
            return new Matrix3(new double[,] {
        { 1.0f, 0.0f, 0.0f },
        { 0.0f, 1.0f, 0.0f },
        { 0.0f, 0.0f, 1.0f } });
        }

        public static Vector3 operator *(Matrix3 matrix3, Vector3 v)
        {
            double[,] m = matrix3.matrix;
            return new Vector3(
                m[0, 0] * v.x + m[0, 1] * v.y + m[0, 2] * v.z,
                m[1, 0] * v.x + m[1, 1] * v.y + m[1, 2] * v.z,
                m[2, 0] * v.x + m[2, 1] * v.y + m[2, 2] * v.z);
        }

        public static Matrix3 operator *(Matrix3 mat1, Matrix3 mat2)
        {
            double[,] m1 = mat1.matrix;
            double[,] m2 = mat2.matrix;
            double[,] m3 = new double[3, 3];
            m3[0, 0] = m1[0, 0] * m2[0, 0] + m1[0, 1] * m2[1, 0] + m1[0, 2] * m2[2, 0];
            m3[0, 1] = m1[0, 0] * m2[0, 1] + m1[0, 1] * m2[1, 1] + m1[0, 2] * m2[2, 1];
            m3[0, 2] = m1[0, 0] * m2[0, 2] + m1[0, 1] * m2[1, 2] + m1[0, 2] * m2[2, 2];
            m3[1, 0] = m1[1, 0] * m2[0, 0] + m1[1, 1] * m2[1, 0] + m1[1, 2] * m2[2, 0];
            m3[1, 1] = m1[1, 0] * m2[0, 1] + m1[1, 1] * m2[1, 1] + m1[1, 2] * m2[2, 1];
            m3[1, 2] = m1[1, 0] * m2[0, 2] + m1[1, 1] * m2[1, 2] + m1[1, 2] * m2[2, 2];
            m3[2, 0] = m1[2, 0] * m2[0, 0] + m1[2, 1] * m2[1, 0] + m1[2, 2] * m2[2, 0];
            m3[2, 1] = m1[2, 0] * m2[0, 1] + m1[2, 1] * m2[1, 1] + m1[2, 2] * m2[2, 1];
            m3[2, 2] = m1[2, 0] * m2[0, 2] + m1[2, 1] * m2[1, 2] + m1[2, 2] * m2[2, 2];
            return new Matrix3(m3);
        }

        public static Matrix3 operator *(Matrix3 m, double scalar)
        {
            return new Matrix3(Multiply(m, scalar));
        }
    }
}
