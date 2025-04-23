using System;
using LightBuzz.BodyTracking;
using UnityEngine;
using UnityEngine.UI;

public class LightBuzz_BodyTracking_Floor : MonoBehaviour
{
    [SerializeField] private DeviceConfiguration _configuration;
    [SerializeField] private SkeletonManager _skeletons;
    [SerializeField] private LightBuzzViewer _viewer;

    [Header("Floor options")]
    [SerializeField][Range(1, 10)] private int _downsample = 4;

    [Header("Floor demo")]
    [SerializeField] private JointType _joint = JointType.Neck;
    [SerializeField] private Transform _floorPlane;
    [SerializeField] private Transform _camera;
    [SerializeField] private Text _infoFOV;
    [SerializeField] private Text _infoHeight;
    [SerializeField] private Text _infoTilt;
    [SerializeField] private Text _infoDistance;

    private Sensor _sensor;

    private void Start()
    {
        _sensor = Sensor.Create(_configuration);
        _sensor?.Open();

        if (_sensor == null || !_sensor.IsOpen)
        {
            Debug.LogError("Sensor is not open. Check the configuration settings.");
        }

        Body.UseDepthForLegs = true;
    }

    private void OnDestroy()
    {
        _sensor?.Close();
        _sensor?.Dispose();
    }

    private void Update()
    {
        if (_sensor == null || !_sensor.IsOpen) return;

        FrameData frame = _sensor.Update();

        if (frame == null) return;

        Floor floor = Floor.Create(frame, _downsample);

        if (floor != null)
        {
            // Display the floor data.
            FieldOfView fov = floor.FieldOfView;
            float height = floor.Height;
            float tilt = floor.Tilt;

            _infoFOV.text = FormatInfo(fov, "Field of view", $"{fov}");
            _infoHeight.text = FormatInfo(fov, "Approximate device height", $"{height:N2}m");
            _infoTilt.text = FormatInfo(fov, "Device tilt", $"{tilt:N0}°");

            if (fov == FieldOfView.OK)
            {
                // Position the floor plane relative to the camera.
                _floorPlane.position = new Vector3(0, -height, 0);

                // Rotate the camera to match the floor tilt.
                _camera.rotation = UnityEngine.Quaternion.Euler(-tilt, 0, 0);

                Body body = frame.BodyData?.Default();

                if (body != null)
                {
                    // Display the vertical distance from the floor to the selected joint.
                    Vector3D point = body.Joints[_joint].Position3D;
                    float distance = floor.Distance(point, Axis.Y);

                    _infoDistance.text = $"Distance to {_joint}: <b>{distance:N2}m</b>";
                }
            }
        }

        _skeletons.Load(frame.BodyData);
        _viewer.Load(frame);
    }

    private string FormatInfo(FieldOfView fov, string label, string value)
    {
        string color = fov == FieldOfView.OK ? "green" : "red";

        return $"{label}: <color={color}><b>{value}</b></color>";
    }
}
