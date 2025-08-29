using LightBuzz.BodyTracking;

namespace LightBuzz.AvaSci.Measurements
{
    public class HipRightRotation : HipLeftRotation
    {
        public HipRightRotation()
        {
            Type = MeasurementType.HipRightRotation;
            
            KeyJoint1 = JointType.HipRight;
            KeyJoint2 = JointType.KneeRight;
            KeyJoint3 = JointType.AnkleRight;
        }
        public override void Update(Body body)
        {
            base.Update(body);
        }
    }
}