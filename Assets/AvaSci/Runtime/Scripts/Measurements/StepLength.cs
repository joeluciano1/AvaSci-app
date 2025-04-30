using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

public class StepLength : Measurement
{
   public StepLength()
   {
      Type = MeasurementType.StepLength;
      KeyJoint1 = JointType.AnkleLeft;
      KeyJoint2 = JointType.AnkleRight;
   }

   public override void Update(Body body)
   {
      base.Update(body);
      var ankleLeft = body.Joints[KeyJoint1];
      var ankleRight = body.Joints[KeyJoint2];

      float stepLength = Calculations.Distance(ankleLeft.Position3D, ankleRight.Position3D);
      stepLength *= 1000;
      _value = stepLength;
      _angleStart = ankleLeft.Position2D;
      _angleCenter = (ankleRight.Position2D + ankleLeft.Position2D)/2;
      _angleEnd = ankleRight.Position2D;
   }
}
