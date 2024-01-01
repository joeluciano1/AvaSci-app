using LightBuzz.BodyTracking;
using LightBuzz.BodyTracking.Video;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// User interface for a video recording view.
    /// </summary>
    public class VideoRecordingView : MonoBehaviour
    {
        [Header("Sensor configuration")]

        [SerializeField] private DeviceConfiguration _configuration;

        [Header("Video recording options")]

        [SerializeField] private string _videoPath = string.Empty;
        [SerializeField] private VideoRecordingMode _mode = VideoRecordingMode.Default;
        [SerializeField][Range(0, 100)] private int _quality = 50;
        [SerializeField] private bool _recordColorData = true;
        [SerializeField] private bool _recordDepthData = false;
        [SerializeField] private bool _recordBodyData = true;
        [SerializeField] private bool _hideFaces = false;

        [Header("User interface")]

        [SerializeField] private RecordButton _recordButton;
        [SerializeField] private SwitchButton _settingsButton;
        [SerializeField] private SwitchButton _switchCameraButton;
        [SerializeField] private GameObject _loading;

        private readonly VideoRecorder _recorder = new VideoRecorder();

        private bool _started = false;
        private bool _stopped = false;
        private bool _canceled = false;
        private bool _completed = false;

        private float _progress = 0.0f;

        private bool _isSwitching = false;

        private FrameData _frame;
        private DateTime _previous = DateTime.MinValue;

        public Sensor Sensor { get; private set; }

        public FrameData Frame
        {
            get
            {
                if (_frame == null) return null;

                if (_frame.Timestamp > _previous)
                {
                    _previous = _frame.Timestamp;

                    return _frame;
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the absolute path of the LightBuzz video folder.
        /// </summary>
        public string VideoPath => _videoPath;

        /// <summary>
        /// Returns the width of the camera frame.
        /// </summary>
        public int Width => Sensor != null ? Sensor.Width : 0;

        /// <summary>
        /// Returns the height of the camera frame.
        /// </summary>
        public int Height => Sensor != null ? Sensor.Height : 0;

        /// <summary>
        /// Returns the color format of the camera frame.
        /// </summary>
        public ColorFormat Format => Sensor != null ? Sensor.ColorFormat : ColorFormat.RGB;

        /// <summary>
        /// Returns the <see cref="VideoRecorder"/> instance.
        /// </summary>
        public VideoRecorder VideoRecorder => _recorder;

        /// <summary>
        /// Raised when the sensor has opened and recording is ready to start.
        /// </summary>
        public UnityEvent<bool> OnRecordingReady = new UnityEvent<bool>();

        /// <summary>
        /// Raised when the recording has started.
        /// </summary>
        public UnityEvent OnRecordingStarted = new UnityEvent();

        /// <summary>
        /// Raised when the recording has stopped.
        /// </summary>
        public UnityEvent OnRecordingStopped = new UnityEvent();

        /// <summary>
        /// Raised when the recording has been canceled.
        /// </summary>
        public UnityEvent OnRecordingCanceled = new UnityEvent();

        /// <summary>
        /// Raised when the recording has completed saving data.
        /// </summary>
        public UnityEvent OnRecordingCompleted = new UnityEvent();

        private async void OnEnable()
        {
            _recorder.OnRecordingStarted += Recorder_OnRecordingStarted;
            _recorder.OnRecordingStopped += Recorder_OnRecordingStopped;
            _recorder.OnRecordingCanceled += Recorder_OnRecordingCanceled;
            _recorder.OnRecordingCompleted += Recorder_OnRecordingCompleted;
            _recorder.OnProgressUpdated += Recorder_OnProgressUpdated;

            if (Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                await Open();
            }
            else
            {
                PermissionCallbacks callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += async (args) =>
                {
                    await Open();
                };
                callbacks.PermissionDenied += (args) =>
                {
                    Debug.LogError("The application does not have permission to open the camera.");
                };
                callbacks.PermissionDeniedAndDontAskAgain += (args) =>
                {
                    Debug.LogError("Open Settings and grant Camera permissions to this app.");
                };
                Permission.RequestUserPermission(Permission.Camera, callbacks);
            }
        }

        private async void OnDisable()
        {
            OnRecordingReady?.RemoveAllListeners();
            OnRecordingStarted?.RemoveAllListeners();
            OnRecordingStopped?.RemoveAllListeners();
            OnRecordingCompleted?.RemoveAllListeners();

            _recorder.OnRecordingStarted -= Recorder_OnRecordingStarted;
            _recorder.OnRecordingStopped -= Recorder_OnRecordingStopped;
            _recorder.OnRecordingCanceled -= Recorder_OnRecordingCanceled;
            _recorder.OnRecordingCompleted -= Recorder_OnRecordingCompleted;
            _recorder.OnProgressUpdated -= Recorder_OnProgressUpdated;
            _recorder.Dispose();

            await Close();
        }

        private async void OnDestroy()
        {
            await Close();
        }

        private async void OnApplicationFocus(bool focus)
        {
            if (!Application.isMobilePlatform) return;

            if (focus) await Open();
            else await Close();
        }

        private void Update()
        {
            if (Sensor != null)
            {
                if (!Sensor.EnableSteadyRate)
                {
                    _frame = Sensor.Update();
                }
            }

            if (_progress > 0.0f)
            {
                _recordButton.SetProgress(_progress);
                _progress = 0.0f;
            }

            if (_started)
            {
                _started = false;

                OnRecordingStarted?.Invoke();
            }

            if (_stopped)
            {
                _stopped = false;

                OnRecordingStopped?.Invoke();
            }

            if (_canceled)
            {
                _recordButton.SetState(RecordingState.Default);
                _loading.SetActive(false);
                _canceled = false;
                _progress = 0.0f;

                OnRecordingCanceled?.Invoke();
            }

            if (_completed)
            {
                _recordButton.SetState(RecordingState.Default);
                _loading.SetActive(false);
                _completed = false;

                OnRecordingCompleted?.Invoke();
            }
        }

        private void Sensor_OnFrameDataArrived(object sender, FrameData frame)
        {
            if (Sensor.EnableSteadyRate)
            {
                _frame = frame;
            }

            if (_recorder.IsRecording && _recorder.Settings.Mode == VideoRecordingMode.Default)
            {
                _recorder.Update(frame);
            }
        }

        private void Recorder_OnRecordingStarted()
        {
            Debug.Log($"Recording started at: {_recorder.Settings.Path}");

            _started = true;
        }

        private void Recorder_OnRecordingStopped()
        {
            Debug.Log("Recording stopped.");

            _stopped = true;
        }

        private void Recorder_OnRecordingCanceled()
        {
            Debug.Log("Recording canceled.");

            _canceled = true;
        }

        private void Recorder_OnRecordingCompleted()
        {
            Debug.Log("Recording completed saving data.");

            _completed = true;
        }

        private void Recorder_OnProgressUpdated(float percentage)
        {
            _progress = percentage;
        }

        /// <summary>
        /// Opens the sensor asynchronously.
        /// </summary>
        private async Task Open()
        {
            _loading.SetActive(true);

            _recordButton.interactable = false;
            _settingsButton.interactable = false;
            _switchCameraButton.interactable = false;

            Sensor = Sensor.Create(_configuration);

            if (Sensor == null)
            {
                _loading.SetActive(false);

                Debug.LogError("Could not create sensor. Check the configuration settings.");
                OnRecordingReady?.Invoke(false);

                return;
            }

            await Task.Run(() =>
            {
                Sensor.Open();
            });

            _loading.SetActive(false);

            if (!Sensor.IsOpen)
            {
                Debug.LogError("Could not open sensor. Check the configuration settings.");

                _settingsButton.interactable = true;

                OnRecordingReady?.Invoke(false);

                return;
            }

            Debug.Log($"Opened {_configuration.SensorType} {_configuration.DeviceIndex}: {Sensor.Width}x{Sensor.Height} @ {Sensor.FPS} fps");

            _recordButton.interactable = true;
            _settingsButton.interactable = true;
            _switchCameraButton.interactable = true;

            OnRecordingReady?.Invoke(true);

            Sensor.FrameDataArrived += Sensor_OnFrameDataArrived;
        }

        /// <summary>
        /// Closes the sensor asynchronously.
        /// </summary>
        private async Task Close()
        {
            if (Sensor == null) return;

            await Task.Run(() =>
            {
                Sensor.FrameDataArrived -= Sensor_OnFrameDataArrived;
                Sensor.Close();
                Sensor.Dispose();
            });
        }

        /// <summary>
        /// Starts the recording.
        /// </summary>
        public void OnStart()
        {
            if (string.IsNullOrWhiteSpace(_videoPath))
            {
                _videoPath = System.IO.Path.Combine(Application.persistentDataPath, "Video");
            }

            _recordButton.SetState(RecordingState.Recording);
            _recorder.Settings = new VideoRecordingSettings
            {
                Path = _videoPath,
                Mode = _mode,
                Smoothing = Sensor.Configuration.Smoothing,
                FrameRate = Sensor.FPS,
                ColorResolution = new Size(Sensor.Width, Sensor.Height),
                DepthResolution = new Size(Sensor.Width, Sensor.Height),
                ColorIntrinsics = Sensor.ColorIntrinsics,
                DepthIntrinsics = Sensor.DepthIntrinsics,
                ColorFormat = Sensor.ColorFormat,
                RecordColor = _recordColorData,
                RecordDepth = _recordDepthData,
                RecordBody = _recordBodyData,
                Quality = _quality,
                HideFaces = _hideFaces
            };
            _recorder.SmoothingType = Sensor.SmoothingType;
            _recorder.Start();
        }

        /// <summary>
        /// Stops the recording.
        /// </summary>
        public void OnStop()
        {
            _recordButton.SetState(RecordingState.Saving);
            _loading.SetActive(true);

            _recorder.Stop();
        }

        /// <summary>
        /// Record button handler.
        /// </summary>
        public void OnRecord_Click()
        {
            if (!_recorder.IsRecording)
            {
                OnStart();
                ReferenceManager.instance.ClearAllGraphs();
            }
            else
            {
                OnStop();
            }
        }

        /// <summary>
        /// Cancel button handler.
        /// </summary>
        public void OnCancel_Click()
        {
            _recorder.Cancel();
        }

        /// <summary>
        /// Switches to the specified sensor type.
        /// </summary>
        /// <param name="type">The <see cref="SensorType"/> to switch to.</param>
        public async void SwitchSensor(SensorType type)
        {
            if (_isSwitching) return;

            if (_configuration.SensorType == type)
            {
                OnSwitch_Click();
            }
            else
            {
                _isSwitching = true;
                _switchCameraButton.Play();

                _configuration.SensorType = type;
                _configuration.DeviceIndex = 0;

                await Close();
                await Open();

                _switchCameraButton.Stop();
                _isSwitching = false;
            }
        }

        /// <summary>
        /// Switches to the specified frame rate.
        /// </summary>
        /// <param name="fps">The new frame rate value.</param>
        public async void SwitchFrameRate(int fps)
        {
            if (_isSwitching) return;

            if (_configuration.RequestedFrameRate != fps)
            {
                _isSwitching = true;
                _switchCameraButton.Play();

                _configuration.RequestedFrameRate = fps;

                await Close();
                await Open();

                _switchCameraButton.Stop();
                _isSwitching = false;
            }
        }

        /// <summary>
        /// Switch button handler.
        /// </summary>
        public async void OnSwitch_Click()
        {
            _isSwitching = true;
            _switchCameraButton.Play();

            int count = Sensor.Count(_configuration.SensorType);

            if (count > 0)
            {
                int current = _configuration.DeviceIndex;
                int next = current + 1;

                if (next >= count) next = 0;

                if (next != current)
                {
                    _configuration.DeviceIndex = next;

                    await Close();
                    await Open();
                }
            }

            _switchCameraButton.Stop();
            _isSwitching = false;
        }

        /// <summary>
        /// Shows the current view.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the current view.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows or hides the current view.
        /// </summary>
        public void Toogle()
        {
            if (gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Sets the sensor smoothing value.
        /// </summary>
        /// <param name="value">The new smoothing value (0 to 1).</param>
        public void SetSmoothing(float value)
        {
            if (Sensor == null) return;

            Sensor.Smoothing = value;
        }

        /// <summary>
        /// Sets the sensor smoothing type.
        /// </summary>
        /// <param name="value">The new smoothing type (Dynamic or Legacy).</param>
        public void SetSmoothingType(int value)
        {
            if (Sensor == null) return;

            Sensor.SmoothingType = (SmoothingType)value;
        }

        /// <summary>
        /// Sets the sensor brightness value.
        /// </summary>
        /// <param name="value">The new brightness value (0 to 1).</param>
        public void SetBrightness(float value)
        {
            if (Sensor == null) return;

            Sensor.Brightness = value;
        }

        /// <summary>
        /// Sets the sensor contrast value.
        /// </summary>
        /// <param name="value">The new contrast value (-1 to +1).</param>
        public void SetContrast(float value)
        {
            if (Sensor == null) return;

            Sensor.Contrast = value;
        }
    }
}