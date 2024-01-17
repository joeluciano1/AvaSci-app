using System;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// Checks if the user is too close or too far from the camera.
    /// If a LiDAR sensor is used, the distance is calculated based on the depth data.
    /// - Adjust the <see cref="_minDistance"/> and <see cref="_maxDistance"/> values to change the distance range.
    /// If an RGB camera is used, the distance is calculated based on the bounding box size.
    /// The bounding box size is calculated based on the user's height.
    /// - Adjust the <see cref="_minRatio"/> and <see cref="_maxRatio"/> values to change the bounding box size range.
    /// </summary>
    public class DistanceWarning : Warning
    {
        [Header("3D World constraints")]
        [SerializeField][Range(1, 5)] private float _minDistance = 1.5f;
        [SerializeField][Range(1, 5)] private float _maxDistance = 3.5f;

        [Header("2D Screen constraints")]
        [SerializeField][Range(0, 1)] private float _minRatio = 0.5f;
        [SerializeField][Range(0, 1)] private float _maxRatio = 0.9f;

        public override void Check(FrameData frame = null, Body body = null, Movement movement = null)
        {
            base.Check();

            _display = false;

            if (frame == null) return;
            if (body == null) return;

            float distance = body.Joints[JointType.Neck].Position3D.Z;

            bool isValidDistance = false;

            if (frame.DepthData != null)
            {
                // We have depth data!
                // Check the actual distance.
                isValidDistance = distance >= _minDistance && distance <= _maxDistance;
            }
            else
            {
                // No depth data :-(
                // Check the distance based on the bounding box size.
                var bbox = body.BoundingBox2D;
                float ratio = bbox.Height / (float)frame.Height;

                isValidDistance = ratio >= _minRatio && ratio <= _maxRatio;
            }

            _message =
                isValidDistance ? string.Empty : "Too Close or Too Far";
            // $"Distance should be between {_minDistance:N2} and {_maxDistance:N2}m.";

            _display = !isValidDistance;
        }
    }
}