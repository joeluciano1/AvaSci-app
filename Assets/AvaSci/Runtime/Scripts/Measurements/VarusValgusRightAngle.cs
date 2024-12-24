using System;
using System.Collections;
using System.Collections.Generic;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;


public class VarusValgusRightAngle : Measurement
{
   
  public VarusValgusRightAngle()
  {
    Type = MeasurementType.VarusValgusRightAngleDistance;

     KeyJoint1 = JointType.HipRight;
        KeyJoint2 = JointType.KneeRight;
        KeyJoint3 = JointType.KneeRight;
  }
    public override void Update(Body body)
    {
        // base.Update(body);
        
        HipKneeRightDistance hipKneeRightDistance = new HipKneeRightDistance();
            hipKneeRightDistance.Update(body);
            HipKneeRightDifference hipKneeRightDifference = new HipKneeRightDifference();
            hipKneeRightDifference.Update(body);
        _value = (float)(hipKneeRightDistance.Value * Math.Sin((double)hipKneeRightDifference.Value*3.14159/180));
        
    Joint hipLeft = body.Joints[KeyJoint1];
    Joint kneeLeft = body.Joints[KeyJoint2];
    _angleStart = hipLeft.Position2D;
    _angleCenter = kneeLeft.Position2D;
    _angleEnd = kneeLeft.Position2D;
    
    ResearchMeasurementManager.instance.rightDisValue = _value;
    }
}
