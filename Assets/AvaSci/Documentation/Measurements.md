# Measurements

The AvaSci app supports the following measurements:

- Neck lateral flexion
- Neck rotation
- Left shoulder abduction
- Right shoulder abduction
- Left shoulder rotation
- Right shoulder rotation
- Left elbow flexion
- Right elbow flexion
- Hip left abduction
- Hip right abduction
- Hip left flexion
- Hip right flexion
- Knee left flexion
- Knee right flexion

## Description of a measurement

A measurement is a value that is calculated from the skeleton data.

To update a measurement, you need to call the `Update()` method of the measurement. The `Update()` method takes the skeleton data as a parameter.

The measurement value is measured in degrees. You can retrieve it by calling the `Value` property. The angles are calculated in the 3D space.

The `Measurement` class also provides the following properties:

- `Type` - the `MeasurementType` of the measurement.
- `JointStart` - the first key joint.
- `JointCenter` - the second key joint.
- `JointEnd` - the third key joint.

**For visualization purposes**, each measurement also provides properties that allow you to draw an angle in the 2D space. These properties are:

- `AngleStart` - the starting point of the angle in the 2D space.
- `AngleCenter` - the center point of the angle in the 2D space.
- `AngleEnd` - the ending point of the angle in the 2D space.

## How to create new measurements

To create a new measurement, follow these steps:

1. Create a new `MeasurementType` enum value in `MeasurementType.cs`.
1. Create a new class that inherits from `Measurement`.
1. Add the new measurement in the `Measurement.Create()` factory method.
1. Implement the new measurement Constructor and `Update()` method.