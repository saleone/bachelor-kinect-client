using Microsoft.Kinect;
using System;
using System.Threading;

namespace BachelorsGetHandPosition
{
    class GetHandPosition
    {
        public static KinectSensor sensor = null;
        public static int frameCounter = 0;
        public static int initialElevationAngle = 15;
        public static int framerateFactor = 10;

        // Exception messages
        public static readonly string AngleChangeErrorMessageFormat = "An error occurred while {0} elevation angle. Please try again.";

        static void Main(string[] args)
        {
            sensor = GetKinectSensor();
            sensor.Start();
            sensor.ElevationAngle = initialElevationAngle;


            // When connected to the sensor dont let the application to close
            ConsoleKeyInfo pressed;
            while (true)
            {
                pressed = Console.ReadKey();
                switch (pressed.Key)
                {
                    case ConsoleKey.Escape:
                        StopSensor();
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
                        sensor.ElevationAngle = initialElevationAngle;
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

        static void StopSensor()
        {
            if (sensor != null)
            {
                Console.WriteLine("Stopping sensor.");
                sensor.Stop();
                sensor.AudioSource.Stop();
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
                        _sensor.SkeletonStream.Enable();
                        _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
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

            Vector3 pos = GetHandPositionVector(skeleton) * 100;
            Console.WriteLine(string.Format("({0}, {1}, {2})", pos.X, pos.Y, pos.Z));

        }

        private static Vector3 GetHandPositionVector(Skeleton sk)
        {
            Joint shoulder = sk.Joints[JointType.ShoulderRight];
            Joint hand = sk.Joints[JointType.HandRight];

            Vector3 shoulderVec = new Vector3(shoulder.Position.X, shoulder.Position.Y, shoulder.Position.Z);
            Vector3 handVec = new Vector3(hand.Position.X, hand.Position.Y, hand.Position.Z);

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
                framerateFactor -= 1;
                Console.WriteLine("Decreasing framerate factor to: " + framerateFactor);
            }
        }
    }
