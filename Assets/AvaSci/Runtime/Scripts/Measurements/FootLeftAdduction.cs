using System;
using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class FootLeftAdduction : Measurement
    {
        public FootLeftAdduction()
        {
            Type = MeasurementType.FootLeftAdduction;

            KeyJoint1 = JointType.AnkleLeft;
            KeyJoint2 = JointType.FootLeft;
            KeyJoint3 = JointType.HipLeft;
        }
         public override void Update(Body body)
        {
            base.Update(body);

            if (!body.Joints.ContainsKey(JointType.AnkleLeft) ||
                !body.Joints.ContainsKey(JointType.FootLeft)  ||
                !body.Joints.ContainsKey(JointType.HipLeft)   ||
                !body.Joints.ContainsKey(JointType.HipRight))
            {
                return;
            }

            Joint ankle = body.Joints[KeyJoint1];
            Joint toe   = body.Joints[KeyJoint2];   // use ToeLeft if available
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

            // Midline proxy = midpoint of both hips (projected usage is implicit in Calculations.Angle)
            Vector3D midHip = new Vector3D(
                (float)((hipL.Position3D.X + hipR.Position3D.X) * 0.5),
                (float)((hipL.Position3D.Y + hipR.Position3D.Y) * 0.5),
                (float)((hipL.Position3D.Z + hipR.Position3D.Z) * 0.5)
            );

            // Angle at the ankle between:
            //   ankle->midHip (toward midline)
            //   ankle->toe    (foot pointing direction)
            float angleDeg = Calculations.Angle(ankle.Position3D, midHip, toe.Position3D);

            // Sign: for the LEFT foot, inward (toward midline) means toe X moves RIGHT wrt ankle.
            float dx = toe.Position3D.X - ankle.Position3D.X;
            float sign;

// Midline is approximately between hips
            float midX = (hipL.Position3D.X + hipR.Position3D.X) * 0.5f;

// If foot moves away from midline → positive
            sign = MathF.Sign(dx * (ankle.Position3D.X - midX));

            _value = angleDeg * sign;

            // Gizmo lines (2D): center at ankle, show vectors toward midline and toe
            _angleCenter = ankle.Position2D;
            _angleStart  = ankle.Position2D;  // reference to midline
            _angleEnd    = toe.Position2D;

            // ResearchMeasurementManager.instance.footLeftAdduction = _value;
        }
    }
}