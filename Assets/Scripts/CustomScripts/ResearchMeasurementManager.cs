using System;
using System.Collections.Generic;
using System.Linq;
using LightBuzz.BodyTracking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchMeasurementManager : MonoBehaviour
{
    public static ResearchMeasurementManager instance;
    public Body LightbuzzBody;

    public List<TempBodyDataSaver> tempBodyDataSavers = new List<TempBodyDataSaver>();
    public List<ResearchProjectCompleteBodyData> researchProjectCompleteBodyDatas =
        new List<ResearchProjectCompleteBodyData>();
    public List<ResearchProjectCompleteBodyData> SelectedBodyDatas =
        new List<ResearchProjectCompleteBodyData>();
    public ResearchProjectCompleteBodyData bodyDataPrefab;
    public Transform SkeletonMangerTransform;
    public float selectedJoints;
    public float distance;
    float stepLength;
    float stepAngleR;
    float stepAngleL;
    public float? footOnGroundPosition;
    public Vector3 lastFootPosition;
    public Vector3 newFootPosition;
    public int footCount;

    Vector3 previousFootPostionR;
    Vector3 previousFootPostionL;

    public TMP_Text DistanceNotifier;
    public TMP_Text StepLengthNotifier;
    public TMP_Text StepAngleRNotifier;
    public TMP_Text StepAngleLNotifier;

    public TMP_Text strideLengthNotifier;
    public TMP_Text stepwidthLNotifier;
    public TMP_Text stepwidthRNotifier;
    bool isStarted;
    public float tollorance;
    public Button TriggerButton;
    private void Awake()
    {
        instance = this;
    }

    public void StartReading()
    {
        isStarted = true;
        researchProjectCompleteBodyDatas.ForEach(x => x.gameObject.SetActive(true));
    }

    public void StopReading()
    {
        isStarted = false;
        researchProjectCompleteBodyDatas.ForEach(x => x.gameObject.SetActive(false));
    }

    public void LateUpdate()
    {
        if (!isStarted)
        {
            return;
        }
        if (LightbuzzBody != null)
        {
            foreach (var item in LightbuzzBody.Joints)
            {

                if (item.Value.TrackingState != TrackingState.Inferred)
                {
                    TempBodyDataSaver tempBodyDataSaver = new TempBodyDataSaver()
                    {
                        bodyJointType = item.Value.Type,
                        bodyJoint = item.Value,
                        Position2D = item.Value.Position2D,
                        Position3D = item.Value.Position3D,
                        trackingState = item.Value.TrackingState,
                        PointPosition = SkeletonMangerTransform.Find("Skeleton(Clone)")?.Find("Points")?.Find(item.Value.Type.ToString()) == null ? null : SkeletonMangerTransform.Find("Skeleton(Clone)").Find("Points").Find(item.Value.Type.ToString()).transform
                    };

                    var alreadyAdded = tempBodyDataSavers.FirstOrDefault(x =>
                        x.bodyJointType == item.Value.Type
                    );
                    if (alreadyAdded != null)
                    {
                        if (tempBodyDataSaver.PointPosition == null)
                        {
                            tempBodyDataSavers.Remove(alreadyAdded);
                        }
                        else
                        {
                            tempBodyDataSavers[tempBodyDataSavers.IndexOf(alreadyAdded)] =
                                tempBodyDataSaver;
                        }
                    }
                    else
                    {
                        if (tempBodyDataSaver.PointPosition != null)
                            tempBodyDataSavers.Add(tempBodyDataSaver);
                    }
                }
                else
                {
                    var alreadyAdded = tempBodyDataSavers.FirstOrDefault(x =>
                        x.bodyJointType == item.Value.Type
                    );
                    if (alreadyAdded != null)
                        tempBodyDataSavers.Remove(alreadyAdded);
                }
            }

            foreach (var item in tempBodyDataSavers)
            {
                var alreadyAdded = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                    x.name == item.bodyJointType.ToString()
                );
                if (alreadyAdded == null)
                {
                    ResearchProjectCompleteBodyData researchProjectCompleteBodyData = Instantiate(
                        bodyDataPrefab,
                        bodyDataPrefab.transform.parent
                    );
                    researchProjectCompleteBodyData.gameObject.SetActive(true);
                    researchProjectCompleteBodyData.gameObject.name = item.bodyJointType.ToString();
                    researchProjectCompleteBodyData.PositionShowCase.text =
                        $"{item.bodyJointType}\n{Mathf.Round(item.Position3D.x * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.y * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.z * 100.0f) * 0.01f}";
                    researchProjectCompleteBodyData.transform.position =
                        item.PointPosition.position;
                    researchProjectCompleteBodyData.Position3D = item.Position3D;
                    researchProjectCompleteBodyData.Position2D = item.Position2D;
                    researchProjectCompleteBodyDatas.Add(researchProjectCompleteBodyData);
                }
                else
                {
                    alreadyAdded.PositionShowCase.text =
                        $"{item.bodyJointType}\n{Mathf.Round(item.Position3D.x * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.y * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.z * 100.0f) * 0.01f}";
                    alreadyAdded.transform.position = item.PointPosition.position;
                    alreadyAdded.Position3D = item.Position3D;
                    alreadyAdded.Position2D = item.Position2D;
                    alreadyAdded.gameObject.SetActive(true);
                }
            }
            List<ResearchProjectCompleteBodyData> ObjectsToDestroy =
                new List<ResearchProjectCompleteBodyData>();
            foreach (var item in researchProjectCompleteBodyDatas)
            {
                if (
                    !tempBodyDataSavers.Any(x => x.bodyJointType.ToString() == item.gameObject.name)
                )
                {
                    ObjectsToDestroy.Add(item);
                }
            }

            ObjectsToDestroy.ForEach(x =>
            {
                x.gameObject.SetActive(false);
            });
            ObjectsToDestroy.Clear();
        }
        else
        {
            Debug.Log("is Nulls");
        }
        if (selectedJoints == 2)
        {
            distance = Vector3.Distance(
                SelectedBodyDatas[0].Position3D,
                SelectedBodyDatas[1].Position3D
            );
            distance = Mathf.Round(distance * 100.0f) * 0.01f;
            SelectedBodyDatas.ForEach(x => x.ShowMyChilds());
            // DistanceNotifier.transform.parent.GetComponent<RectTransform>().anchoredPosition = (SelectedBodyDatas[0].GetComponent<RectTransform>().anchoredPosition + SelectedBodyDatas[1].GetComponent<RectTransform>().anchoredPosition) / 2;
            DistanceNotifier.text = "Distance: " + distance.ToString() + " meters";
        }
        else
        {
            DistanceNotifier.text = "";
        }

        var ankleStepLengthJoints = researchProjectCompleteBodyDatas
            .Where(x =>
                x.gameObject.name == JointType.AnkleLeft.ToString()
                || x.gameObject.name == JointType.AnkleRight.ToString()
            )
            .ToList();
        if (ankleStepLengthJoints.Count == 2)
        {
            stepLength = Vector3.Distance(
                ankleStepLengthJoints[0].Position3D,
                ankleStepLengthJoints[1].Position3D
            );
            StepLengthNotifier.text = $"Step Length\n{stepLength}m";
        }
        else
        {
            StepLengthNotifier.text = $"Step Length\nNo Joint";
        }
        var jointsForStepAngleR = researchProjectCompleteBodyDatas
            .Where(x =>
                x.gameObject.name == JointType.AnkleRight.ToString()
                || x.gameObject.name == JointType.Pelvis.ToString()
            )
            .ToList();
        if (jointsForStepAngleR.Count == 2)
        {
            Vector3 pointA = new Vector3(0, 0, 0);
            Vector3 pointB = new Vector3(0, 0, 0);
            var pelvis = jointsForStepAngleR.FirstOrDefault(x =>
                x.gameObject.name == JointType.Pelvis.ToString()
            );
            var ankle = jointsForStepAngleR.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleRight.ToString()
            );

            pointA = new Vector3(pelvis.Position3D.x, ankle.Position3D.y, pelvis.Position3D.z);
            pointB = ankle.Position3D;

            stepAngleR = Vector3.Angle(pointA, pointB);
            StepAngleRNotifier.text = $"Step AngleR\n{stepAngleR}°";
        }
        else
        {
            StepAngleRNotifier.text = $"Step AngleR\nNo Joint";
        }

        var jointsForStepAngleL = researchProjectCompleteBodyDatas
            .Where(x =>
                x.gameObject.name == JointType.AnkleLeft.ToString()
                || x.gameObject.name == JointType.Pelvis.ToString()
            )
            .ToList();
        if (jointsForStepAngleR.Count == 2)
        {
            Vector3 pointA = new Vector3(0, 0, 0);
            Vector3 pointB = new Vector3(0, 0, 0);
            var pelvis = jointsForStepAngleL.FirstOrDefault(x =>
                x.gameObject.name == JointType.Pelvis.ToString()
            );
            var ankle = jointsForStepAngleL.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleLeft.ToString()
            );
            if (pelvis != null && ankle != null)
            {
                pointA = new Vector3(pelvis.Position3D.x, ankle.Position3D.y, pelvis.Position3D.z);
                pointB = ankle.Position3D;

                stepAngleL = Vector3.Angle(pointA, pointB);
                StepAngleLNotifier.text = $"Step AngleL\n{stepAngleL}°";
            }
        }
        else
        {
            StepAngleLNotifier.text = $"Step AngleL\nNo Joint";
        }
        //SetInitialFootPlace();
    }


    public void ClearFootOnGroundPosition()
    {
        footOnGroundPosition = null;
    }
    public void SetInitialFootPlace()
    {

        var jointForStrideLengthL = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
            x.gameObject.name == JointType.AnkleLeft.ToString()
        );

        if (jointForStrideLengthL == null)
        {
            Debug.Log("Not Detected");
            return;
        }
        //Debug.Log($"\n3D Position {jointForStrideLengthL.Position3D}\n2D Position {jointForStrideLengthL.Position2D}");

        Debug.Log("Detected");

        footOnGroundPosition = jointForStrideLengthL.MyTransform.anchoredPosition.y;

        if (footCount == 0)
        {
            lastFootPosition = jointForStrideLengthL.Position3D;
            TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Measure StrideLength";
            footCount += 1;
        }
        else if (footCount > 0)
        {
            newFootPosition = jointForStrideLengthL.Position3D;
            float distance = Vector3.Distance(newFootPosition, lastFootPosition);
            distance = Mathf.Round(distance * 100.0f) * 0.01f;
            strideLengthNotifier.text = $"Stride Length\n{distance}m";
            TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Set Initial Foot Position";
            footCount = 0;
        }


    }

    public void CalculateStepWidthL()
    {
        var ankleLeft = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
            x.gameObject.name == JointType.AnkleLeft.ToString()
        );
        var pelvis = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
            x.gameObject.name == JointType.Pelvis.ToString()
        );
        if (ankleLeft == null || pelvis == null)
        {
            return;
        }
        float stepWidth = Math.Abs(ankleLeft.Position3D.x - pelvis.Position3D.x);
        stepwidthLNotifier.text = $"Step Width L:\n{stepWidth}";
    }

    public void CalculateStepWidthR()
    {
        var ankleRight = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
            x.gameObject.name == JointType.AnkleRight.ToString()
        );
        var pelvis = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
            x.gameObject.name == JointType.Pelvis.ToString()
        );
        if (ankleRight == null || pelvis == null)
        {
            return;
        }
        float stepWidth = Math.Abs(ankleRight.Position3D.x - pelvis.Position3D.x);
        stepwidthRNotifier.text = $"Step Width L:\n{stepWidth}";
    }
}

[System.Serializable]
public class TempBodyDataSaver
{
    public JointType bodyJointType;
    public TrackingState trackingState;
    public LightBuzz.BodyTracking.Joint bodyJoint;

    public Vector2 Position2D;
    public Vector3 Position3D;
    public Transform PointPosition;
}
