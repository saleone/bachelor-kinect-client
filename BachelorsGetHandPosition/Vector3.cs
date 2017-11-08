using System;

namespace BachelorsGetHandPosition
{
    internal class Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = (float)Math.Round(x, 2);
            Y = (float)Math.Round(y, 2);
            Z = (float)Math.Round(z, 2);
        }

        public static Vector3 operator +(Vector3 first, Vector3 second)
        {
            return new Vector3(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
        }

        public static Vector3 operator -(Vector3 f, Vector3 s)
        {
            return new Vector3(f.X - s.X, f.Y - s.Y, f.Z - s.Z);
        }

        public static Vector3 operator *(Vector3 f, int num)
        {
            return new Vector3(f.X * num, f.Y * num, f.Z * num);
        }

    }
}