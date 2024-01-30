using LightBuzz.AvaSci.Csv;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.AvaSci.UI;
using LightBuzz.AvaSci.Warnings;
using LightBuzz.BodyTracking;
using UnityEngine;
using UnityEngine.UI;

namespace LightBuzz.AvaSci
{
    /// <summary>
    /// The main scene manager of the application.
    /// </summary>
    public class Main : MonoBehaviour
    {
        [SerializeField] private SettingsView _settingsView;
        [SerializeField] private LightBuzzViewer _viewer;
        [SerializeField] private AngleManager _angles;
        [SerializeField] public VideoRecordingView _videoRecorderView;
        [SerializeField] public VideoPlayerView _videoPlayerView;
        [SerializeField] private MeasurementSelector _measurementSelector;

        [SerializeField] private WarningCollection _warnings;

        [SerializeField] TMPro.TMP_Text _loading;
        [SerializeField] TMPro.TMP_Text _debug;
        [SerializeField] TMPro.TMP_Text _version;

        private bool _isReady = false;

        private bool _pointCloudEnabled = false;

        public readonly Movement _movement = new Movement();

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private async void Start()
        {
            //_version.text = $"v{Application.version}";

            await _settingsView.Load();

            _loading.text = string.Empty;
            _videoRecorderView.Show();



        }

        private void Update()
        {
            bool isLive = _isReady && !_videoPlayerView.isActiveAndEnabled;

            FrameData frame = isLive ? _videoRecorderView.Frame : _videoPlayerView.Frame;

            if (frame != null)
            {
                Body body = frame.BodyData?.Default();

                if (body != null)
                {
                    _movement.Update(body);
                    _debug.text = _movement.ToString();
                }

                _warnings.Load(frame, body, _movement);
                _angles.Load(body, _movement);
                _viewer.Load(frame);
            }
        }

        /// <summary>
        /// Called when the measurement types have changed.
        /// </summary>
        /// <param name="types">The new <see cref="MeasurementType"/>s to load.</param>
        public void OnMeasurementsChanged(MeasurementType[] types)
        {
            _movement.Reset(types);
        }

        /// <summary>
        /// Called when the recording is ready to start.
        /// </summary>
        /// <param name="ready">Specifies whether the recording is ready to start or failed to initialize the camera.</param>
        public void OnRecordingReady(bool ready)
        {
            _isReady = ready;

            _viewer.gameObject.SetActive(true);
            _loading.text = _isReady ? string.Empty : "Could not start the specified camera. Check your configuration settings.";

        }

        /// <summary>
        /// Called when the recording starts.
        /// </summary>
        public void OnRecordingStarted()
        {
            _measurementSelector.SetEnable(false);
        }

        /// <summary>
        /// Called when the recording has stopeed and completed saving data to disc.
        /// </summary>
        public void OnRecordingCompleted()
        {
            _videoRecorderView.Hide();
            _videoPlayerView.Show();

            _videoPlayerView.Options.Path = _videoRecorderView.VideoPath;
            _videoPlayerView.Play();
        }

        /// <summary>
        /// Called when the playback stops.
        /// </summary>
        public void OnPlaybackStopped()
        {
            _measurementSelector.SetEnable(true);
            _videoRecorderView.Show();
            _videoPlayerView.Hide();

            _videoPlayerView.Stop();
        }

        /// <summary>
        /// Called when the user clicks the settings button.
        /// </summary>
        public void OnSettingsClicked()
        {
            _settingsView.Toggle();
        }

        /// <summary>
        /// Called when the user clicks the export button.
        /// </summary>
        public void OnCSVClicked()
        {
            CSVManager.CreateSaveExport(_videoRecorderView.VideoPath, _movement.MeasurementTypes);
        }

        #region Settings

        /// <summary>
        /// Called when the sensor type changes.
        /// </summary>
        /// <param name="value">The new <see cref="SensorType"/> option.</param>
        public void OnSensorChange(int sensorType)
        {
            _settingsView.Hide();
            _videoRecorderView?.SwitchSensor((SensorType)sensorType);
        }

        /// <summary>
        /// Called when the frame rate changes.
        /// </summary>
        /// <param name="fps">The new frame rate (frames per second).</param>
        public void OnFrameRateChange(int fps)
        {
            _settingsView.Hide();
            _videoRecorderView?.SwitchFrameRate(fps);
        }

        /// <summary>
        /// Called when the smoothing value changes.
        /// </summary>
        /// <param name="value">The new smoothing value.</param>
        public void OnSmoothingChange(float value)
        {
            _videoRecorderView?.SetSmoothing(value);
        }

        /// <summary>
        /// Called when the smoothing type changes.
        /// </summary>
        /// <param name="value">The new smoothing type.</param>
        public void OnSmoothingTypeChange(int value)
        {
            _videoRecorderView?.SetSmoothingType(value);
        }

        /// <summary>
        /// Called when the brightness value changes.
        /// </summary>
        /// <param name="value">The new brightness value (0 to 1).</param>
        public void OnBrightnessChange(float value)
        {
            _videoRecorderView?.SetBrightness(value);
        }

        /// <summary>
        /// Called when the contrast value changes.
        /// </summary>
        /// <param name="value">The new contrast value (-1 to +1).</param>
        public void OnContrastChange(float value)
        {
            _videoRecorderView?.SetContrast(value);
        }

        /// <summary>
        /// Called when the flip value changes.
        /// </summary>
        /// <param name="value">True if the image view is mirrored; false otherwise.</param>
        public void OnFlip(bool value)
        {
            _viewer.Image.FlipHorizontally = value;
        }

        /// <summary>
        /// Called when the bounding box value changes.
        /// </summary>
        /// <param name="value">True if the view displays bounding boxes; false otherwise.</param>
        public void OnBoundingBox(bool value)
        {
            _viewer.SkeletonManager.ShowBoundingBox = value;
        }

        /// <summary>
        /// Called when the point cloud value changes.
        /// </summary>
        /// <param name="value">True if the view displays point cloud; false otherwise.</param>
        public void OnPointCloud(bool value)
        {
            _pointCloudEnabled = value;
        }

        /// <summary>
        /// Called when the show faces value changes.
        /// </summary>
        /// <param name="value">True if the view displays user faces; false otherwise.</param>
        public void OnShowFaces(bool value)
        {
            _viewer.SkeletonManager.HideFaces = !value;
        }

        /// <summary>
        /// Called when the fit image value changes.
        /// </summary>
        /// <param name="value">True if the image view fits the screen; false if it covers all the available area.</param>
        public void OnFitImage(bool value)
        {
            _viewer.Image.AspectMode = value ? AspectRatioFitter.AspectMode.FitInParent : AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        #endregion
    }
}
