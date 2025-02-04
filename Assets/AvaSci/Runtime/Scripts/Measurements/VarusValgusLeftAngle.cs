using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;

public class VarusValgusLeftAngle : Measurement
{
  public VarusValgusLeftAngle()
  {
    Type = MeasurementType.VarusValgusLeftAngleDistance;
     KeyJoint1 = JointType.HipLeft;
        KeyJoint2 = JointType.KneeLeft;
        KeyJoint3 = JointType.KneeLeft;
  }
    public override void Update(Body body)
    {
        base.Update(body);
        if (ReferenceManager.instance.ignoreLowConfidenceJoints &&
            ((body.Joints[KeyJoint1] != null && body.Joints[KeyJoint1].Confidence < 0.3f) ||
             (body.Joints[KeyJoint2] != null && body.Joints[KeyJoint2].Confidence < 0.3f) ||
             (body.Joints[KeyJoint3] != null && body.Joints[KeyJoint3].Confidence < 0.3f)))
        {
            return; // Prevent further execution
        }
       HipKneeLeftDistance hipKneeLeftDistance = new HipKneeLeftDistance();
            hipKneeLeftDistance.Update(body);
            HipKneeLeftDifference hipKneeLeftDifference = new HipKneeLeftDifference();
            hipKneeLeftDifference.Update(body);
        _value = (float)(hipKneeLeftDistance.Value * Math.Sin((double)hipKneeLeftDifference.Value*3.14159/180));
        
        Joint hipLeft = body.Joints[KeyJoint1];
        Joint kneeLeft = body.Joints[KeyJoint2];
        _angleStart = hipLeft.Position2D;
        _angleCenter = kneeLeft.Position2D;
        _angleEnd = kneeLeft.Position2D;
        
        ResearchMeasurementManager.instance.leftDisValue = _value;
         
    }
}
