using System.Collections.Generic;
using System.Text;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// Checks if the tracking confidence of key joints is low.
    /// </summary>
    public class TrackingConfidenceWarning : Warning
    {
        [SerializeField][Range(0, 1)] private float _minConfidence = 0.5f;

        public override void Check(FrameData frame = null, Body body = null, Movement movement = null)
        {
            base.Check(frame, body, movement);

            _display = false;

            if (body == null) return;
            if (movement == null) return;

            HashSet<JointType> criticalJoints = new HashSet<JointType>();

            foreach (var m in movement.MeasurementValues)
            {
                criticalJoints.Add(m.KeyJoint1);
                criticalJoints.Add(m.KeyJoint2);
                criticalJoints.Add(m.KeyJoint3);
            }

            List<JointType> lowConfidenceJoints = new List<JointType>();

            foreach (JointType type in criticalJoints)
            {
                var joint = body.Joints[type];

                if (joint.Confidence < _minConfidence)
                {
                    lowConfidenceJoints.Add(type);
                }
            }

            if (lowConfidenceJoints.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < lowConfidenceJoints.Count; i++)
                {
                    sb.Append(lowConfidenceJoints[i]);

                    if (i < lowConfidenceJoints.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                _message = $"Low Tracking Joints: {sb}";
            }

            _display = lowConfidenceJoints.Count > 0;
        }
    }
}