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

// Added allocation free alternatives
// UoK , 2019, Odysseas Doumas (od79@kent.ac.uk / odydoum@gmail.com)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{
    public class PathPublisher : UnityPublisher<MessageTypes.Nav.Path>
    {
        public Transform pathMaster;
        public string FrameId = "map";
        public bool activeFeed;

        private MessageTypes.Nav.Path message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            if(activeFeed) UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Nav.Path
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                }
            };
        }

        private void UpdateMessage()
        {
            message.header.Update();

            message.poses = GetPose(pathMaster);

            Publish(message);
        }

        private MessageTypes.Geometry.PoseStamped[] GetPose(Transform pathMaster)
        {
            WaypointPath[] pathArray = pathMaster.GetComponentsInChildren<WaypointPath>();
            List<MessageTypes.Geometry.PoseStamped> poseList = new List<MessageTypes.Geometry.PoseStamped>();

            MessageTypes.Std.Header header = new MessageTypes.Std.Header()
            {
                frame_id = FrameId
            };

            foreach (WaypointPath path in pathArray)
            {
                foreach (PathNode node in path.pathNodes)
                {
                    MessageTypes.Geometry.Point nodePoint = new MessageTypes.Geometry.Point();
                    MessageTypes.Geometry.Quaternion nodeQuaternion = new MessageTypes.Geometry.Quaternion();

                    GetGeometryPoint(node.transform.position.Unity2Ros(), nodePoint);
                    GetGeometryQuaternion(node.transform.rotation.Unity2Ros(), nodeQuaternion);

                    MessageTypes.Geometry.Pose nodePose = new MessageTypes.Geometry.Pose(nodePoint, nodeQuaternion);
                    MessageTypes.Geometry.PoseStamped nodePoseStamp = new MessageTypes.Geometry.PoseStamped(header, nodePose);

                    poseList.Add(nodePoseStamp);
                }
            }

            MessageTypes.Geometry.PoseStamped[]  pathList = poseList.ToArray();

            return pathList;
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

    }
}
