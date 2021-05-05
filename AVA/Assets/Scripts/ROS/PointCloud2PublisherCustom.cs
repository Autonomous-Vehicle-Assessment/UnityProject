/*
© Aarhus University 2020
Author: Mads Rosenhøj Jeppesen (madsrosenhoej@gmail.com)

Modified version of PoseStampedPublisher.cs originally created and licenced by:

    © Siemens AG, 2017-2018
    Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
    <http://www.apache.org/licenses/LICENSE-2.0>.
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
*/

using UnityEngine;
using System.Collections;

namespace RosSharp.RosBridgeClient 
{
    public class PointCloud2PublisherCustom : UnityPublisher<MessageTypes.Sensor.PointCloud2>
    {
        public string FrameId = "Unity";
        public LiDAR lidar;
        private uint width;
        private MessageTypes.Sensor.PointCloud2 message;
        [HideInInspector]   public bool activeFeed;
        [Range(1,10)]       public float frequency = 1;
        private IEnumerator publishRoutine;


        protected override void Start()
        {
            base.Start();
            InitializeMessage();

            publishRoutine = PublishRoutine();
            StartDataTransfer();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Sensor.PointCloud2
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                },

                height = 1,
                width = 1,

                fields = GetPointField(),

                is_bigendian = false,
                point_step = 12,
                row_step = 1,
                is_dense = true,
            };
        }

        private void StartDataTransfer()
        {
            activeFeed = true;
            StartCoroutine(publishRoutine);
        }

        private void StopDataTransfer()
        {
            activeFeed = false;
            StopCoroutine(publishRoutine);
        }

        private void OnApplicationQuit()
        {
            if(activeFeed) StopDataTransfer();
        }
                
        private void UpdateMessage()
        {
            message.header.Update();

            message.data = GetPointCloudData();
            message.width = width;

            Publish(message);
        }

        private byte[] GetPointCloudData()
        {
            // LiDAR Depth data
            Vector3[] pointsUnity = GetDepth();
            Vector3[] pointsROS = ConvertVector3ArrayUnityToRosArray(pointsUnity);
            float[] dataFloats = ConvertVector3ArrayToFloatArray(pointsROS);
            
            byte[] data = ConvertFloatToByteArray(dataFloats);

            return data;
        }

        private static MessageTypes.Sensor.PointField[] GetPointField()
        {
            MessageTypes.Sensor.PointField[] pointFields = new MessageTypes.Sensor.PointField[3];

            pointFields[0] = new MessageTypes.Sensor.PointField
            {
                name = "x",
                offset = 0,
                datatype = MessageTypes.Sensor.PointField.FLOAT32,
                count = 1
            };

            pointFields[1] = new MessageTypes.Sensor.PointField
            {
                name = "y",
                offset = 4,
                datatype = MessageTypes.Sensor.PointField.FLOAT32,
                count = 1
            };

            pointFields[2] = new MessageTypes.Sensor.PointField
            {
                name = "z",
                offset = 8,
                datatype = MessageTypes.Sensor.PointField.FLOAT32,
                count = 1
            };

            //pointFields[3] = new MessageTypes.Sensor.PointField
            //{
            //    name = "intensity",
            //    offset = 12,
            //    datatype = MessageTypes.Sensor.PointField.FLOAT32,
            //    count = 1
            //};

            return pointFields;
        }

        private Vector3[] GetDepth()
        {
            Vector3[] depths = lidar.GetDepthArray();
            width = (uint)depths.Length;

            return depths;
        }

        private static void GetDepth(out Vector3[] point, out int[] intensity)
        {
            point = new Vector3[1] { new Vector3(0, 1, 0) };
            intensity = new int[1] { 100 };
        }

        static Vector3[] ConvertVector3ArrayUnityToRosArray(Vector3[] positions)
        {
            Vector3[] rosPositions = new Vector3[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                rosPositions[i] = positions[i].Unity2Ros();
            }

            return rosPositions;
        }

        static float[] ConvertVector3ArrayToFloatArray(Vector3[] vectors)
        {
            float[] ret = new float[vectors.Length * 3];

            for (int i = 0; i < vectors.Length; i++)
            {
                int index = i * 3;

                ret[index + 0] = vectors[i].x;
                ret[index + 1] = vectors[i].y;
                ret[index + 2] = vectors[i].z;
            }

            return ret;
        }

        static byte[] ConvertFloatToByteArray(float[] floats)
        {
            byte[] ret = new byte[floats.Length * 4];// a single float is 4 bytes/32 bits

            int index = 0;
            for (int i = 0; i < floats.Length; i++)
            {
                ret[index + 0] = System.BitConverter.GetBytes(floats[i])[0];
                ret[index + 1] = System.BitConverter.GetBytes(floats[i])[1];
                ret[index + 2] = System.BitConverter.GetBytes(floats[i])[2];
                ret[index + 3] = System.BitConverter.GetBytes(floats[i])[3];

                index += 4;
            }
            return ret;
        }

        IEnumerator PublishRoutine()
        {
            while (activeFeed)
            {
                yield return new WaitForSeconds(1f / frequency);

                UpdateMessage();
            }
        }
    }
}
