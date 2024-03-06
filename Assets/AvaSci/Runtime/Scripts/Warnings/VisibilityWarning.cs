using System.Collections.Generic;
using System.Text;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Warnings
{
    /// <summary>
    /// Checks if the key joints are not visible.
    /// </summary>
    public class VisibilityWarning : Warning
    {
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

            List<JointType> invisibleJoints = new List<JointType>();

            foreach (JointType type in criticalJoints)
            {
                var joint = body.Joints[type];

                if (joint.TrackingState == TrackingState.Inferred)
                {
                    invisibleJoints.Add(type);
                }
            }

            if (invisibleJoints.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < invisibleJoints.Count; i++)
                {
                    sb.Append(invisibleJoints[i]);

                    if (i < invisibleJoints.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                _message = $"The following joints are not visible: {sb}";
            }

            _display = invisibleJoints.Count > 0;
        }
    }
}