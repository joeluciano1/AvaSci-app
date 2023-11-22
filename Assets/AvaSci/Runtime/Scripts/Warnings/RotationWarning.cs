using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// Checks if the user's torso is rotated.
    /// Adjust the <see cref="_maxRotation"/> value to change the maximum allowed rotation.
    /// </summary>
    public class RotationWarning : Warning
    {
        [SerializeField][Range(0, 90)] private float _maxRotation = 20.0f;

        public override void Check(FrameData frame = null, Body body = null, Movement movement = null)
        {
            base.Check(frame);

            _display = false;

            if (body == null) return;

            var shoulderLeft = body.Joints[JointType.ShoulderLeft];
            var shoulderRight = body.Joints[JointType.ShoulderRight];
            var hipLeft = body.Joints[JointType.HipLeft];
            var hipRight = body.Joints[JointType.HipRight];

            if (shoulderLeft.TrackingState == TrackingState.Inferred ||
                shoulderRight.TrackingState == TrackingState.Inferred)
            {
                _message = "Your upper torso is not visible.";
                _display = true;
                return;
            }

            if (hipLeft.TrackingState == TrackingState.Inferred ||
                hipRight.TrackingState == TrackingState.Inferred)
            {
                _message = "Your lower torso is not visible.";
                _display = true;
                return;
            }

            Vector3D shoulderLeft3D = shoulderLeft.Position3D;
            Vector3D shoulderRight3D = shoulderRight.Position3D;
            Vector3D hipLeft3D = hipLeft.Position3D;
            Vector3D hipRight3D = hipRight.Position3D;

            float shoulderRotation = 90.0f - Calculations.Rotation(shoulderLeft3D, shoulderRight3D, BodyTracking.Plane.Sagittal);
            float hipRotation = 90.0f - Calculations.Rotation(hipLeft3D, hipRight3D, BodyTracking.Plane.Sagittal);

            bool isValidRotation = 
                shoulderRotation <= _maxRotation &&
                hipRotation <= _maxRotation;

            _message =
                isValidRotation ? string.Empty :
                $"Keep your torso straight and facing the camera.";
            _display = !isValidRotation;
        }
    }
}