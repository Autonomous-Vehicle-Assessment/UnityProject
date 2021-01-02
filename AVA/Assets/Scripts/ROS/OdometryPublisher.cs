/*
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

namespace RosSharp.RosBridgeClient
{
    public class OdometryPublisher : UnityPublisher<MessageTypes.Nav.Odometry>
    {
        public Transform PublishedTransform;
        public string FrameId = "map";
        public string childFrameID = "base_link";


        private Vector3 previousPosition = Vector3.zero;
        private Quaternion previousRotation = Quaternion.identity;
        private MessageTypes.Nav.Odometry message;


        protected override void Start()
		{
			base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Nav.Odometry
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                },
                child_frame_id = childFrameID,
                twist = new MessageTypes.Geometry.TwistWithCovariance(),
                pose = new MessageTypes.Geometry.PoseWithCovariance()
            };
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            message.header.Update();
            GetGeometryPoint(PublishedTransform.position.Unity2Ros(), message.pose.pose.position);
            GetGeometryQuaternion(PublishedTransform.rotation.Unity2Ros(), message.pose.pose.orientation);

            Vector3 linearVelocity = (Vector3.zero - PublishedTransform.InverseTransformPoint(previousPosition)) / Time.fixedDeltaTime;
            Vector3 angularVelocity = PublishedTransform.InverseTransformDirection((PublishedTransform.rotation.eulerAngles * Mathf.Deg2Rad - previousRotation.eulerAngles * Mathf.Deg2Rad) / Time.fixedDeltaTime);

            message.twist.twist.linear = GetGeometryVector3(linearVelocity.Unity2Ros());
            message.twist.twist.angular = GetGeometryVector3(-angularVelocity.Unity2Ros());

            previousPosition = PublishedTransform.position;
            previousRotation = PublishedTransform.rotation;

            Publish(message);
        }
        private static void GetGeometryPoint(Vector3 position, MessageTypes.Geometry.Point geometryPoint)
        {
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = 0; // position.z;
        }

        private static void GetGeometryQuaternion(Quaternion quaternion, MessageTypes.Geometry.Quaternion geometryQuaternion)
        {
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
        }

        private static MessageTypes.Geometry.Vector3 GetGeometryVector3(Vector3 vector3)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = vector3.x;
            geometryVector3.y = vector3.y;
            geometryVector3.z = vector3.z;
            return geometryVector3;
        }
    }
}