using LightBuzz.BodyTracking;
using UnityEngine;

public class LightBuzz_BodyTracking_RealSense : MonoBehaviour
{
    [SerializeField] public DeviceConfiguration _configuration;
    [SerializeField] private LightBuzzViewer _colorViewer;
    [SerializeField] private LightBuzzViewer _depthViewer;

    private Sensor _sensor;

    private void Start()
    {
        // _sensor = Sensor.Create(_configuration);
        // _sensor?.Open();
        //
        // if (_sensor == null || !_sensor.IsOpen)
        // {
        //     Debug.LogError("Sensor is not open. Check the configuration settings.");
        // }
    }

    private void OnDestroy()
    {
        _sensor?.Close();
        _sensor?.Dispose();
    }

    private void Update()
    {
        // if (_sensor == null || !_sensor.IsOpen) return;
        //
        // FrameData frame = _sensor.Update();
        //
        // if (frame != null)
        // {
        //     _colorViewer.Load(frame);
        //     _depthViewer.Load(frame);
        // }
    }
}
