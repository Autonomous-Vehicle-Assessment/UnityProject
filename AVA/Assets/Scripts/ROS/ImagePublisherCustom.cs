/*
© CentraleSupelec, 2017
Author: Dr. Jeremy Fix (jeremy.fix@centralesupelec.fr)

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

// Adjustments to new Publication Timing and Execution Framework 
// © Siemens AG, 2018, Dr. Martin Bischoff (martin.bischoff@siemens.com)

using UnityEngine;
using System.Collections;

namespace RosSharp.RosBridgeClient
{
    public class ImagePublisherCustom : UnityPublisher<MessageTypes.Sensor.CompressedImage>
    {
        public Camera ImageCamera;
        public string FrameId = "Camera";
        public int resolutionWidth = 640;
        public int resolutionHeight = 480;
        [Range(0, 100)]
        public int qualityLevel = 50;

        private IEnumerator publishRoutine;
        [HideInInspector]
        public bool activeFeed;
        [Range(1, 30)]
        public float frequency = 1;

        [HideInInspector]
        public MessageTypes.Sensor.CompressedImage message;
        private Texture2D texture2D;
        private Rect rect;

        public PoseStampedPublisher posePublisher;

        protected override void Start()
        {
            base.Start();
            InitializeGameObject();
            InitializeMessage();
            //Camera.onPostRender += UpdateImage;

            publishRoutine = PublishRoutine();
            activeFeed = false;
            BeginDataTransfer();
        }

        private void UpdateImage(Camera _camera)
        {
            if (texture2D != null && _camera == this.ImageCamera)
                UpdateMessage();
                posePublisher.message.header.stamp = message.header.stamp;
                posePublisher.UpdateMessage();
                Camera.onPostRender -= UpdateImage;
        }

        private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
            ImageCamera.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Sensor.CompressedImage();
            message.header.frame_id = FrameId;
            message.format = "jpeg";
        }

        private void UpdateMessage()
        {
            message.header.Update();
            texture2D.ReadPixels(rect, 0, 0);
            message.data = texture2D.EncodeToJPG(qualityLevel);
            Publish(message);
        }

        public void BeginDataTransfer()
        {
            activeFeed = true;
            StartCoroutine(publishRoutine);
        }

        public void StopDataTransfer()
        {
            activeFeed = false;
            StopCoroutine(publishRoutine);
        }

        private void OnApplicationQuit()
        {
            if (activeFeed) StopDataTransfer();
        }

        IEnumerator PublishRoutine()
        {
            while (activeFeed)
            {
                yield return new WaitForSeconds(1f / frequency); 
                Camera.onPostRender += UpdateImage;
                //UpdateMessage();
            }
        }
    }
}
