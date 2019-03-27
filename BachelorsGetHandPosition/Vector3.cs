// Vector3.cs
//
// Copyright 2019 Saša Savić <sasa@sasa-savic.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//
// SPDX-License-Identifier: MIT


﻿using System;

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

        public double Intensity =>
            Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));

        public static Vector3 operator +(Vector3 first, Vector3 second) => 
            new Vector3(first.X - second.X, first.Y - second.Y, first.Z - second.Z);

        public static Vector3 operator -(Vector3 f, Vector3 s) => 
            new Vector3(f.X - s.X, f.Y - s.Y, f.Z - s.Z);

        public static Vector3 operator *(Vector3 f, float num) => 
            new Vector3(f.X * num, f.Y * num, f.Z * num);
    }
}