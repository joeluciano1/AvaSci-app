using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

public class StepLeftAngle : Measurement
{
   public StepLeftAngle()
   {
      Type = MeasurementType.StepLeftAngle;

      KeyJoint1 = JointType.AnkleLeft;
      KeyJoint2 = JointType.Pelvis;
      KeyJoint3 = JointType.Neck;
   }

   public override void Update(Body body)
   {
      base.Update(body);
      var ankle = body.Joints[KeyJoint1];
      var pelvis = body.Joints[KeyJoint2];
      var neck = body.Joints[KeyJoint3];
      float value = Calculations.Angle(ankle.Position3D, pelvis.Position3D, neck.Position3D);
      
      _value = value;
      _angleStart = ankle.Position2D;
      _angleCenter = ankle.Position2D;
      _angleEnd = ankle.Position2D;
   }
}
