using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
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
    public List<float> footStrikeAtTimes = new List<float>();
    public UserConsentPanel userConsentPanel;

    [HideInInspector]
    public bool leftLeg;

    [HideInInspector]
    public bool rightLeg;
    public Button StepButon;
    public float debugDistance;
    public TMP_InputField toloranceValue;
    private void Awake()
    {
        instance = this;
    }

    public void StartReading()
    {
        isStarted = true;
        ReferenceManager.instance.PopupManager.Show("Information", "Please run the video completely once again to complete the foot detection record. Thanks!");
        researchProjectCompleteBodyDatas.ForEach(x => x.gameObject.SetActive(true));
        ReferenceManager.instance.StartTimer();
        TakeUserConsent();
    }

    public void StopReading()
    {
        ReferenceManager.instance.StopTimer();
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
                        PointPosition =
                            SkeletonMangerTransform
                                .Find("Skeleton(Clone)")
                                ?.Find("Points")
                                ?.Find(item.Value.Type.ToString()) == null
                                ? null
                                : SkeletonMangerTransform
                                    .Find("Skeleton(Clone)")
                                    .Find("Points")
                                    .Find(item.Value.Type.ToString())
                                    .transform
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
        if (!ReferenceManager.instance.graphManagers.Any(x => x.MySineWave.isVideoDoneLoading))
            RecordFoots();
        //SetInitialFootPlace();
        if (
            coroutine == null
            && ReferenceManager.instance.graphManagers.Any(x => x.MySineWave.isVideoDoneLoading)
        )
            coroutine = StartCoroutine(DetectFootOnGround());
        DetectFootFullyPressed();
        generate();
    }
    public GameObject cubePrefab;
    List<GameObject> cubes = new List<GameObject>();
    public void generate(){
        foreach(var bodyData in researchProjectCompleteBodyDatas){
            var alreadycube = cubes.FirstOrDefault(x=>x.name == bodyData.name);
            if (alreadycube != null)
            {
                alreadycube.transform.localPosition = bodyData.Position3D;
            }
            else
            {
                GameObject cube = Instantiate(cubePrefab);
                cube.name = bodyData.name;
                cube.transform.localPosition = bodyData.Position3D;
                cubes.Add(cube);
            }
        }
    }
    public void DetectFootFullyPressed(){
        ResearchProjectCompleteBodyData ankleLeft =
                researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                    x.gameObject.name == JointType.AnkleLeft.ToString()
                );

            ResearchProjectCompleteBodyData ankleRight =
                researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                    x.gameObject.name == JointType.AnkleRight.ToString()
                );
            if (ankleLeft == null || ankleRight == null)
                return;

            if(Math.Abs(ankleLeft.Position3D.z - ankleRight.Position3D.z)<=0.05f){
                if(ankleLeft.Position3D.y < ankleRight.Position3D.y){
                            Debug.Log("Right Foot fully pressed");
                }
                if(ankleRight.Position3D.y < ankleLeft.Position3D.y){
                            Debug.Log("Left Foot is fully pressed");
                }
    }

}
    public List<float> footDistances = new List<float>();

    public void RecordFoots()
    {
        ResearchProjectCompleteBodyData jointForStrideLengthL =
            researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleLeft.ToString()
            );

        ResearchProjectCompleteBodyData jointForStrideLengthR =
            researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleRight.ToString()
            );
        if (
            jointForStrideLengthL == null
            || !jointForStrideLengthL.gameObject.activeSelf
            || jointForStrideLengthR == null
            || !jointForStrideLengthR.gameObject.activeSelf
        )
        {
            return;
        }
        float distance = Vector3.Distance(
            jointForStrideLengthL.Position3D,
            jointForStrideLengthR.Position3D
        );
        if (!footDistances.Contains(distance))
        {
            footDistances.Add(distance);
            footDistances.Sort();
        }
    }

    Coroutine coroutine;
    public Vector3 previousPosition;

    public IEnumerator DetectFootOnGround()
    {
        if(footDistances.Count == 0)
        {
            ReferenceManager.instance.graphManagers.ForEach(x=>x.MySineWave.isVideoDoneLoading = false);
            coroutine = null;
            yield break;
        }
        if (ReferenceManager.instance.videoPlayerView.VideoPlayer.IsPaused)
        {
            // Debug.Log("Video Is paused");
            coroutine = null;
            yield break;
        }
        ResearchProjectCompleteBodyData jointForStrideLengthL =
            researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleLeft.ToString()
            );
        ResearchProjectCompleteBodyData jointForStrideLengthR =
            researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleRight.ToString()
            );
        if (
            jointForStrideLengthL == null
            || !jointForStrideLengthL.gameObject.activeSelf
            || jointForStrideLengthR == null
            || !jointForStrideLengthR.gameObject.activeSelf
        )
        {
            jointForStrideLengthL?.ShockWaveEffect.SetActive(false);
            jointForStrideLengthR?.ShockWaveEffect.SetActive(false);
            coroutine = null;
            yield break;
        }
        // Debug.Log(
        //     $"Left Z Pos: {jointForStrideLengthL.Position3D.z}\nRight Z Pos: {jointForStrideLengthR.Position3D.z}"
        // );

        yield return new WaitForEndOfFrame();
        if (leftLeg)
        {
            if (jointForStrideLengthL.Position3D.z > jointForStrideLengthR.Position3D.z)
            {
                jointForStrideLengthL.ShockWaveEffect.SetActive(false);
                jointForStrideLengthR.ShockWaveEffect.SetActive(false);
                coroutine = null;
                yield break;
            }
            debugDistance = Vector3.Distance(
                jointForStrideLengthL.Position3D,
                jointForStrideLengthR.Position3D
            );
            if (
                Vector3.Distance(jointForStrideLengthL.Position3D, jointForStrideLengthR.Position3D)
                >= footDistances.Max() - float.Parse(toloranceValue.text)
            )
            {
                // Debug.Log("Foot Detected");
                StepButon.onClick.Invoke();
                
                jointForStrideLengthL.ShockWaveEffect.SetActive(true);
                jointForStrideLengthR.ShockWaveEffect.SetActive(false);
                yield return new WaitForSeconds(1);
            }
            else
            {
                previousPosition = jointForStrideLengthL.Position3D;
                jointForStrideLengthL.ShockWaveEffect.SetActive(false);
            }
        }
        else
        {
            if (jointForStrideLengthR.Position3D.z > jointForStrideLengthL.Position3D.z)
            {
                // Debug.Log("Because z is less or behind");
                jointForStrideLengthL.ShockWaveEffect.SetActive(false);
                jointForStrideLengthR.ShockWaveEffect.SetActive(false);
                coroutine = null;
                yield break;
            }
            if (
                Vector3.Distance(jointForStrideLengthL.Position3D, jointForStrideLengthR.Position3D)
                >= footDistances.Max() - float.Parse(toloranceValue.text)
            )
            {
                // Debug.Log("Foot Detected");
                StepButon.onClick.Invoke();
                
                jointForStrideLengthR.ShockWaveEffect.SetActive(true);
                jointForStrideLengthL.ShockWaveEffect.SetActive(false);
                yield return new WaitForSeconds(1);
            }
            else
            {
                previousPosition = jointForStrideLengthR.Position3D;
                jointForStrideLengthR.ShockWaveEffect.SetActive(false);
            }
        }
        coroutine = null;
    }

    public void ClearFootOnGroundPosition()
    {
        footOnGroundPosition = null;
    }

    public void SetInitialFootPlace()
    {
        ResearchProjectCompleteBodyData jointForStrideLengthL = null;

        if (leftLeg)
        {
            jointForStrideLengthL = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleLeft.ToString()
            );
            if (
                !GeneralStaticManager.GraphsReadings.ContainsKey(
                    MeasurementType.HipAnkleHipKneeLeftAbductionDifference.ToString()
                )
            )
            {
                ReferenceManager.instance.PopupManager.Show(
                    "Need Gait Data",
                    "Please Select gait measures in measurement selector"
                );
                return;
            }
        }
        else
        {
            jointForStrideLengthL = researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                x.gameObject.name == JointType.AnkleRight.ToString()
            );
            if (
                !GeneralStaticManager.GraphsReadings.ContainsKey(
                    MeasurementType.HipAnkleHipKneeRightAbductionDifference.ToString()
                )
            )
            {
                ReferenceManager.instance.PopupManager.Show(
                    "Need Gait Data",
                    "Please Select gait measures in measurement selector"
                );
                return;
            }
        }

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
            footStrikeAtTimes.Clear();
            AddTimerReading();

            lastFootPosition = jointForStrideLengthL.Position3D;

            footCount += 1;
            TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
                $"Set Step {footCount + 1}";
        }
        else if (footCount > 0)
        {
            if (footCount != 1)
            {
                lastFootPosition = newFootPosition;
            }
            AddTimerReading();
            newFootPosition = jointForStrideLengthL.Position3D;
            float distance = Vector3.Distance(newFootPosition, lastFootPosition);
            distance = Mathf.Round(distance * 100.0f) * 0.01f;
            strideLengthNotifier.text = $"Stride Length\n{distance}m";

            footCount += 1;
            TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
                $"Set Step {footCount + 1}";
        }
    }

    public void AddTimerReading()
    {
        float footstrikesAtTime = 0;
        if (ReferenceManager.instance.videoPlayerView.gameObject.activeSelf)
        {
            footstrikesAtTime = (float)ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds;
            if (!footStrikeAtTimes.Contains(footstrikesAtTime))
                footStrikeAtTimes.Add(footstrikesAtTime);
        }
        else
        {
            footstrikesAtTime = ReferenceManager.instance.Timer;
            if (!footStrikeAtTimes.Contains(footstrikesAtTime))
                footStrikeAtTimes.Add(footstrikesAtTime);
        }
        float maxAngle;
        float maxDistance;
        float currentAngle;
        float curreentDistance;
        if (leftLeg)
        {
            maxAngle = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipAnkleHipKneeLeftAbductionDifference.ToString()]
                .Max();

            currentAngle = ReferenceManager
                .instance
                .angleManager
                ._angles[MeasurementType.HipAnkleHipKneeLeftAbductionDifference]
                .Angle;
        }
        else
        {
            maxAngle = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipAnkleHipKneeRightAbductionDifference.ToString()]
                .Max();

            currentAngle = ReferenceManager
                .instance
                .angleManager
                ._angles[MeasurementType.HipAnkleHipKneeRightAbductionDifference]
                .Angle;
        }
        if (leftLeg)
        {
            maxDistance = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipKneeLeftDistance.ToString()]
                .Max();

            curreentDistance = ReferenceManager
                .instance
                .angleManager
                ._angles[MeasurementType.HipKneeLeftDistance]
                .Angle;
        }
        else
        {
            maxDistance = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipKneeRightDistance.ToString()]
                .Max();

            curreentDistance = ReferenceManager
                .instance
                .angleManager
                ._angles[MeasurementType.HipKneeRightDistance]
                .Angle;
        }
        if (!ReferenceManager.instance.maxAngleAtFootStrikingTime.ContainsKey(footstrikesAtTime))
            ReferenceManager.instance.maxAngleAtFootStrikingTime.Add(footstrikesAtTime, maxAngle);
        else
            ReferenceManager.instance.maxAngleAtFootStrikingTime[footstrikesAtTime] = maxAngle;
        if (!ReferenceManager.instance.maxDistanceAtFootStrikingTime.ContainsKey(footstrikesAtTime))
            ReferenceManager.instance.maxDistanceAtFootStrikingTime.Add(
                footstrikesAtTime,
                maxDistance
            );
        else
            ReferenceManager.instance.maxDistanceAtFootStrikingTime[footstrikesAtTime] =
                maxDistance;
        if (!ReferenceManager.instance.AngleAtFootStrikingTime.ContainsKey(footstrikesAtTime))
            ReferenceManager.instance.AngleAtFootStrikingTime.Add(footstrikesAtTime, currentAngle);
        else
            ReferenceManager.instance.AngleAtFootStrikingTime[footstrikesAtTime] = currentAngle;
        if (!ReferenceManager.instance.DistanceAtFootStrikingTime.ContainsKey(footstrikesAtTime))
            ReferenceManager.instance.DistanceAtFootStrikingTime.Add(
                footstrikesAtTime,
                curreentDistance
            );
        else
            ReferenceManager.instance.DistanceAtFootStrikingTime[footstrikesAtTime] =
                curreentDistance;
        

    }

    public void Reset()
    {
        footCount = 0;
        TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
            $"Set Step {footCount + 1}";

        ReferenceManager.instance.maxAngleAtFootStrikingTime.Clear();
        ReferenceManager.instance.maxDistanceAtFootStrikingTime.Clear();
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

    public void TakeUserConsent()
    {
        userConsentPanel.gameObject.SetActive(true);
    }

    public void OnLeftConsentSelection(bool value)
    {
        leftLeg = value;
        if (value)
        {
            userConsentPanel.LeftToggle.image.DOColor(UnityEngine.Color.green, 1f);
            userConsentPanel.RightToggle.isOn = false;
            ;
        }
        else
        {
            userConsentPanel.LeftToggle.image.DOColor(UnityEngine.Color.white, 1f);
        }
    }

    public void OnRightConsentSelection(bool value)
    {
        rightLeg = value;
        if (value)
        {
            userConsentPanel.RightToggle.image.DOColor(UnityEngine.Color.green, 1f);
            userConsentPanel.LeftToggle.isOn = false;
        }
        else
        {
            userConsentPanel.RightToggle.image.DOColor(UnityEngine.Color.white, 1f);
        }
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
