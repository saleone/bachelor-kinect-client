using Microsoft.Kinect;
using System;
using System.Threading;

namespace BachelorsGetHandPosition
{
    class GetHandPosition
    {
        public static KinectSensor sensor = null;

        static void Main(string[] args)
        {
            sensor = GetKinectSensor();
            int elevation;
            try
            {
                elevation = int.Parse(args[1]);
            }
            catch (IndexOutOfRangeException)
            {
                elevation = 15;
            }

            if (elevation >= 20 && elevation <= 0)
            {
                Console.WriteLine();
            }
            

            sensor.Start();


            // When connected to the sensor dont let the application to close
            while (true) ;

        }

        static void Destruct()
        {
            if (sensor != null)
            {
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

            Vector3 pos = GetHandPositionVector(skeleton);
            Console.WriteLine(string.Format("Hand position vector is: ({0}, {1}, {2})", pos.X, pos.Y, pos.Z));

        }

        private static Vector3 GetHandPositionVector(Skeleton sk)
        {
            Joint shoulder = sk.Joints[JointType.ShoulderRight];
            Joint hand = sk.Joints[JointType.HandRight];

            Vector3 shoulderVec = new Vector3(shoulder.Position.X, shoulder.Position.Y, shoulder.Position.Z);
            Vector3 handVec = new Vector3(hand.Position.X, hand.Position.Y, hand.Position.Z);

            return handVec - shoulderVec;
        }

    }
}
