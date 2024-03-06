using LightBuzz.BodyTracking;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// The settings view.
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject _loading;
        [SerializeField] private GameObject _scrollView;

        [SerializeField] private Text _labelSmoothing;
        [SerializeField] private Text _labelBrightness;
        [SerializeField] private Text _labelContrast;

        /// <summary>
        /// Raised when the sensor type changes.
        /// </summary>
        public UnityEvent<int> SensorChanged = new UnityEvent<int>();

        /// <summary>
        /// Raised when the frame rate changes.
        /// </summary>
        public UnityEvent<int> FrameRateChanged = new UnityEvent<int>();

        /// <summary>
        /// Raised when the smoothing value changes.
        /// </summary>
        public UnityEvent<float> SmoothingChanged = new UnityEvent<float>();

        /// <summary>
        /// Raised when the smoothing type changes.
        /// </summary>
        public UnityEvent<int> SmoothingTypeChanged = new UnityEvent<int>();

        /// <summary>
        /// Raised when the brightness value changes.
        /// </summary>
        public UnityEvent<float> BrightnessChanged = new UnityEvent<float>();

        /// <summary>
        /// Raised when the contrast value changes.
        /// </summary>
        public UnityEvent<float> ContrastChanged = new UnityEvent<float>();

        /// <summary>
        /// Raised when the flip value changes.
        /// </summary>
        public UnityEvent<bool> FlipChanged = new UnityEvent<bool>();

        /// <summary>
        /// Raised when the bounding box value changes.
        /// </summary>
        public UnityEvent<bool> BoundingBoxChanged = new UnityEvent<bool>();

        /// <summary>
        /// Raised when the point cloud value changes.
        /// </summary>
        public UnityEvent<bool> PointCloudChanged = new UnityEvent<bool>();

        /// <summary>
        /// Raised when the show faces value changes.
        /// </summary>
        public UnityEvent<bool> ShowFacesChanged = new UnityEvent<bool>();

        /// <summary>
        /// Raised when the fit image value changes.
        /// </summary>
        public UnityEvent<bool> FitImageChanged = new UnityEvent<bool>();

        /// <summary>
        /// Counts the available sensors and loads the settings view.
        /// </summary>
        /// <returns></returns>
        public async Task Load()
        {
            int countWebcam = 0;
            int countLiDAR = 0;

            await Task.Run(() =>
            {
                countWebcam = Sensor.Count(SensorType.Webcam);
                countLiDAR = Sensor.Count(SensorType.LiDAR);
            });

            Debug.Log($"Found {countWebcam} RGB cameras.");
            Debug.Log($"Found {countLiDAR} LiDAR sensors.");

            _loading.SetActive(false);
            _scrollView.SetActive(true);
        }

        private void OnDestroy()
        {
            SensorChanged?.RemoveAllListeners();
            SmoothingChanged?.RemoveAllListeners();
            SmoothingTypeChanged?.RemoveAllListeners();
            BrightnessChanged?.RemoveAllListeners();
            ContrastChanged?.RemoveAllListeners();
            FlipChanged?.RemoveAllListeners();
            BoundingBoxChanged?.RemoveAllListeners();
            PointCloudChanged?.RemoveAllListeners();
            ShowFacesChanged?.RemoveAllListeners();
        }

        /// <summary>
        /// Toggles the visibility of the settings view.
        /// </summary>
        public void Toggle()
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
        /// Shows the settings view.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the settings view.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the sensor type changes.
        /// </summary>
        /// <param name="value">0 for the Webcam sensor type, 1 for the LiDAR sensor type.</param>
        public void OnSensorChange(int value)
        {
            SensorType sensorType =
                value == 0 ? SensorType.Webcam : SensorType.LiDAR;

            SensorChanged?.Invoke((int)sensorType);
        }

        /// <summary>
        /// Called when the frame rate changes.
        /// </summary>
        /// <param name="value">0 for 30 FPS, 1 for 60.</param>
        public void OnFrameRateChange(int value)
        {
            int fps = value == 0 ? 60 : 30;

            FrameRateChanged?.Invoke(fps);
        }

        /// <summary>
        /// Called when the smoothing value changes.
        /// </summary>
        /// <param name="value">The new smoothing value.</param>
        public void OnSmoothingChange(float value)
        {
            _labelSmoothing.text = $"{value:N0}%";

            float smoothing = value / 100.0f;

            SmoothingChanged?.Invoke(smoothing);
        }

        /// <summary>
        /// Called when the smoothing type changes.
        /// </summary>
        /// <param name="value">The new smoothing type.</param>
        public void OnSmoothingTypeChange(int value)
        {
            SmoothingType smoothingType =
                value == 0 ? SmoothingType.Dynamic : SmoothingType.Legacy;

            SmoothingTypeChanged?.Invoke((int)smoothingType);
        }

        /// <summary>
        /// Called when the brightness value changes.
        /// </summary>
        /// <param name="value">The new brightness value (0 to 1).</param>
        public void OnBrightnessChange(float value)
        {
            _labelBrightness.text = $"{value:N0}%";

            BrightnessChanged?.Invoke(value / 100.0f);
        }

        /// <summary>
        /// Called when the contrast value changes.
        /// </summary>
        /// <param name="value">The new contrast value (-1 to +1).</param>
        public void OnContrastChange(float value)
        {
            _labelContrast.text = $"{value:N0}%";

            ContrastChanged?.Invoke(value / 100.0f);
        }

        /// <summary>
        /// Called when the flip value changes.
        /// </summary>
        /// <param name="value">True if the image view is mirrored; false otherwise.</param>
        public void OnFlip(bool value)
        {
            FlipChanged?.Invoke(value);
        }

        /// <summary>
        /// Called when the bounding box value changes.
        /// </summary>
        /// <param name="value">True if the view displays bounding boxes; false otherwise.</param>
        public void OnBoundingBox(bool value)
        {
            BoundingBoxChanged?.Invoke(value);
        }

        /// <summary>
        /// Called when the point cloud value changes.
        /// </summary>
        /// <param name="value">True if the view displays point cloud; false otherwise.</param>
        public void OnPointCloud(bool value)
        {
            PointCloudChanged?.Invoke(value);
        }

        /// <summary>
        /// Called when the show faces value changes.
        /// </summary>
        /// <param name="value">True if the view displays user faces; false otherwise.</param>
        public void OnShowFaces(bool value)
        {
            ShowFacesChanged?.Invoke(value);
        }

        /// <summary>
        /// Called when the fit image value changes.
        /// </summary>
        /// <param name="value">True if the image view fits the screen; false if it covers all the available area.</param>
        public void OnFitImage(bool value)
        {
            FitImageChanged?.Invoke(value);
        }
    }
}