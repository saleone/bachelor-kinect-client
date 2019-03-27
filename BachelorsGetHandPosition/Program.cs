// Program.cs
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


﻿using Microsoft.Kinect;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BachelorsGetHandPosition
{
    class GetHandPosition
    {
        public static KinectSensor sensor = null;
        public static Socket client = null;
        public static int frameCounter = 0;
        public static int initialElevationAngle = 15;
        public static int framerateFactor = 5;

        public static int armLength = 54;

        public static bool sendMessages = true;

        // Exception messages
        public static readonly string AngleChangeErrorMessageFormat = "An error occurred while {0} elevation angle. Please try again.";

        static void Main(string[] args)
        {
            sensor = GetKinectSensor();
            sensor.Start();

            // When connected to the sensor dont let the application to close
            ConsoleKeyInfo pressed;
            while (true)
            {
                pressed = Console.ReadKey();
                switch (pressed.Key)
                {
                    case ConsoleKey.Escape:
                        StopApp();
                        return;
                    case ConsoleKey.Spacebar:
                        ChangeSensorTrackingMode();
                        break;
                    case ConsoleKey.UpArrow:
                        IncreaseSensorElevationAngle();
                        break;
                    case ConsoleKey.DownArrow:
                        DecreaseSensorElevationAngle();
                        break;
                    case ConsoleKey.R:
                        Console.WriteLine("Reseting sensor elevation.");
                        sensor.ElevationAngle = (sensor.MaxElevationAngle - sensor.MinElevationAngle) / 2;
                        break;
                    case ConsoleKey.RightArrow:
                        IncreaseFramerateFactor();
                        break;
                    case ConsoleKey.LeftArrow:
                        DecreaseFramerateFactor();
                        break;
                }
            }
        }
        /// <summary>
        /// Safely exits the application.
        /// </summary>
        static void StopApp()
        {
            Console.WriteLine("Shutting down...");
            if (sensor != null)
            {
                Console.WriteLine("  Stopping sensor.");
                sensor.Stop();
                sensor.AudioSource.Stop();
            }

            if (client != null)
            {
                Console.WriteLine("  Closing server connection.");
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        /// <summary>
        /// Return initialized Kinect sensor object.
        /// </summary>
        /// <returns>KinectSensor</returns>
        static KinectSensor GetKinectSensor()
        {
            Console.Write("Connecting to Kinect sensor");
            KinectSensor _sensor = null;
            while (_sensor == null || _sensor.Status == KinectStatus.Connected)
            {
                if (KinectSensor.KinectSensors.Count > 0)
                {
                    _sensor = KinectSensor.KinectSensors[0];
                    if (_sensor.Status == KinectStatus.Connected)
                    {
                        _sensor.SkeletonStream.Enable(new TransformSmoothParameters {
                            Correction = 0.9f,
                            Prediction = 0.0f,
                            Smoothing = 0.9f,
                            JitterRadius = 0.02f,
                            MaxDeviationRadius = 0.01f,
                        });
                        _sensor.SkeletonStream.EnableTrackingInNearRange = true;

                        _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                        _sensor.ColorStream.Enable();
                        _sensor.SkeletonFrameReady += _sensor_SkeletonFrameReady;
                        Console.WriteLine("\n Connected.");
                        return _sensor;
                    }

                }
                Console.Write(".");
                Thread.Sleep(1000);
            }

            return _sensor;

        }

        /// <summary>
        /// Handles skeleton changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void _sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            if ((frameCounter++ % framerateFactor) != 0)
            {
                return;
            }

            Skeleton[] skeletonData = new Skeleton[6];
            using (var skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;
                skeletonFrame.CopySkeletonDataTo(skeletonData);
            }

            Skeleton skeleton = null;
            foreach (Skeleton s in skeletonData)
            {
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                    skeleton = s;
            }

            if (skeleton == null)
                return;

            Vector3 pos = GetHandPositionVector(skeleton) * 100f;
            if (!DifferentEnough(previousSent, pos)) return;
            
            Vector3 p = Normalize(pos) * 100;
            string msg = string.Format("{0};{1};{2}", p.X, p.Z, p.Y).Replace(",", ".");
            Console.WriteLine(msg);
            SendMessage(msg);
            previousSent = pos;
        }

        private static Vector3 previousSent = new Vector3(0f, 0f, 0f);

        private static bool DifferentEnough(Vector3 prev, Vector3 current)
        {
            int min_change = 2;
            Vector3 change = prev - current;
            return change.Intensity > min_change;
        }

        private static float Normalize(float val)
            => val / armLength;

        private static Vector3 Normalize(Vector3 val) =>
            new Vector3(Normalize(val.X), Normalize(val.Y), Normalize(val.Z));

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <returns></returns>
        private static void ConnectToServer()
        {
            int retryCount = 0;
            client = null;
            while (client == null)
            {
                ++retryCount;
                if (retryCount == 10)
                {
                    Console.WriteLine("Could not connect to server. Data won't be sent.");
                    break;
                }

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    client.Connect("localhost", 7777);
                }
                catch
                {
                    client = null;
                    continue;
                }
            }
        }

        /// <summary>
        /// Send a message to the server.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static int SendMessage(string message)
        {
            if (!sendMessages)
                return 0;

            ConnectToServer();
            if (client != null)
                return client.Send(Encoding.ASCII.GetBytes(message));
            return 0;
        }

        private static Vector3 GetHandPositionVector(Skeleton sk)
        {
            Joint shoulder = sk.Joints[JointType.ShoulderRight];
            Joint wrist = sk.Joints[JointType.WristRight];
            Joint hand = sk.Joints[JointType.HandRight];
            Joint elbow = sk.Joints[JointType.ElbowRight];

            Vector3 shoulderVec = new Vector3(shoulder.Position.X, shoulder.Position.Y, shoulder.Position.Z);
            Vector3 handVec = new Vector3(wrist.Position.X, wrist.Position.Y, wrist.Position.Z);

            return handVec - shoulderVec;
        }

        private static void ChangeSensorTrackingMode()
        {
            Console.Write("Changing tracking mode:");
            if (sensor.SkeletonStream.TrackingMode == SkeletonTrackingMode.Seated)
            {
                Console.WriteLine(" Seated -> Default ");
                sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }
            else
            {
                Console.WriteLine(" Default -> Seated ");
                sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            }
        }

        private static void IncreaseSensorElevationAngle()
        {
            if (sensor.ElevationAngle < sensor.MaxElevationAngle)
            {
                try
                {
                    sensor.ElevationAngle += 1;
                    Console.Write("Increasing sensor elevation angle to: ");
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine(string.Format(AngleChangeErrorMessageFormat, "increasing"));
                }
            }
            else
            {
                Console.Write("Sensor is already at its highest elevation angle: ");
            }
            Console.WriteLine(sensor.ElevationAngle);
        }

        private static void DecreaseSensorElevationAngle()
        {
            if (sensor.ElevationAngle > sensor.MinElevationAngle)
            {
                try
                {
                    sensor.ElevationAngle -= 1;
                    Console.Write("Decreasing sensor elevation angle to: ");
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine(string.Format(AngleChangeErrorMessageFormat, "decreasing"));
                }
            }
            else
            {
                    Console.Write("Sensor is already at its lowest elevation angle: ");
                }
                Console.WriteLine(sensor.ElevationAngle);
            }

        private static void IncreaseFramerateFactor()
        {
            framerateFactor += 1;
            Console.WriteLine("Increasing framerate factor to: " + framerateFactor);
        }

        private static void DecreaseFramerateFactor()
        {
            if (framerateFactor > 1)
                framerateFactor -= 1;
            Console.WriteLine("Decreasing framerate factor to: " + framerateFactor);
        }
    }
}
