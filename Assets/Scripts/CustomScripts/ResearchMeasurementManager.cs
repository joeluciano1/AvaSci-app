using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DG.Tweening;
using LightBuzz.AvaSci.Measurements;
using LightBuzz.BodyTracking;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ResearchMeasurementManager : MonoBehaviour
{
    public static ResearchMeasurementManager instance;
    public Body LightbuzzBody;
    public FrameData LightbuzzFrame;

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
    public bool isStarted;
    public float tollorance;
    public Button TriggerButton;
    public List<string> footStrikeAtTimes = new List<string>();
    public UserConsentPanel userConsentPanel;

    [HideInInspector]
    public bool leftLeg;

    [HideInInspector]
    public bool rightLeg;
    public Button StepButon;
    public float debugDistance;
    public TMP_InputField toloranceValue;
    bool informationShown;
    public ProcessingNotifier processingNotifier;
    public Dictionary<string,float> abdDiffAtTime = new Dictionary<string,float>();
    public Dictionary<string,float> varValAtTime = new Dictionary<string,float>();
    public Dictionary<string,float> kneeAbdAtTime = new Dictionary<string,float>();
    public Dictionary<string,float> ankleAbdAtTime = new Dictionary<string,float>();
    public Dictionary<string,float> pelvisAngleAtTime = new Dictionary<string,float>();
    public Dictionary<string,float> strideLengthAtTime = new Dictionary<string,float>();
    public float leftAngleValue;
	public float rightAngleValue;
	public float leftDisValue;
	public float rightDisValue;
    public float leftKneeAbdValue;
    public float rightKneeAbdValue;
    public float leftAnkleAbdValue;
    public float rightAnkleAbdValue;
    public float pelvisAngleValue;

    public List<TimeBasedReadingRequest> timebasedReadings = new List<TimeBasedReadingRequest>();
    private void Awake()
    {
        
        instance = this;
    }

    public void StartReading()
    {
        
        assigned = false;
        timebasedReadings.Clear();
        pelvisBaseHeight = 0;
        possibleFootStepPoints.Clear();
        ReferenceManager.instance.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        previousPosition = Vector3.zero;
        isStarted = true;
        footCount = 0;
        firstStepIgnored = false;
        ReferenceManager.instance.videoPlayingCount = 0;
        processingNotifier.NotifierText.text = "Reading Video Data...";
        processingNotifier.gameObject.SetActive(true);
        // if (!informationShown)
        // {
        //     ReferenceManager.instance.PopupManager.Show("Information", "Please run the video completely once again to complete the foot detection record. Thanks!");
        //     informationShown = true;
        // }
        abdDiffAtTime.Clear();
        varValAtTime.Clear();
        kneeAbdAtTime.Clear();
        ankleAbdAtTime.Clear();
        pelvisAngleAtTime.Clear();
        footStrikeAtTimes.Clear();
        ReferenceManager.instance.AngleAtFootStrikingTime.Clear();
        ReferenceManager.instance.DistanceAtFootStrikingTime.Clear();
        ReferenceManager.instance.maxAngleAtFootStrikingTime.Clear();
        ReferenceManager.instance.maxDistanceAtFootStrikingTime.Clear();
        ReferenceManager.instance.AnkleAbductionAtFootStrikingTime.Clear();
        ReferenceManager.instance.KneeAbductionAtFootStrikingTime.Clear();
        ReferenceManager.instance.PelvisAngleAtFootStrikingTime.Clear();
        ReferenceManager.instance.VarusValgusAtFootStrikingTime.Clear();
        ReferenceManager.instance.heelPressDetectionBodies.Clear();
        ReferenceManager.instance.standingDetectionBodies.Clear();
        isDoneWithLeft = false;
        isDoneWithRight = false;
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

    public TMP_Text testing;
    private float? initialYPosition;
    public void FixedUpdate()
    {
        if (LightbuzzBody != null)
        {
            // var ankleLeft = LightbuzzBody.Joints[JointType.AnkleLeft];
            // Floor floor = Floor.Create(LightbuzzFrame);
            //
            // testing.text = $"Ankle Left Position = {ankleLeft.Position3D.Y}\nFloor Position = {floor.Y}";
            // var ankle = LightbuzzBody.Joints[JointType.AnkleLeft];
            // var hip = LightbuzzBody.Joints[JointType.HipLeft];
            // testing.text = $"{ankle.Position3D.Y - hip.Position3D.Y}";
        }
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
        
        //SetInitialFootPlace();
        // if (
        //     coroutine == null
        //     && ReferenceManager.instance.graphManagers.Any(x => x.MySineWave.isVideoDoneLoading) && ReferenceManager.instance.videoPlayingCount ==1
        // )
        //     coroutine = StartCoroutine(DetectFootOnGround());
        // DetectFootFullyPressed();
        if (ReferenceManager.instance.videoPlayingCount == 1 && !ReferenceManager.instance.videoPlayerView.VideoPlayer.IsPaused )
            RecordFoots();
        // AddAllTimeReadings(); 
        // GenerateAvatar();
        // DetectIfCubePassed();
        // DetectIfUserIsStanding();
        // FootFullyPressedNewDetection();
    }
    public Button StopButon;
    public bool isDoneWithLeft,isDoneWithRight;
    public async void DetectIfUserIsStanding()
    {
        if (ReferenceManager.instance.videoPlayingCount == 2)
        {
            processingNotifier.NotifierText.text = "Detecting Initial Pose Of User...";
            processingNotifier.gameObject.SetActive(true);
            var pelvis = cubes.FirstOrDefault(x => x.gameObject.name == "Pelvis");
            if (pelvis == null)
            {
                return;   
            }
            float zPosOfPelvis = pelvis.transform.position.z;
            await Task.Delay(500);
            if(Math.Abs(zPosOfPelvis - pelvis.transform.position.z)<=0.05f)
            {
                StandingDetectionCreatePutValues(ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds.ToString());
            }
        }
        if(ReferenceManager.instance.videoPlayingCount == 3)
        {
            StopButon.onClick.Invoke();
            ReferenceManager.instance.placeHeelDetectionValues = false;
            processingNotifier.NotifierText.text = "Select An Option";
            processingNotifier.gameObject.SetActive(true);
            processingNotifier.LoadingFill.transform.parent.gameObject.SetActive(false);
            processingNotifier.buttons.SetActive(true);
            ReferenceManager.instance.videoPlayerView.VideoPlayer.Speed = 1f;
            if (leftLeg)
            {
                isDoneWithLeft = true;
            }
            else if (rightLeg)
            {
                isDoneWithRight = true;
            }

            ReferenceManager.instance.canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
    }
    
    public void DetectIfCubePassed(){
        var cubeLeft = cubes.FirstOrDefault(x => x.gameObject.name == "AnkleLeft");
        var cubeRight = cubes.FirstOrDefault(x => x.gameObject.name == "AnkleRight");
        if(cubeLeft == null || cubeRight == null)
        {
            return;
        }
        if(Math.Abs(cubeLeft.transform.localPosition.z - cubeRight.transform.localPosition.z)<=0.03f){
            Debug.Log("Wajya Nai par phar lya");
             FootFullyPressedNewDetection(ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds.ToString());
        }
    }
    public void AddAllTimeReadings()
    {
        if (timebasedReadings == null)
        {
            return;
        }
        if(ReferenceManager.instance.videoPlayerView.VideoPlayer.IsPaused && !ReferenceManager.instance.angleManager._angles.ContainsKey(MeasurementType.HipAnkleHipKneeLeftAbductionDifference)||!ReferenceManager.instance.angleManager._angles.ContainsKey(MeasurementType.VarusValgusLeftAngleDistance))
        {
            return;
        }    
        if(ReferenceManager.instance.videoPlayingCount == 0)
        {
            double timeValue = ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds;
            if(!abdDiffAtTime.ContainsKey(timeValue.ToString()))
            {
                if (leftLeg)
                {
                    leftAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .AnkleHipLeftAbductionDifference??0f;
                    abdDiffAtTime.Add(timeValue.ToString(), leftAngleValue);
                    
                }
                else if (rightLeg)
                {
                    rightAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .AnkleHipRightAbductionDifference??0f;
                    abdDiffAtTime.Add(timeValue.ToString(), rightAngleValue);
                    
                }
            }
            else
            {
                if (leftLeg)
                {
                    leftAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .AnkleHipLeftAbductionDifference??0f;
                    abdDiffAtTime[timeValue.ToString()] = leftAngleValue;
                    
                }
                else if (rightLeg)
                {
                    rightAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .AnkleHipRightAbductionDifference??0;
                    abdDiffAtTime[timeValue.ToString()] = rightAngleValue;
                    
                }

            }
            if(!varValAtTime.ContainsKey(timeValue.ToString()))
            {
                if (leftLeg)
                {
                    leftDisValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .VarusValgusLeft??0;
                    varValAtTime.Add(timeValue.ToString(), rightAngleValue);
                    
                }
                else if (rightLeg)
                {
                    rightAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .VarusValgusRight??0;
                    varValAtTime.Add(timeValue.ToString(), rightAngleValue);
                    
                }
            }
            else
            {
                if (leftLeg)
                {
                    leftDisValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .VarusValgusLeft??0;
                    varValAtTime[timeValue.ToString()] = leftDisValue;
                    
                }
                else if (rightLeg)
                {
                    rightAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .VarusValgusRight??0;
                    varValAtTime[timeValue.ToString()] = rightDisValue;
                    
                }
            }
            // if(!ankleAbdAtTime.ContainsKey(timeValue.ToString()))
            // {
            //     if (leftLeg)
            //     {
            //         leftAnkleAbdValue = (float)timebasedReadings.FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))
            //             .AnkleLeftAbduction;
            //         ankleAbdAtTime.Add(timeValue.ToString(), leftAnkleAbdValue);
            //         
            //     }
            //     else if (rightLeg)
            //     {
            //         rightAnkleAbdValue = (float)timebasedReadings.FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))
            //             .AnkleRightAbduction;
            //         ankleAbdAtTime.Add(timeValue.ToString(), rightAnkleAbdValue);
            //         
            //     }
            // }
            // else
            // {
            //     if (leftLeg)
            //     {
            //         leftAnkleAbdValue = (float)timebasedReadings.FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))
            //             .AnkleLeftAbduction;
            //         ankleAbdAtTime[timeValue.ToString()] = leftAnkleAbdValue;
            //         
            //     }
            //     else if (rightLeg)
            //     {
            //         rightAnkleAbdValue = (float)timebasedReadings.FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))
            //             .AnkleRightAbduction;
            //         ankleAbdAtTime[timeValue.ToString()] = rightAnkleAbdValue;
            //         
            //     }
            // }
            if(!kneeAbdAtTime.ContainsKey(timeValue.ToString()))
            {
                if (leftLeg)
                {
                    leftKneeAbdValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .HipLeftAbduction??0;
                    kneeAbdAtTime.Add(timeValue.ToString(), leftKneeAbdValue);
                    
                }
                else if (rightLeg)
                {
                    rightKneeAbdValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .HipRightAbduction??0;
                    kneeAbdAtTime.Add(timeValue.ToString(), rightKneeAbdValue);
                    
                }
            }
            else
            {
                if (leftLeg)
                {
                    rightKneeAbdValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .HipLeftAbduction??0;
                    kneeAbdAtTime[timeValue.ToString()] = leftKneeAbdValue;
                    
                }
                else if (rightLeg)
                {
                    rightKneeAbdValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .HipRightAbduction??0;
                    kneeAbdAtTime[timeValue.ToString()] = rightKneeAbdValue;
                    
                }
            }
            if(!pelvisAngleAtTime.ContainsKey(timeValue.ToString()))
            {
                if (leftLeg)
                {
                    pelvisAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .PelvisAngle??0;
                    pelvisAngleAtTime.Add(timeValue.ToString(), pelvisAngleValue);
                }

                else if (rightLeg)
                {
                    pelvisAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .PelvisAngle??0;
                    pelvisAngleAtTime.Add(timeValue.ToString(), pelvisAngleValue);
                }
            }
            else
            {
                if (leftLeg)
                {
                    pelvisAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .PelvisAngle??0;
                    pelvisAngleAtTime[timeValue.ToString()] = pelvisAngleValue;
                }
                else if (rightLeg)
                {
                    pelvisAngleValue = timebasedReadings.FirstOrDefault(x => Math.Round(double.Parse(x.TimeOfReading),5).Equals(Math.Round(timeValue,5)))?
                        .PelvisAngle??0;
                    pelvisAngleAtTime[timeValue.ToString()] = pelvisAngleValue;
                }
            }

            if (!strideLengthAtTime.ContainsKey(timeValue.ToString()))
            {
                if (leftLeg)
                {
                    strideLengthAtTime.Add(timeValue.ToString(), strideLengthDistance);
                }
                else if (rightLeg)
                {
                    strideLengthAtTime.Add(timeValue.ToString(), strideLengthDistance);
                }
            }
            else
            {
                if (leftLeg)
                {
                    strideLengthAtTime[timeValue.ToString()] = strideLengthDistance;
                }
                else if (rightLeg)
                {
                    strideLengthAtTime[timeValue.ToString()] = strideLengthDistance;
                }
            }
            var toBeRemoved = abdDiffAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemoved)
                abdDiffAtTime.Remove(item.Key);
                
            var toBeRemovedVar = varValAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemovedVar)
                varValAtTime.Remove(item.Key);

            var toBeRemovedAnk = ankleAbdAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemovedAnk)
                ankleAbdAtTime.Remove(item.Key);    

            var toBeRemovedKnee = kneeAbdAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemovedKnee)
                kneeAbdAtTime.Remove(item.Key);    

            var toBeRemovedPelv = pelvisAngleAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemovedPelv)
                pelvisAngleAtTime.Remove(item.Key);   
            
            var toBeRemovedStride = strideLengthAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemoved)
                strideLengthAtTime.Remove(item.Key);

            abdDiffAtTime = abdDiffAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value); 
            varValAtTime = varValAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value); 
            ankleAbdAtTime =  ankleAbdAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value); 
            kneeAbdAtTime =  kneeAbdAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value); 
            pelvisAngleAtTime =  pelvisAngleAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value);
            strideLengthAtTime =  strideLengthAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value);
        }
        if(ReferenceManager.instance.videoPlayingCount == 1){
             processingNotifier.NotifierText.text = "Gathering Heel Pressed Data...";
           
            processingNotifier.gameObject.SetActive(true);
        }
        // if(ReferenceManager.instance.videoPlayingCount >3 &&footStrikeAtTimes.Count!=0)
        // {
            
        //         processingNotifier.NotifierText.text = "";
        //         ReferenceManager.instance.placeHeelDetectionValues = false;
        //         processingNotifier.gameObject.SetActive(false);
        // }
    }
    public GameObject cubePrefab;
    List<GameObject> cubes = new List<GameObject>();
    public void GenerateAvatar(){
        foreach(var item in researchProjectCompleteBodyDatas)
        {
            var cubealready = cubes.FirstOrDefault(x => x.name == item.gameObject.name);
            if(cubealready != null){
                cubealready.transform.localPosition = item.Position3D;
            }
            else
            {
                var go = Instantiate(cubePrefab,cubePrefab.transform.parent);
                go.name = item.name;
                go.SetActive(true);
                go.transform.localPosition = item.Position3D;
                if(item.name.Contains("Ankle"))
                {
                    go.GetComponent<BoxCollider>().enabled = true;
                }
                cubes.Add(go);
            }
        }
    }
   
   public void StandingDetectionCreatePutValues(string timeDetectedOn = "")
   {
        string finalValue = timeDetectedOn;
        var abdDict = abdDiffAtTime.FirstOrDefault(x => finalValue == x.Key);
        var varDict = varValAtTime.FirstOrDefault(x => finalValue==x.Key);
        var kneeAbdDict = kneeAbdAtTime.FirstOrDefault(x => finalValue==x.Key);
        var ankleAbdDict = ankleAbdAtTime.FirstOrDefault(x => finalValue==x.Key);
        var pelvAngleDict = pelvisAngleAtTime.FirstOrDefault(x => finalValue==x.Key);

        StandingDetectionBody standingDetectionBody = new StandingDetectionBody()
        {
            TimeofStanding = finalValue,
            angleDifferenceValue = abdDict.Value,
            distanceValue = varDict.Value,
            nameOfTheFoot = leftLeg? "Left Leg":"Right Leg",
            kneeAbductionValue = kneeAbdDict.Value,
            ankleAbductionValue = ankleAbdDict.Value,
            pelvisAngleValue = pelvAngleDict.Value,
        };
        if(!ReferenceManager.instance.standingDetectionBodies.Any(x=>x.TimeofStanding == standingDetectionBody.TimeofStanding))
                    ReferenceManager.instance.standingDetectionBodies.Add(standingDetectionBody);

                ReferenceManager.instance.standingDetectionBodies = ReferenceManager.instance.standingDetectionBodies.OrderBy(x => x.TimeofStanding).ToList();
   }
   public void FootFullyPressedNewDetection(string timeDetectedOn = "")
   {
        if(footCount == 0)
        {
            return;
        }
        if(ReferenceManager.instance.videoPlayingCount ==1 &&footStrikeAtTimes.Count!=0)
        {
                string finalValue = timeDetectedOn;
                
                var abdDict = abdDiffAtTime.FirstOrDefault(x => finalValue==x.Key);
                var varDict = varValAtTime.FirstOrDefault(x => finalValue==x.Key);
                var kneeAbdDict = kneeAbdAtTime.FirstOrDefault(x => finalValue==x.Key);
                var ankleAbdDict = ankleAbdAtTime.FirstOrDefault(x => finalValue==x.Key);
                var pelvAngleDict = pelvisAngleAtTime.FirstOrDefault(x => finalValue==x.Key);
                if (abdDict.Value == 0 || varDict.Value == 0 || kneeAbdDict.Value == 0 || ankleAbdDict.Value == 0 ||
                    pelvAngleDict.Value == 0)
                {
                    return;
                }
                // Debug.Log("Adding " + abdDict.Value + " at " + abdDict.Key + " With Float Value " + finalValue);
                HeelPressDetectionBody heelPressDetectionBody = new HeelPressDetectionBody()
                {
                    TimeOfHeelPressed = finalValue,
                    angleDifferenceValue = abdDict.Value,
                    distanceValue = varDict.Value,
                    nameOfTheFoot = leftLeg? "Left Leg":"Right Leg",
                    kneeAbductionValue = kneeAbdDict.Value,
                    ankleAbductionValue = ankleAbdDict.Value,
                    pelvisAngleValue = pelvAngleDict.Value
                };
                if(!ReferenceManager.instance.heelPressDetectionBodies.Any(x=>x.TimeOfHeelPressed == heelPressDetectionBody.TimeOfHeelPressed))
                    ReferenceManager.instance.heelPressDetectionBodies.Add(heelPressDetectionBody);

                ReferenceManager.instance.heelPressDetectionBodies = ReferenceManager.instance.heelPressDetectionBodies.OrderBy(x => x.TimeOfHeelPressed).ToList();
        }
       
    }
#region  Deprecated
    // TMP_Text testText;
    public void DetectFootFullyPressed(){
        ResearchProjectCompleteBodyData ankleLeft =
                researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                    x.gameObject.name == JointType.AnkleLeft.ToString()
                );

            ResearchProjectCompleteBodyData ankleRight =
                researchProjectCompleteBodyDatas.FirstOrDefault(x =>
                    x.gameObject.name == JointType.AnkleRight.ToString()
                );
            if (ankleLeft == null || ankleRight == null || ReferenceManager.instance.graphManagers.Any(x=>!x.MySineWave.isVideoDoneLoading))
                return;
        // if (testText == null)
        // {
        //     GameObject go = new GameObject();
        //     testText = go.AddComponent<TextMeshProUGUI>();
        //     testText.transform.parent = ankleLeft.transform.parent;
        //     testText.transform.localScale = Vector3.one;
        //     testText.transform.localPosition = Vector3.zero;
        // }

        // // testText.text = GeneralStaticManager.ClosestTo(footDistances, Math.Abs(ankleLeft.Position3D.z- ankleRight.Position3D.z)).ToString();
        // testText.text = (Mathf.Round(Math.Abs(ankleLeft.Position3D.z - ankleRight.Position3D.z)*10)/10).ToString();
        float valueToCompare = Mathf.Round(Math.Abs(ankleLeft.Position3D.z - ankleRight.Position3D.z) * 10) / 10;
        // float closestToValue = GeneralStaticManager.ClosestTo(footZDistances, Math.Abs(ankleLeft.Position3D.z - ankleRight.Position3D.z));
        // Debug.Log("Current Difference: " + Math.Abs(ankleLeft.Position3D.z - ankleRight.Position3D.z));
            if(valueToCompare==0.2f)
            {
            
                if(ankleLeft.Position3D.y < ankleRight.Position3D.y && rightLeg)
                {
                    Debug.Log("Right Foot fully pressed");
                    HeelPressDetectionBody heelPressDetectionBody = new HeelPressDetectionBody()
                    {
                        Subject = GeneralStaticManager.GlobalVar["Subject"],
                        nameOfTheFoot = "Right Leg",
                        angleDifferenceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.HipAnkleHipKneeRightAbductionDifference].Angle,
                        distanceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.HipKneeRightDistance].Angle,
                        TimeOfHeelPressed= ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds.ToString("0.0")
                    };
                    var alreadyPresent = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x => x.TimeOfHeelPressed == heelPressDetectionBody.TimeOfHeelPressed);
                    if (alreadyPresent != null)
                    {
                        alreadyPresent = heelPressDetectionBody;
                    }
                    else
                    {
                        ReferenceManager.instance.heelPressDetectionBodies.Add(heelPressDetectionBody);
                        ReferenceManager.instance.heelPressDetectionBodies = ReferenceManager.instance.heelPressDetectionBodies.OrderBy(x => float.Parse(x.TimeOfHeelPressed)).ToList();
                    }
                }
                if(ankleRight.Position3D.y < ankleLeft.Position3D.y && leftLeg)
                {
                    Debug.Log("Left Foot is fully pressed");
                    HeelPressDetectionBody heelPressDetectionBody = new HeelPressDetectionBody()
                    {
                        Subject = GeneralStaticManager.GlobalVar["Subject"],
                        nameOfTheFoot = "Left Leg",
                        angleDifferenceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.HipAnkleHipKneeLeftAbductionDifference].Angle,
                        distanceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.HipKneeLeftDistance].Angle,
                        TimeOfHeelPressed= ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds.ToString("0.0")
                    };
                    var alreadyPresent = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x => x.TimeOfHeelPressed == heelPressDetectionBody.TimeOfHeelPressed);
                    if (alreadyPresent != null)
                    {
                        alreadyPresent = heelPressDetectionBody;
                    }
                    else
                    {
                        ReferenceManager.instance.heelPressDetectionBodies.Add(heelPressDetectionBody);
                        ReferenceManager.instance.heelPressDetectionBodies = ReferenceManager.instance.heelPressDetectionBodies.OrderBy(x => float.Parse(x.TimeOfHeelPressed)).ToList();
                    }
                }
    }

}
#endregion

    public float footDistanceThreshold = 0.07f;
    public float footDistanceOffset=0.01f;
    public List<float> footDistances = new List<float>();
    // public List<float> footZDistances = new List<float>();
    [FormerlySerializedAs("pelvisBaseFootHeight")] [FormerlySerializedAs("baseFootHeight")] public float pelvisBaseHeight;
    public List<float> possibleFootStepPoints = new List<float>();
    public float previousRecordedZofStep;
    private bool assigned;
    public Button cancelButton;
    public void ChangeThreshold(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        footDistanceThreshold = float.Parse(value);
    }
    bool AlreadyExists(List<float> list, float value, float epsilon = 0.001f)
    {
        return list.Any(x => Mathf.Abs(x - value) < epsilon);
    }
    List<float> SmoothEMA(List<float> values, float alpha = 0.3f)
    {
        if (values == null || values.Count == 0) return values;

        List<float> smoothed = new List<float> { values[0] };

        for (int i = 1; i < values.Count; i++)
        {
            float ema = alpha * values[i] + (1 - alpha) * smoothed[i - 1];
            smoothed.Add(ema);
        }

        return smoothed;
    }
    float GetMedian(List<float> values)
    {
        if (values == null || values.Count == 0)
            throw new ArgumentException("List is empty or null");

        var sorted = values.OrderBy(x => x).ToList();
        int midIndex = sorted.Count / 2;

        if (sorted.Count % 2 == 0)
        {
            // Even number of elements → average of two middles
            return (sorted[midIndex - 1] + sorted[midIndex]) / 2f;
        }
        else
        {
            // Odd number of elements → exact middle
            return sorted[midIndex];
        }
    }

    
    public async void RecordFoots()
    {
        if (!assigned)
        {
            string theCSVJson =
                await GeneralStaticManager.ConvertCsvStringToJson(ReferenceManager.instance.LightBuzzMain
                    .GenerateCSVString());
            timebasedReadings = JsonConvert.DeserializeObject<List<TimeBasedReadingRequest>>(theCSVJson);
            // timebasedReadings.ForEach(x=>
            // {
            //     if(x.AnkleLeft3DZ!=null)
            //        x.AnkleLeft3DZ = Mathf.RoundToInt((float)x.AnkleLeft3DZ);
            //     if(x.AnkleRight3DZ != null)
            //         x.AnkleRight3DZ = Mathf.RoundToInt((float)x.AnkleRight3DZ);
            // });
            //
            
           
            
            footDistances.Clear();
            
            foreach (var timeBasedReading in timebasedReadings)
            {
                if (timeBasedReading.AnkleLeft3DZ != null && timeBasedReading.AnkleRight3DZ != null)
                {
                    float value = Mathf.Abs((float)timeBasedReading.AnkleLeft3DZ -
                                            (float)timeBasedReading.AnkleRight3DZ);
                    
                        footDistances.Add(value);
                }
            }
            footDistances.Sort();


            footDistances = SmoothEMA(footDistances);
            
            footDistanceThreshold = GetMedian(footDistances);
            footDistanceThreshold = Mathf.Clamp(footDistanceThreshold, 0.01f, 0.1f);
           
            Debug.Log("Threshold: " + footDistanceThreshold);
            
            int leftDone = 0;
            int rightDone = 0;
            float lastZPostion = 0;
         if (leftLeg)
            {
                foreach (var item in timebasedReadings)
                {
                   

                    TimeBasedReadingRequest angleData;
                    TimeBasedReadingRequest distanceData;
                    TimeBasedReadingRequest kneeData;
                    TimeBasedReadingRequest pelvisData;
                    
                    if (item.AnkleLeft3DZ < item.AnkleRight3DZ && Mathf.Abs((float)item.AnkleLeft3DZ - (float)item.AnkleRight3DZ) > footDistanceThreshold + footDistanceOffset && float.Parse(item.TimeOfReading)>1.0)
                    {
                        angleData = item;
                        distanceData = item;
                        kneeData = item;
                        pelvisData = item;
                    }
                    else
                    {
                        continue;
                    }
                    var middleItem = timebasedReadings.FirstOrDefault(x => x.AnkleHipLeftAbductionDifference != null && float.Parse(x.TimeOfReading)> float.Parse(angleData.TimeOfReading) && Mathf.Abs((float)x.AnkleRight3DZ - (float)x.AnkleLeft3DZ)<=footDistanceThreshold/1.5f);
                    if (middleItem == null)
                    {
                        continue;
                    }
                    if (leftDone < 2)
                    {
                        leftDone += 1;
                        continue;
                    }
                    if (angleData != null)
                    {
                        if (lastZPostion != 0)
                        {
                            strideLengthDistance = lastZPostion - (float)angleData.HeelLeft3DZ;
                        }

                        lastZPostion = (float)angleData.HeelLeft3DZ;
                        var key = angleData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.AngleAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.AngleAtFootStrikingTime.Add(key, (float)angleData.AnkleHipLeftAbductionDifference);
                    }
                    else
                    {
                       continue;
                    }
                    
                    if (distanceData != null)
                    {
                        var key = distanceData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.DistanceAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.DistanceAtFootStrikingTime.Add(key, (float)distanceData.VarusValgusLeft);
                    }

                    if (kneeData != null)
                    {
                        var key = kneeData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.KneeAbductionAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.KneeAbductionAtFootStrikingTime.Add(key, (float)kneeData.HipLeftAbduction);
                    }

                    if (pelvisData != null)
                    {
                        var key = pelvisData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.PelvisAngleAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.PelvisAngleAtFootStrikingTime.Add(key, (float)pelvisData.PelvisAngle);
                    }

                    // Use the middle item for heel press/standing detection
                  
                    
                    if (middleItem != null)
                    {
                        string timeKey = middleItem.TimeOfReading.ToString();

                        var heelPressDetectionBody = new HeelPressDetectionBody()
                        {
                            angleDifferenceValue = (float)middleItem.AnkleHipLeftAbductionDifference,
                            pelvisAngleValue = (float)middleItem.PelvisAngle,
                            kneeAbductionValue = (float)middleItem.HipLeftAbduction,
                            distanceValue = (float)middleItem.VarusValgusLeft,
                            nameOfTheFoot = "left foot",
                            varusValgusValue = (float)middleItem.VarusValgusLeft,
                            TimeOfHeelPressed = timeKey
                        };

                       

                        ReferenceManager.instance.heelPressDetectionBodies.Add(heelPressDetectionBody);
                        
                    }
                    var firstitem = timebasedReadings.Skip(1).FirstOrDefault(x=>x.AnkleHipRightAbductionDifference != null);
                    var standingDetectionBody = new StandingDetectionBody()
                    {
                        angleDifferenceValue = (float)firstitem.AnkleHipLeftAbductionDifference,
                        pelvisAngleValue = (float)firstitem.PelvisAngle,
                        kneeAbductionValue = (float)firstitem.HipLeftAbduction,
                        distanceValue = (float)firstitem.VarusValgusLeft,
                        nameOfTheFoot = "left foot",
                        varusValgusValue = (float)firstitem.VarusValgusLeft,
                        TimeofStanding = firstitem.TimeOfReading
                    };
                    ReferenceManager.instance.standingDetectionBodies.Add(standingDetectionBody);
                    if (!ReferenceManager.instance.StrideLengthAtFootStrikingTime.ContainsKey(pelvisData.TimeOfReading))
                        ReferenceManager.instance.StrideLengthAtFootStrikingTime.Add(pelvisData.TimeOfReading, strideLengthDistance);
                }
            }
            else
            {
                foreach (var item in timebasedReadings)
                {
                   

                    TimeBasedReadingRequest angleData;
                    TimeBasedReadingRequest distanceData;
                    TimeBasedReadingRequest kneeData;
                    TimeBasedReadingRequest pelvisData;
                    if (item.AnkleRight3DZ < item.AnkleLeft3DZ && Mathf.Abs((float)item.AnkleLeft3DZ - (float)item.AnkleRight3DZ) > footDistanceThreshold && float.Parse(item.TimeOfReading)>1.0)
                    {
                        angleData = item;
                        distanceData = item;
                        kneeData = item;
                        pelvisData = item;
                    }
                    else
                    {
                        continue;
                    }
                    // Use the middle item for heel press/standing detection
                    var middleItem = timebasedReadings.FirstOrDefault(x => x.AnkleHipLeftAbductionDifference != null && float.Parse(x.TimeOfReading)> float.Parse(angleData.TimeOfReading) && Mathf.Abs((float)x.AnkleRight3DZ - (float)x.AnkleLeft3DZ)<=footDistanceThreshold/1.5f);
                    if (middleItem == null)
                    {
                        continue;
                    }
                    if (rightDone < 2)
                    {
                        rightDone += 1;
                        continue;
                    }
                    if (angleData != null)
                    {
                        if (lastZPostion != 0)
                        {
                            strideLengthDistance = lastZPostion - (float)angleData.HeelRight3DZ;
                        }

                        lastZPostion = (float)angleData.HeelRight3DZ;
                        var key = angleData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.AngleAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.AngleAtFootStrikingTime.Add(key, (float)angleData.AnkleHipRightAbductionDifference);
                    }
                    else
                    {
                       
                        continue;
                    }

                    if (distanceData != null)
                    {
                        var key = distanceData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.DistanceAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.DistanceAtFootStrikingTime.Add(key, (float)distanceData.VarusValgusRight);
                    }

                    if (kneeData != null)
                    {
                        var key = kneeData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.KneeAbductionAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.KneeAbductionAtFootStrikingTime.Add(key, (float)kneeData.HipRightAbduction);
                    }

                    if (pelvisData != null)
                    {
                        var key = pelvisData.TimeOfReading.ToString();
                        if (!ReferenceManager.instance.PelvisAngleAtFootStrikingTime.ContainsKey(key))
                            ReferenceManager.instance.PelvisAngleAtFootStrikingTime.Add(key, (float)pelvisData.PelvisAngle);
                    }

                    
                    
                    if (middleItem != null)
                    {
                        string timeKey = middleItem.TimeOfReading.ToString();

                        var heelPressDetectionBody = new HeelPressDetectionBody()
                        {
                            angleDifferenceValue = (float)middleItem.AnkleHipRightAbductionDifference,
                            pelvisAngleValue = (float)middleItem.PelvisAngle,
                            kneeAbductionValue = (float)middleItem.HipRightAbduction,
                            distanceValue = (float)middleItem.VarusValgusRight,
                            nameOfTheFoot = "Right foot",
                            varusValgusValue = (float)middleItem.VarusValgusRight,
                            TimeOfHeelPressed = timeKey
                        };

                       

                        ReferenceManager.instance.heelPressDetectionBodies.Add(heelPressDetectionBody);
                        
                    }
                    var firstitem = timebasedReadings.Skip(1).FirstOrDefault(x=>x.AnkleHipRightAbductionDifference != null);
                    var standingDetectionBody = new StandingDetectionBody()
                    {
                        angleDifferenceValue = (float)firstitem.AnkleHipRightAbductionDifference,
                        pelvisAngleValue = (float)firstitem.PelvisAngle,
                        kneeAbductionValue = (float)firstitem.HipRightAbduction,
                        distanceValue = (float)firstitem.VarusValgusRight,
                        nameOfTheFoot = "Right foot",
                        varusValgusValue = (float)firstitem.VarusValgusRight,
                        TimeofStanding = firstitem.TimeOfReading
                    };
                    ReferenceManager.instance.standingDetectionBodies.Add(standingDetectionBody);
                    if (!ReferenceManager.instance.StrideLengthAtFootStrikingTime.ContainsKey(pelvisData.TimeOfReading))
                        ReferenceManager.instance.StrideLengthAtFootStrikingTime.Add(pelvisData.TimeOfReading, strideLengthDistance);
                }
            }



            assigned = true;
            StopButon.onClick.Invoke();
            ReferenceManager.instance.placeHeelDetectionValues = false;
            processingNotifier.NotifierText.text = "Select An Option";
            processingNotifier.gameObject.SetActive(true);
            processingNotifier.LoadingFill.transform.parent.gameObject.SetActive(false);
            processingNotifier.buttons.SetActive(true);
            ReferenceManager.instance.videoPlayerView.VideoPlayer.Speed = 1f;
            if (leftLeg)
            {
                isDoneWithLeft = true;
            }
            else if (rightLeg)
            {
                isDoneWithRight = true;
            }

            ReferenceManager.instance.canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        if (ReferenceManager.instance.AngleAtFootStrikingTime.Count == 0)
        {
            ReferenceManager.instance.PopupManager.Show("High Threshold","Please lower the threshold (because subject is taking short steps) and try again","",okPressed:
                () =>
                {
                    Debug.Log("Canceled");
                                   
                },"Ok");
            cancelButton.onClick.Invoke();
        }
        return;
        var ankleLeft = LightbuzzBody.Joints[JointType.AnkleLeft];
        var ankleRight = LightbuzzBody.Joints[JointType.AnkleRight];

        if (ankleLeft == null || ankleRight == null)
        {
            return;
        }
      
        
        // float distance = Vector3.Distance(
        //     jointForStrideLengthL.Position3D,
        //     jointForStrideLengthR.Position3D
        // );
        //
        // if (!footDistances.Contains(distance))
        // {
        //     footDistances.Add(distance);
        //     footDistances.Sort();
        // }

        if (leftLeg)
        {
            
           
            if (pelvisBaseHeight == 0)
            {
                pelvisBaseHeight = Mathf.RoundToInt(ankleLeft.Position3D.Z);
                previousRecordedZofStep = Mathf.RoundToInt(ankleLeft.Position3D.Z);
            }

           
            if (Mathf.RoundToInt(ankleLeft.Position3D.Z) < pelvisBaseHeight && Mathf.RoundToInt(ankleLeft.Position3D.Z) <previousRecordedZofStep && ankleLeft.Position3D.Z < ankleRight.Position3D.Z && Math.Round(ankleLeft.Position3D.Z%1,1) < 0.9f)
            {
                previousRecordedZofStep = (int)ankleLeft.Position3D.Z;
                if (firstStepIgnored)
                {
                    Debug.Log("FootDetected");
                    StepButon.onClick.Invoke();
                }
                else
                {
                    Debug.Log("Ignored");
                    firstStepIgnored = true;
                }
            }
            
        }
        else
        {
            if (pelvisBaseHeight == 0)
            {
                pelvisBaseHeight = Mathf.RoundToInt(ankleRight.Position3D.Z);
                previousRecordedZofStep = Mathf.RoundToInt(ankleRight.Position3D.Z);
            }
           
            if (Mathf.RoundToInt(ankleRight.Position3D.Z) < pelvisBaseHeight && Mathf.RoundToInt(ankleRight.Position3D.Z) <previousRecordedZofStep && ankleRight.Position3D.Z < ankleLeft.Position3D.Z && Math.Round(ankleRight.Position3D.Z%1,1) < 0.9f)
            {
                previousRecordedZofStep = (int)ankleRight.Position3D.Z;
                if (firstStepIgnored)
                {
                    Debug.Log("FootDetected");
                    StepButon.onClick.Invoke();
                }
                else
                {
                    Debug.Log("Ignored");
                    firstStepIgnored = true;
                }
            }
        }
        
        
    }

    Coroutine coroutine;
    public Vector3 previousPosition;
    public bool firstStepIgnored;
    
    
    public IEnumerator DetectFootOnGround()
    {
        processingNotifier.NotifierText.text = "Detecting Foot On Ground...";
        processingNotifier.gameObject.SetActive(true);
       
        // if(footDistances.Count == 0)
        // {
        //     ReferenceManager.instance.graphManagers.ForEach(x=>x.MySineWave.isVideoDoneLoading = false);
        //     coroutine = null;
        //     yield break;
        // }
        if (ReferenceManager.instance.videoPlayerView.VideoPlayer.IsPaused)
        {
            // Debug.Log("Video Is paused");
            coroutine = null;
            yield break;
        }

        var jointForStrideLengthL = LightbuzzBody.Joints[JointType.AnkleLeft];
        var jointForStrideLengthR = LightbuzzBody.Joints[JointType.AnkleRight];
        
        // if (
        //     jointForStrideLengthL == null
        //     || !jointForStrideLengthL.gameObject.activeSelf
        //     || jointForStrideLengthR == null
        //     || !jointForStrideLengthR.gameObject.activeSelf
        // )
        // {
        //     jointForStrideLengthL?.ShockWaveEffect.SetActive(false);
        //     jointForStrideLengthR?.ShockWaveEffect.SetActive(false);
        //     coroutine = null;
        //     yield break;
        // }
        // Debug.Log(
        //     $"Left Z Pos: {jointForStrideLengthL.Position3D.z}\nRight Z Pos: {jointForStrideLengthR.Position3D.z}"
        // );

        // yield return new WaitForEndOfFrame();
        if (leftLeg)
        {
            // if (isDoneWithLeft) //to ignore the socond time reading is taken
            // {
            //     jointForStrideLengthL.ShockWaveEffect.SetActive(false);
            //     jointForStrideLengthR.ShockWaveEffect.SetActive(false);
            //     coroutine = null;
            //     yield break;
            // }
            
            if (jointForStrideLengthL.Position3D.Z > jointForStrideLengthR.Position3D.Z || previousPosition.z.Equals(jointForStrideLengthL.Position3D.Z))
            {
                
                coroutine = null;
                yield break;
            }
            // debugDistance = Vector3.Distance(
            //     jointForStrideLengthL.Position3D,
            //     jointForStrideLengthR.Position3D
            // );
            
            yield return null;
            if (
                possibleFootStepPoints.Contains(jointForStrideLengthL.Position3D.Y)
            )
            {
                Debug.Log("Foot Detected");
                StepButon.onClick.Invoke();
                // if (firstStepIgnored)
                // {
                //     Debug.Log("Foot Detected");
                //     StepButon.onClick.Invoke();
                // }
                // else
                // {
                //     firstStepIgnored = true;
                // }
                // yield return new WaitForSeconds(0.1f);
            }
            else
            {
                
                previousPosition = jointForStrideLengthL.Position3D;
            }
        }
        else if(rightLeg)
        {
            // if (isDoneWithRight)
            // {
            //     jointForStrideLengthL.ShockWaveEffect.SetActive(false);
            //     jointForStrideLengthR.ShockWaveEffect.SetActive(false);
            //     coroutine = null;
            //     yield break;
            // }
            
            if (
                jointForStrideLengthR.Position3D.Z > jointForStrideLengthL.Position3D.Z || previousPosition.z.Equals(jointForStrideLengthR.Position3D.Z)
                )
            {
                // Debug.Log("Because z is less or behind");
                coroutine = null;
                yield break;
            }
           
            yield return null;
            if (
                possibleFootStepPoints.Contains(jointForStrideLengthR.Position3D.Y)
            )
            {
                Debug.Log("Foot Detected");
                StepButon.onClick.Invoke();
                // if (firstStepIgnored)
                // {
                //     Debug.Log("Foot Detected");
                //     StepButon.onClick.Invoke();
                // }
                // else
                // {
                //     firstStepIgnored = true;
                // }
                // yield return new WaitForSeconds(0.1f);
            }
            else
            {
                previousPosition = jointForStrideLengthR.Position3D;
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
            jointForStrideLengthL = researchProjectCompleteBodyDatas.FirstOrDefault(x => x.gameObject.name == JointType.AnkleLeft.ToString());
            if (
                !GeneralStaticManager.GraphsReadings.ContainsKey(MeasurementType.HipAnkleHipKneeLeftAbductionDifference.ToString())
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

        // Debug.Log("Detected");

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
            strideLengthDistance = Vector3.Distance(newFootPosition, lastFootPosition);
            strideLengthDistance = Mathf.Round(strideLengthDistance * 100.0f) * 0.01f;
            strideLengthNotifier.text = $"Stride Length\n{strideLengthDistance}m";

            footCount += 1;
            TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
                $"Set Step {footCount + 1}";
        }
    }

    private float strideLengthDistance;
    public void AddTimerReading()
    {
        string footstrikesAtTime = "";
        double totalseconds=0;
        if (ReferenceManager.instance.videoPlayerView.gameObject.activeSelf)
        {
            footstrikesAtTime = ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds.ToString();
            totalseconds = ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds;
            if (!footStrikeAtTimes.Contains(footstrikesAtTime))
                footStrikeAtTimes.Add(footstrikesAtTime);
            footStrikeAtTimes.Sort();
        }
        else
        {
            // footstrikesAtTime = ReferenceManager.instance.Timer;
            // if (!footStrikeAtTimes.Contains(footstrikesAtTime))
            //     footStrikeAtTimes.Add(footstrikesAtTime);
            // footStrikeAtTimes.Sort();
        }
        float maxAngle;
        float maxDistance;
        float currentAngle;
        float curreentDistance;
        float currentKneeAbd;
        float currentAnkleAbd;
        float currentPelvAngle;
        float currentStrideLength;
        if (leftLeg)
        {
            maxAngle = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipAnkleHipKneeLeftAbductionDifference.ToString()]
                .Max();

            // currentAngle = abdDiffAtTime.ContainsKey(footstrikesAtTime)? abdDiffAtTime[footstrikesAtTime]:0;
            currentAngle = (float)timebasedReadings.FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading),5) == Math.Round(totalseconds,5))
                .AnkleHipLeftAbductionDifference;
            // Debug.Log($"{footstrikesAtTime} and {currentAngle}");
            // currentKneeAbd = kneeAbdAtTime.ContainsKey(footstrikesAtTime)? kneeAbdAtTime[footstrikesAtTime]:0;
            currentKneeAbd = (float)timebasedReadings
                .FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading), 5) == Math.Round(totalseconds, 5))
                .HipLeftAbduction;

            // currentAnkleAbd = ankleAbdAtTime.ContainsKey(footstrikesAtTime)? ankleAbdAtTime[footstrikesAtTime]:0;
            // currentAnkleAbd = currentAnkleAbd == 0 ? ReferenceManager.instance.angleManager._angles[MeasurementType.AnkleLeftAbduction].Angle : currentAnkleAbd;

            // currentPelvAngle = pelvisAngleAtTime.ContainsKey(footstrikesAtTime)? pelvisAngleAtTime[footstrikesAtTime]:0;
            currentPelvAngle = (float)timebasedReadings
                .FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading), 5) == Math.Round(totalseconds, 5))
                .PelvisAngle;
            
            currentStrideLength = strideLengthAtTime.ContainsKey(footstrikesAtTime) ? strideLengthAtTime[footstrikesAtTime]:strideLengthDistance;
        }
        else
        {
            maxAngle = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipAnkleHipKneeRightAbductionDifference.ToString()]
                .Max();

            // currentAngle = abdDiffAtTime.ContainsKey(footstrikesAtTime)? abdDiffAtTime[footstrikesAtTime]:0;
            Debug.Log($"{totalseconds} and nothing");
            currentAngle =  (float)timebasedReadings.FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading),5) == Math.Round(totalseconds,5))
                .AnkleHipRightAbductionDifference;
            Debug.Log($"{footstrikesAtTime} and {currentAngle}");
            
            // currentKneeAbd = kneeAbdAtTime.ContainsKey(footstrikesAtTime)? kneeAbdAtTime[footstrikesAtTime]:0;
            currentKneeAbd =(float)timebasedReadings
                .FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading), 5) == Math.Round(totalseconds, 5))
                .HipRightAbduction;

            // currentAnkleAbd = ankleAbdAtTime.ContainsKey(footstrikesAtTime)? ankleAbdAtTime[footstrikesAtTime]:0;
            // currentAnkleAbd = currentAnkleAbd == 0 ? ReferenceManager.instance.angleManager._angles[MeasurementType.AnkleRightAbduction].Angle : currentAnkleAbd;
            
            // currentPelvAngle = pelvisAngleAtTime.ContainsKey(footstrikesAtTime)? pelvisAngleAtTime[footstrikesAtTime]:0;
            currentPelvAngle = (float)timebasedReadings
                .FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading), 5) == Math.Round(totalseconds, 5))
                .PelvisAngle;
            
            currentStrideLength = strideLengthAtTime.ContainsKey(footstrikesAtTime) ? strideLengthAtTime[footstrikesAtTime]:strideLengthDistance;
        }
        if (leftLeg)
        {
            maxDistance = GeneralStaticManager
                .GraphsReadings[MeasurementType.VarusValgusLeftAngleDistance.ToString()]
                .Max();

            // curreentDistance = varValAtTime.ContainsKey(footstrikesAtTime)? varValAtTime[footstrikesAtTime]:0;
            curreentDistance =  (float)timebasedReadings
                .FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading), 5) == Math.Round(totalseconds, 5))
                .VarusValgusLeft;
        }
        else
        {
            maxDistance = GeneralStaticManager
                .GraphsReadings[MeasurementType.VarusValgusRightAngleDistance.ToString()]
                .Max();

            // curreentDistance = varValAtTime.ContainsKey(footstrikesAtTime)? varValAtTime[footstrikesAtTime]:0;
            curreentDistance =  (float)timebasedReadings
                .FirstOrDefault(x => Math.Round(float.Parse(x.TimeOfReading), 5) == Math.Round(totalseconds, 5))
                .VarusValgusRight;
        }
        if (!ReferenceManager.instance.KneeAbductionAtFootStrikingTime.Any(x=> x.Key == footstrikesAtTime))
            ReferenceManager.instance.KneeAbductionAtFootStrikingTime.Add(footstrikesAtTime, currentKneeAbd);
        
        if (!ReferenceManager.instance.StrideLengthAtFootStrikingTime.Any(x=> x.Key == footstrikesAtTime) && currentStrideLength !=0)
            ReferenceManager.instance.StrideLengthAtFootStrikingTime.Add(footstrikesAtTime, currentStrideLength);
        


        // if (!ReferenceManager.instance.AnkleAbductionAtFootStrikingTime.Any(x=> x.Key == footstrikesAtTime))
        //     ReferenceManager.instance.AnkleAbductionAtFootStrikingTime.Add(footstrikesAtTime, currentAnkleAbd);

        if (!ReferenceManager.instance.PelvisAngleAtFootStrikingTime.Any(x=> x.Key == footstrikesAtTime))
        ReferenceManager.instance.PelvisAngleAtFootStrikingTime.Add(footstrikesAtTime, currentPelvAngle);

        if (!ReferenceManager.instance.maxAngleAtFootStrikingTime.Any(x=> x.Key == footstrikesAtTime))
            ReferenceManager.instance.maxAngleAtFootStrikingTime.Add(footstrikesAtTime, maxAngle);
        // else
        //     ReferenceManager.instance.maxAngleAtFootStrikingTime[footstrikesAtTime] = maxAngle;
        if (!ReferenceManager.instance.maxDistanceAtFootStrikingTime.Any(x=>footstrikesAtTime==x.Key))
            ReferenceManager.instance.maxDistanceAtFootStrikingTime.Add(
                footstrikesAtTime,
                maxDistance
            );
        // else
        //     ReferenceManager.instance.maxDistanceAtFootStrikingTime[footstrikesAtTime] =
        //         maxDistance;
        if (!ReferenceManager.instance.AngleAtFootStrikingTime.Any(x=>footstrikesAtTime.Substring(0,5)==x.Key.Substring(0,5)))
            ReferenceManager.instance.AngleAtFootStrikingTime.Add(footstrikesAtTime, currentAngle);
        else
            ReferenceManager.instance.AngleAtFootStrikingTime[ReferenceManager.instance.AngleAtFootStrikingTime.FirstOrDefault(x=>footstrikesAtTime.Substring(0,5)==x.Key.Substring(0,5)).Key] = currentAngle;
        if (!ReferenceManager.instance.DistanceAtFootStrikingTime.Any(x=>footstrikesAtTime.Substring(0,5)==x.Key.Substring(0,5)))
            ReferenceManager.instance.DistanceAtFootStrikingTime.Add(
                footstrikesAtTime,
                curreentDistance
            );
        else
            ReferenceManager.instance.DistanceAtFootStrikingTime[ReferenceManager.instance.DistanceAtFootStrikingTime.FirstOrDefault(x=>footstrikesAtTime.Substring(0,5)==x.Key.Substring(0,5)).Key] =
                curreentDistance;
        
        ReferenceManager.instance.AngleAtFootStrikingTime  =  ReferenceManager.instance.AngleAtFootStrikingTime.OrderBy(x => x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.maxAngleAtFootStrikingTime = ReferenceManager.instance.maxAngleAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.maxDistanceAtFootStrikingTime = ReferenceManager.instance.maxDistanceAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.DistanceAtFootStrikingTime =ReferenceManager.instance.DistanceAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.heelPressDetectionBodies = ReferenceManager.instance.heelPressDetectionBodies.OrderBy(x=>x.TimeOfHeelPressed).ToList();
        ReferenceManager.instance.KneeAbductionAtFootStrikingTime = ReferenceManager.instance.KneeAbductionAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.AnkleAbductionAtFootStrikingTime = ReferenceManager.instance.AnkleAbductionAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.PelvisAngleAtFootStrikingTime = ReferenceManager.instance.PelvisAngleAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
        ReferenceManager.instance.StrideLengthAtFootStrikingTime = ReferenceManager.instance.StrideLengthAtFootStrikingTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x => x.Value);
    }

    public void Reset()
    {
        footCount = 0;
        TriggerButton.transform.GetChild(0).GetComponent<TMP_Text>().text =
            $"Set Step {footCount + 1}";

        ReferenceManager.instance.maxAngleAtFootStrikingTime.Clear();
        ReferenceManager.instance.maxDistanceAtFootStrikingTime.Clear();
        ReferenceManager.instance.heelPressDetectionBodies.Clear();
        ReferenceManager.instance.videoPlayingCount = 0;
        isStarted = false;
        ReferenceManager.instance.canvas.renderMode = RenderMode.ScreenSpaceCamera;
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
