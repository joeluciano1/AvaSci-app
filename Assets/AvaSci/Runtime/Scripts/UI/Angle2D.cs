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
		[SerializeField]
		private ImageView _imageView;

		[SerializeField]
		private Image _backgroundImage;

		[SerializeField]
		private Image _foregroundImage;

		[SerializeField]
		private Text _text;

		[Header("Arc components")]
		[SerializeField]
		[Range(0f, 360f)]
		private float _angle;

		[SerializeField]
		private Vector2 _start;

		[SerializeField]
		private Vector2 _center;

		[SerializeField]
		private Vector2 _end;

		[Header("Arc properties")]
		[SerializeField]
		private string _displayMessage;
		Dictionary<string, string> Abbriviations = new Dictionary<string, string>();

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

		private void AddToReferenceManager()
		{
			if (!ReferenceManager.instance.AnglesAdded.Contains(this))
				ReferenceManager.instance.AnglesAdded.Add(this);
		}

		private void OnDestroy()
		{
			Debug.Log("destroyed " + gameObject.name);
			if (ReferenceManager.instance.AnglesAdded.Contains(this))
				ReferenceManager.instance.AnglesAdded.Remove(this);
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
				if (item.Contains("Difference"))
				{
					jName = values[0] + "/" + values[1] + " Abd";
				}
				jType = values.Last().Substring(0, 3);
				string abbs = $"{direction} {jName} {jType}";
				Abbriviations.Add(item, abbs);
			}
		}

		private static string SplitAccordingToCapital(string item)
		{
			var r = new Regex(
				@"
				(?<=[A-Z])(?=[A-Z][a-z]) |
				 (?<=[^A-Z])(?=[A-Z]) |
				 (?<=[A-Za-z])(?=[^A-Za-z])",
				RegexOptions.IgnorePatternWhitespace
			);
			string splitted = r.Replace(item, " ");
			return splitted;
		}

		/// <summary>
		/// Refreshes the angle arc data.
		/// </summary>
		public void Refresh()
		{
			if (_rect == null)
				_rect = gameObject.transform as RectTransform;

			float uiAngle = Vector2.Angle(_center - _start, _center - _end);

			Vector2 direction = _center - _start;
			float directionAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			float startingRot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

			var stickmanStartCenter = _start - _center;
			var stickmanEndCenter = _end - _center;
			float stickmanAngle = Vector3.Angle(stickmanStartCenter, stickmanEndCenter);
			float sign = Mathf.Sign(
				stickmanStartCenter.x * stickmanEndCenter.y
					- stickmanStartCenter.y * stickmanEndCenter.x
			);

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
		///
		public float initialDifference;
		public float MinScale = 0.3f;
		public float MaxScale = 1f;

		public void Load(Measurement measurement, Body body = null)
		{
			if (measurement == null)
				return;

			gameObject.name = measurement.Type.ToString();

			_start = _imageView.GetPosition(measurement.AngleStart);
			_center = _imageView.GetPosition(measurement.AngleCenter);
			_end = _imageView.GetPosition(measurement.AngleEnd);
			initialDifference = GetComponent<RectTransform>().sizeDelta.x;
			float scale =
				MinScale
				+ (MaxScale - MinScale)
					* (1 - (body.Joints[JointType.Neck].Position3D.Z - 1) / (3 - 1));
			scale = Math.Clamp(scale, 0.5f, 1f);
			transform.localScale = new Vector3(scale, scale, scale);
			_angle = measurement.Value;

			string name = Abbriviations[Enum.GetName(typeof(MeasurementType), measurement.Type)];

			_displayMessage = $"{measurement.Value:N0}° \n<size=15>{name}</size>";
			if (measurement.Type.ToString().Contains("Difference"))
			{
				_foregroundImage.color = UnityEngine.Color.red;
			}

			if (measurement.Type == MeasurementType.KneeLeftAbduction)
			{
				if (_angle >= 10)
				{
					Vector3D positionOfKnee = body.Joints[JointType.KneeLeft].Position3D;
					Vector3D positionOfHip = body.Joints[JointType.HipLeft].Position3D;

					if (positionOfKnee.X > positionOfHip.X)
					{
						//valgus condition
						_displayMessage += "\nValgus Condition";
					}
					if (positionOfKnee.X < positionOfHip.X)
					{
						//varus condition
						_displayMessage += "\nVarus Condition";
					}
				}
			}
			if (measurement.Type == MeasurementType.KneeRightAbduction)
			{
				Vector3D positionOfKnee = body.Joints[JointType.KneeRight].Position3D;
				Vector3D positionOfHip = body.Joints[JointType.KneeRight].Position3D;

				if (positionOfKnee.X > positionOfHip.X)
				{
					//valgus condition
					_displayMessage += "\nValgus Condition";
				}
				if (positionOfKnee.X < positionOfHip.X)
				{
					//varus condition
					_displayMessage += "\nVarus Condition";
				}
			}
			
			Refresh();
		}
		
	}
}
