using System;
using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    /// <summary>
    /// Measures the adduction angle of the right foot (inward toward body midline).
    /// Positive when toes move toward midline.
    /// </summary>
    public class FootRightAdduction : Measurement
    {
        public FootRightAdduction()
        {
            Type = MeasurementType.FootRightAdduction;

            KeyJoint1 = JointType.AnkleRight;  // pivot
            KeyJoint2 = JointType.FootRight;   // or ToeRight if your SDK has it
            KeyJoint3 = JointType.HipRight;    // for mid-hip calc; we'll also read HipLeft
        }

        public override void Update(Body body)
        {
            base.Update(body);

            if (!body.Joints.ContainsKey(JointType.AnkleRight) ||
                !body.Joints.ContainsKey(JointType.FootRight)  ||
                !body.Joints.ContainsKey(JointType.HipLeft)     ||
                !body.Joints.ContainsKey(JointType.HipRight))
            {
                return;
            }

            Joint ankle = body.Joints[JointType.AnkleRight];
            Joint toe   = body.Joints[JointType.FootRight];  // use ToeRight if available
            Joint hipL  = body.Joints[JointType.HipLeft];
            Joint hipR  = body.Joints[JointType.HipRight];

            if (ReferenceManager.instance.ignoreLowConfidenceJoints)
            {
                if ((ankle != null && ankle.Confidence < 0.30f) ||
                    (toe   != null && toe.Confidence   < 0.30f) ||
                    (hipL  != null && hipL.Confidence  < 0.30f) ||
                    (hipR  != null && hipR.Confidence  < 0.30f))
                {
                    return;
                }
            }

            Vector3D midHip = new Vector3D(
                (float)((hipL.Position3D.X + hipR.Position3D.X) * 0.5),
                (float)((hipL.Position3D.Y + hipR.Position3D.Y) * 0.5),
                (float)((hipL.Position3D.Z + hipR.Position3D.Z) * 0.5)
            );

            float angleDeg = Calculations.Angle(ankle.Position3D, midHip, toe.Position3D);

            // Sign: for the RIGHT foot, inward (toward midline) means toe X moves LEFT wrt ankle.
            float dx = toe.Position3D.X - ankle.Position3D.X;
            float sign;

// Midline is approximately between hips
            float midX = (hipL.Position3D.X + hipR.Position3D.X) * 0.5f;

// If foot moves away from midline → positive
            sign = MathF.Sign(dx * (ankle.Position3D.X - midX));

            _value = angleDeg * sign;

            _angleCenter = ankle.Position2D;
            _angleStart  = ankle.Position2D;
            _angleEnd    = toe.Position2D;

            
        }
    }
}
