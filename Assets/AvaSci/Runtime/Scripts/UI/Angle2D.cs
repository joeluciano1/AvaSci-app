using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;
using UnityEngine.UI;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// Represents a 2D angle arc visual component.
    /// </summary>
    public class Angle2D : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private ImageView _imageView;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _foregroundImage;
        [SerializeField] private Text _text;

        [Header("Arc components")]
        [SerializeField]
        [Range(0f, 360f)]
        private float _angle;

        [SerializeField] private Vector2 _start;
        [SerializeField] private Vector2 _center;
        [SerializeField] private Vector2 _end;

        [Header("Arc properties")]
        [SerializeField] private string _displayMessage;

        private RectTransform _rect;

        /// <summary>
        /// The scaled image view.
        /// This member is used for scaling the angle arc according to the image view's scale.
        /// </summary>
        public ImageView ImageView
        {
            get => _imageView;
            set => _imageView = value;
        }

        /// <summary>
        /// The angle value.
        /// </summary>
        public float Angle
        {
            get => _angle;
            set => _angle = value;
        }

        /// <summary>
        /// The starting point of the arc.
        /// </summary>
        public Vector3 Start
        {
            get => _start;
            set => _start = value;
        }

        /// <summary>
        /// The middle (center) point of the arc.
        /// </summary>
        public Vector3 Center
        {
            get => _center;
            set => _center = value;
        }

        /// <summary>
        /// The end point of the arc.
        /// </summary>
        public Vector3 End
        {
            get => _end;
            set => _end = value;
        }

        /// <summary>
        /// The text to display.
        /// </summary>
        public string DisplayMessage
        {
            get => _displayMessage;
            set => _displayMessage = value;
        }

        private void OnValidate()
        {
            //Load();
        }

        /// <summary>
        /// Refreshes the angle arc data.
        /// </summary>
        public void Refresh()
        {
            if (_rect == null) _rect = gameObject.transform as RectTransform;

            float uiAngle = Vector2.Angle(_center - _start, _center - _end);

            Vector2 direction = _center - _start;
            float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            float startingRot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var stickmanStartCenter = _start - _center;
            var stickmanEndCenter = _end - _center;
            float stickmanAngle = Vector3.Angle(stickmanStartCenter, stickmanEndCenter);
            float sign = Mathf.Sign(stickmanStartCenter.x * stickmanEndCenter.y - stickmanStartCenter.y * stickmanEndCenter.x);

            //if (CanBeReflexAngle)
            //{
            //    if (_isInternalAngle)
            //    {
            //        startingRot -= 90f;
            //        if (sign > 0)
            //            stickmanAngle = (180f - stickmanAngle) + 180f;
            //    }
            //    else if (!_isInternalAngle)
            //    {
            //        if (sign < 0)
            //            stickmanAngle = (180f - stickmanAngle) + 180f;

            //        startingRot += stickmanAngle - 90f;
            //    }
            //}
            //else
            //{
            //    if (sign < 0f)
            //        startingRot -= 90f;
            //    else
            //        startingRot += stickmanAngle - 90f;
            //}

            if (sign < 0f)
                startingRot -= 90f;
            else
                startingRot += stickmanAngle - 90f;

            if (!_foregroundImage.fillClockwise)
            {
                startingRot -= stickmanAngle;
            }

            var newRotation = UnityEngine.Quaternion.Euler(0f, 0f, startingRot);

            _foregroundImage.transform.localRotation = newRotation;
            _foregroundImage.fillAmount = uiAngle / 360.0f;

            _text.text = _displayMessage;

            _rect.anchoredPosition = new Vector2(_center.x, _center.y);
        }

        /// <summary>
        /// Updates the angle arc visual components according to the specified measurement data.
        /// </summary>
        /// <param name="measurement">The measurement to display.</param>
        public void Load(Measurement measurement, Body body = null)
        {
            if (measurement == null) return;

            _start = _imageView.GetPosition(measurement.AngleStart);
            _center = _imageView.GetPosition(measurement.AngleCenter);
            _end = _imageView.GetPosition(measurement.AngleEnd);

            _angle = measurement.Value;
            _displayMessage = $"{measurement.Value:N0}°";

            Refresh();
        }
    }
}