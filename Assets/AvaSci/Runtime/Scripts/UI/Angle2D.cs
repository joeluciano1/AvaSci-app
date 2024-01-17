using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public bool leftAdded;
        public bool rightAdded;

        [Header("Arc components")]
        [SerializeField]
        [Range(0f, 360f)]
        private float _angle;

        [SerializeField] private Vector2 _start;
        [SerializeField] public Vector2 _center;
        [SerializeField] private Vector2 _end;

        [Header("Arc properties")]
        [SerializeField] private string _displayMessage;
        Dictionary<string, string> Abbriviations = new Dictionary<string, string>();
        private RectTransform _rect;
        public Vector2 positionOffset;
        public float MinScale;
        public float MaxScale;
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
        private void Awake()
        {

            foreach (var item in Enum.GetNames(typeof(MeasurementType)))
            {
                string splitted = SplitAccordingToCapital(item);
                string[] values = splitted.Split(" ");
                string direction = "";
                string jName = "";
                string jType = "";

                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == "Left" || values[i] == "Right")
                    {
                        direction = values[i][0].ToString();
                    }


                }
                jName = values[0].Substring(0, 3);
                jType = values.Last().Substring(0, 3);
                string abbs = $"{direction} {jName} {jType}";
                Abbriviations.Add(item, abbs);
            }

        }

        private static string SplitAccordingToCapital(string item)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
            string splitted = r.Replace(item, " ");
            return splitted;
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

        public float initialDifference;
        /// <summary>
        /// Updates the angle arc visual components according to the specified measurement data.
        /// </summary>
        /// <param name="measurement">The measurement to display.</param>
        public void Load(Body body, Measurement measurement)
        {
            if (measurement == null) return;

            _start = _imageView.GetPosition(measurement.AngleStart);
            _center = _imageView.GetPosition(measurement.AngleCenter);

            foreach (var item in ReferenceManager.instance.AnglesAdded)
            {
                initialDifference = GetComponent<RectTransform>().sizeDelta.x;
                float scale = MinScale + (MaxScale - MinScale) * (1 - (body.Joints[JointType.Neck].Position3D.Z - 1) / (3 - 1));
                transform.localScale = new Vector3(scale, scale, scale);
                positionOffset.x = initialDifference - (scale * 100);
                if (item == this || rightAdded || leftAdded)
                {
                    continue;
                }
                if (Math.Abs(Vector2.Distance(item._center, _center)) < 2)
                {
                    item.initialDifference = item.positionOffset.x;
                    leftAdded = true;
                    GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                    item.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                    item.rightAdded = true;
                }
            }
            if (leftAdded)
            {
                _center.x += positionOffset.x / 2;
            }
            else if (rightAdded)
            {
                _center.x -= positionOffset.x / 2;
            }


            _end = _imageView.GetPosition(measurement.AngleEnd);

            _angle = measurement.Value;
            string name = Abbriviations[Enum.GetName(typeof(MeasurementType), measurement.Type)];
            _displayMessage = $"{measurement.Value:N0}° \n<size=15>{name}</size>";



            Refresh();
        }

        public void OnEnable()
        {
            if (!ReferenceManager.instance.AnglesAdded.Contains(this))
            {
                ReferenceManager.instance.AnglesAdded.Add(this);
            }
        }

        public void OnDisable()
        {
            if (ReferenceManager.instance.AnglesAdded.Contains(this))
            {
                ReferenceManager.instance.AnglesAdded.Remove(this);
            }
        }
        public void OnDestroy()
        {
            if (ReferenceManager.instance.AnglesAdded.Contains(this))
            {
                ReferenceManager.instance.AnglesAdded.Remove(this);
            }
        }
    }
}