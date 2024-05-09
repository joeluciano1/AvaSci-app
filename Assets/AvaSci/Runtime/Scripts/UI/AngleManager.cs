using System;
using System.Collections.Generic;
using System.Linq;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using UnityEngine;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// Manages a list of <see cref="Angle2D"/> visual components.
    /// </summary>
    public class AngleManager : MonoBehaviour
    {
        [SerializeField] private ImageView _imageView;
        [SerializeField] private Angle2D _anglePrefab;
        [SerializeField] private RectTransform contentParentPrefab;

        private readonly Dictionary<MeasurementType, Angle2D> _angles = new Dictionary<MeasurementType, Angle2D>();
        private Dictionary<MeasurementType, RectTransform> contentParents = new Dictionary<MeasurementType, RectTransform>();

        private void Start()
        {
            CreateParents();
        }

        private void CreateParents()
        {
            RectTransform ankleLeft = Instantiate(contentParentPrefab, transform);
            RectTransform ankleRight = Instantiate(contentParentPrefab, transform);
            RectTransform kneeLeft = Instantiate(contentParentPrefab, transform);
            RectTransform kneeRight = Instantiate(contentParentPrefab, transform);
            RectTransform elbowLeft = Instantiate(contentParentPrefab, transform);
            RectTransform elbowRight = Instantiate(contentParentPrefab, transform);
            RectTransform hipLeft = Instantiate(contentParentPrefab, transform);
            RectTransform hipRight = Instantiate(contentParentPrefab, transform);
            RectTransform shoulderLeft = Instantiate(contentParentPrefab, transform);
            RectTransform shoulderRight = Instantiate(contentParentPrefab, transform);
            RectTransform neck = Instantiate(contentParentPrefab, transform);
            RectTransform pelvis = Instantiate(contentParentPrefab, transform);
            RectTransform none = Instantiate(contentParentPrefab, transform);

            ankleLeft.name = "AnkleLeft Angles";
            ankleRight.name = "AnkleRight Angles";
            kneeLeft.name = "KneeLeft Angles";
            kneeRight.name = "KneeRight Angles";
            elbowLeft.name = "ElbowLeft Angles";
            elbowRight.name = "ElbowRight Angles";
            hipLeft.name = "HipLeft Angles";
            hipRight.name = "HipRight Angles";
            shoulderLeft.name = "ShoulderLeft Angles";
            shoulderRight.name = "ShoulderRight Angles";
            neck.name = "Neck Angles";
            pelvis.name = "Pelvis Angles";
            none.name = "None";

            contentParents.Add(MeasurementType.None, none);

            contentParents.Add(MeasurementType.AnkleLeftAbduction, ankleLeft);
            contentParents.Add(MeasurementType.AnkleLeftRotation, ankleLeft);
            contentParents.Add(MeasurementType.AnkleHipLeftDistance, ankleLeft);

            contentParents.Add(MeasurementType.AnkleHipRightDistance, ankleRight);
            contentParents.Add(MeasurementType.AnkleRightAbduction, ankleRight);
            contentParents.Add(MeasurementType.AnkleRightRotation, ankleRight);

            contentParents.Add(MeasurementType.ElbowLeftFlexion, elbowLeft);
            contentParents.Add(MeasurementType.ElbowRightFlexion, elbowRight);

            contentParents.Add(MeasurementType.KneeLeftAbduction, kneeLeft);
            contentParents.Add(MeasurementType.KneeLeftFlexion, kneeLeft);
            contentParents.Add(MeasurementType.HipKneeLeftDistance, kneeLeft);

            contentParents.Add(MeasurementType.KneeRightAbduction, kneeRight);
            contentParents.Add(MeasurementType.KneeRightFlexion, kneeRight);
            contentParents.Add(MeasurementType.HipKneeRightDistance, kneeRight);

            contentParents.Add(MeasurementType.HipLeftAbduction, hipLeft);
            contentParents.Add(MeasurementType.HipLeftFlexion, hipLeft);

            contentParents.Add(MeasurementType.HipRightAbduction, hipRight);
            contentParents.Add(MeasurementType.HipRightFlexion, hipRight);

            contentParents.Add(MeasurementType.NeckRotation, neck);
            contentParents.Add(MeasurementType.NeckLateralFlexion, neck);

            contentParents.Add(MeasurementType.PelvisAngle, pelvis);

            contentParents.Add(MeasurementType.ShoulderLeftAbduction, shoulderLeft);
            contentParents.Add(MeasurementType.ShoulderLeftFlexion, shoulderLeft);
            contentParents.Add(MeasurementType.ShoulderLeftRotation, shoulderLeft);

            contentParents.Add(MeasurementType.ShoulderRightAbduction, shoulderRight);
            contentParents.Add(MeasurementType.ShoulderRightFlexion, shoulderRight);
            contentParents.Add(MeasurementType.ShoulderRightRotation, shoulderRight);
        }

        private void OnDestroy()
        {
            contentParents.ToList().ForEach(y =>
            {
                Destroy(y.Value.gameObject);
            });
            contentParents.Clear();
            Clear();
        }

        /// <summary>
        /// Clears the list of <see cref="Angle2D"/> visual components.
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<MeasurementType, Angle2D> angle in _angles)
            {
                Destroy(angle.Value.gameObject);
            }

            _angles.Clear();
        }

        /// <summary>
        /// Loads the specified <see cref="Movement"/> data to the corresponding <see cref="Angle2D"/> visual components.
        /// </summary>
        /// <param name="body">The <see cref="Body"/> skeleton data.</param>
        /// <param name="movement">A collection of <see cref="Measurement"/> data to load.</param>
        public void Load(Body body, Movement movement)
        {
            if (body == null || movement == null)
            {
                Clear();
                return;
            }

            if (movement.Measurements.Count != _angles.Count)
            {
                // Clear();
            }

            foreach (Measurement measurement in movement.MeasurementValues)
            {
                Load(body, measurement);
            }
        }

        /// <summary>
        /// Loads the specified <see cref="Measurement"/> data to the corresponding <see cref="Angle2D"/> visual component.
        /// </summary>
        /// <param name="body">The <see cref="Body"/> skeleton data.</param>
        /// <param name="measurement">The <see cref="Measurement"/> data to load.</param>
        public void Load(Body body, Measurement measurement)
        {
            MeasurementType type = measurement.Type;
            RectTransform rectParent = contentParents[type];

            if (!_angles.ContainsKey(type))
            {
                Angle2D go = Instantiate(_anglePrefab, rectParent);
                go.ImageView = _imageView;

                _angles.Add(type, go);
            }

            if (body.Joints[measurement.KeyJoint1].TrackingState == TrackingState.Inferred ||
                    body.Joints[measurement.KeyJoint2].TrackingState == TrackingState.Inferred ||
                    body.Joints[measurement.KeyJoint3].TrackingState == TrackingState.Inferred)
            {
                _angles[type].gameObject.SetActive(false);
                return;
            }

            if (!_angles[type].gameObject.activeSelf)
            {
                _angles[type].gameObject.SetActive(true);
            }

            _angles[type].Load(measurement, body);
            Vector2 _center = _imageView.GetPosition(measurement.AngleCenter);
            rectParent.anchoredPosition = new Vector2(_center.x, _center.y);
        }
    }
}