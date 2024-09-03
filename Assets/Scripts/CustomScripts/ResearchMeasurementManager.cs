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
    public float leftAngleValue;
	public float rightAngleValue;
	public float leftDisValue;
	public float rightDisValue;
    private void Awake()
    {
        instance = this;
    }

    public void StartReading()
    {
        isStarted = true;
        ReferenceManager.instance.videoPlayingCount = 0;
        if (!informationShown)
        {
            ReferenceManager.instance.PopupManager.Show("Information", "Please run the video completely once again to complete the foot detection record. Thanks!");
            informationShown = true;
        }
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
        // DetectFootFullyPressed();
        AddAllTimeReadings();
        GenerateAvatar();
        DetectIfCubePassed();
        // FootFullyPressedNewDetection();
    }
    public void DetectIfCubePassed(){
        var cubeLeft = cubes.FirstOrDefault(x => x.gameObject.name == "AnkleLeft");
        var cubeRight = cubes.FirstOrDefault(x => x.gameObject.name == "AnkleRight");
        if(cubeLeft == null || cubeRight == null)
        {
            return;
        }
        if(Math.Abs(cubeLeft.transform.localPosition.z - cubeRight.transform.localPosition.z)<=0.02f){
            Debug.Log("Wajya Nai par phar lya");
             FootFullyPressedNewDetection(ReferenceManager.instance.TimeElapsedLightBuzz.text);
        }
    }
    public void AddAllTimeReadings()
    {
        if(ReferenceManager.instance.videoPlayerView.VideoPlayer.IsPaused && !ReferenceManager.instance.angleManager._angles.ContainsKey(MeasurementType.HipAnkleHipKneeLeftAbductionDifference)||!ReferenceManager.instance.angleManager._angles.ContainsKey(MeasurementType.VarusValgusLeftAngleDistance))
        {
            return;
        }    
        if(ReferenceManager.instance.videoPlayingCount <= 2){
            if(!abdDiffAtTime.ContainsKey(ReferenceManager.instance.TimeElapsedLightBuzz.text))
            {
                if(leftLeg )
                abdDiffAtTime.Add(ReferenceManager.instance.TimeElapsedLightBuzz.text, leftAngleValue);
                else if(rightLeg )
                abdDiffAtTime.Add(ReferenceManager.instance.TimeElapsedLightBuzz.text, rightAngleValue);
            }
            else
            {
                if(leftLeg )
                abdDiffAtTime[ReferenceManager.instance.TimeElapsedLightBuzz.text] = leftAngleValue;
                else if(rightLeg ) 
                abdDiffAtTime[ReferenceManager.instance.TimeElapsedLightBuzz.text] = rightAngleValue;
                
            }
            if(!varValAtTime.ContainsKey(ReferenceManager.instance.TimeElapsedLightBuzz.text))
            {
                if(leftLeg )
                varValAtTime.Add(ReferenceManager.instance.TimeElapsedLightBuzz.text, leftDisValue);
                else if(rightLeg)
                varValAtTime.Add(ReferenceManager.instance.TimeElapsedLightBuzz.text, rightDisValue);
            }
            else
            {
                if(leftLeg )
                 varValAtTime[ReferenceManager.instance.TimeElapsedLightBuzz.text] = leftDisValue;
                 else if (rightLeg)
                 varValAtTime[ReferenceManager.instance.TimeElapsedLightBuzz.text] = rightDisValue;
            }
            var toBeRemoved = abdDiffAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemoved)
                abdDiffAtTime.Remove(item.Key);
                
            var toBeRemovedVar = varValAtTime.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value);
            foreach (var item in toBeRemovedVar)
                varValAtTime.Remove(item.Key);

            abdDiffAtTime = abdDiffAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value); 
            varValAtTime = varValAtTime.OrderBy(x=>x.Key).ToDictionary(x=>x.Key,x=>x.Value);  
        }
        if(ReferenceManager.instance.videoPlayingCount == 2){
             processingNotifier.NotifierText.text = "Gathering Heel Pressed Data...";
           
            processingNotifier.gameObject.SetActive(true);
        }
        if(ReferenceManager.instance.videoPlayingCount >2 &&footStrikeAtTimes.Count!=0)
        {
            // processingNotifier.NotifierText.text = "Processing Heel Pressed Data...";
            // processingNotifier.gameObject.SetActive(true);
            // ReferenceManager.instance.placeHeelDetectionValues = true;
            // if (ReferenceManager.instance.videoPlayingCount > 3)
            // {
                processingNotifier.NotifierText.text = "";
                ReferenceManager.instance.placeHeelDetectionValues = false;
                processingNotifier.gameObject.SetActive(false);
            // }
            // if (ReferenceManager.instance.videoPlayingCount == 3)
            // {
            //     List<float> floatValues = ReferenceManager.instance.heelPressDetectionBodies.Select(x => float.Parse(x.TimeOfHeelPressed)).ToList();
            //     HeelPressDetectionBody videoAtSavedValue = ReferenceManager.instance.heelPressDetectionBodies.FirstOrDefault(x => GeneralStaticManager.ClosestTo(floatValues, (float)ReferenceManager.instance.videoPlayerView.VideoPlayer.TimeElapsed.TotalSeconds) == float.Parse(x.TimeOfHeelPressed));

            //     PutGaitValuesInDetectedTime(videoAtSavedValue);
            // }
        }
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
   public void FootFullyPressedNewDetection(string timeDetectedOn = "")
   {
        if(footCount == 0)
        {
            return;
        }
        if(ReferenceManager.instance.videoPlayingCount ==2 &&footStrikeAtTimes.Count!=0)
        {
           
            
                // float time1 = footStrikeAtTimes[i];
                // float time2 = 0;
                // if (i < footStrikeAtTimes.Count-1)
                // {
                //     time2 = footStrikeAtTimes[i + 1];
                // }
                // else if(i == footStrikeAtTimes.Count-1 && i!=0)
                // {
                //     time2 = footStrikeAtTimes[i - 1];
                // }
                string finalValue = timeDetectedOn;
                var theFloatsABD = abdDiffAtTime.Select(x => x.Key).ToList();
                var varDis = varValAtTime.Select(x => x.Key).ToList();

                var abdDict = abdDiffAtTime.FirstOrDefault(x => finalValue==x.Key);
                var varDict = varValAtTime.FirstOrDefault(x => finalValue==x.Key);

                Debug.Log("Adding " + abdDict.Value + " at " + abdDict.Key + " With Float Value " + finalValue);
                HeelPressDetectionBody heelPressDetectionBody = new HeelPressDetectionBody()
                {
                    TimeOfHeelPressed = finalValue.ToString(),
                    angleDifferenceValue = abdDict.Value,
                    distanceValue = varDict.Value,
                    nameOfTheFoot = leftLeg? "Left Leg":"Right Leg"
                };
                if(!ReferenceManager.instance.heelPressDetectionBodies.Any(x=>x.TimeOfHeelPressed == heelPressDetectionBody.TimeOfHeelPressed))
                    ReferenceManager.instance.heelPressDetectionBodies.Add(heelPressDetectionBody);

                ReferenceManager.instance.heelPressDetectionBodies = ReferenceManager.instance.heelPressDetectionBodies.OrderBy(x => x.TimeOfHeelPressed).ToList();
               
        }
       
    }

    public void PutGaitValuesInDetectedTime(HeelPressDetectionBody videoAtSavedValue)
    {
        if (videoAtSavedValue != null)
        {
            // ReferenceManager.instance.PauseTheVideo();
            Debug.Log($"Video Value: {videoAtSavedValue.TimeOfHeelPressed}\n Timer Value: {ReferenceManager.instance.TimeElapsedLightBuzz.text}");
            
            if (leftLeg)
            {
                // videoAtSavedValue.TimeOfHeelPressed = float.Parse(videoAtSavedValue.TimeOfHeelPressed).ToString(@"hh\:mm\:ss\:fff");
                videoAtSavedValue.Subject = GeneralStaticManager.GlobalVar["Subject"];
                videoAtSavedValue.nameOfTheFoot = "Left Leg";
                videoAtSavedValue.angleDifferenceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.HipAnkleHipKneeLeftAbductionDifference].Angle;
                videoAtSavedValue.distanceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.VarusValgusLeftAngleDistance].Angle;
            }
            if (rightLeg)
            {
                // videoAtSavedValue.TimeOfHeelPressed = float.Parse(videoAtSavedValue.TimeOfHeelPressed).ToString(@"hh\:mm\:ss\:fff");
                videoAtSavedValue.Subject = GeneralStaticManager.GlobalVar["Subject"];
                videoAtSavedValue.nameOfTheFoot = "Right Leg";
                videoAtSavedValue.angleDifferenceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.HipAnkleHipKneeRightAbductionDifference].Angle;
                videoAtSavedValue.distanceValue = ReferenceManager.instance.angleManager._angles[MeasurementType.VarusValgusRightAngleDistance].Angle;
            }
              
        }
       
    }

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
    public List<float> footDistances = new List<float>();
    // public List<float> footZDistances = new List<float>();

    public void RecordFoots()
    {
        processingNotifier.NotifierText.text = "Reading Video Data...";
        processingNotifier.gameObject.SetActive(true);
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
        // float zDistance = Math.Abs(jointForStrideLengthL.Position3D.z - jointForStrideLengthR.Position3D.z);
        if (!footDistances.Contains(distance))
        {
            footDistances.Add(distance);
            footDistances.Sort();
        }
        // if(!footZDistances.Contains(zDistance)){
        //     footZDistances.Add(zDistance);
        //     footZDistances.Sort();
        // }
    }

    Coroutine coroutine;
    public Vector3 previousPosition;
    
    public IEnumerator DetectFootOnGround()
    {
        processingNotifier.NotifierText.text = "Detecting Foot On Ground...";
        processingNotifier.gameObject.SetActive(true);
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
        string footstrikesAtTime = "";
        if (ReferenceManager.instance.videoPlayerView.gameObject.activeSelf)
        {
            footstrikesAtTime = ReferenceManager.instance.TimeElapsedLightBuzz.text;
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
        if (leftLeg)
        {
            maxAngle = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipAnkleHipKneeLeftAbductionDifference.ToString()]
                .Max();

            currentAngle = abdDiffAtTime[footstrikesAtTime];
            currentAngle = currentAngle == 0 ? ReferenceManager.instance.angleManager._angles[MeasurementType.HipAnkleHipKneeLeftAbductionDifference].Angle : currentAngle;
        }
        else
        {
            maxAngle = GeneralStaticManager
                .GraphsReadings[MeasurementType.HipAnkleHipKneeRightAbductionDifference.ToString()]
                .Max();

            currentAngle = abdDiffAtTime[footstrikesAtTime];
            currentAngle = currentAngle == 0 ? ReferenceManager.instance.angleManager._angles[MeasurementType.HipAnkleHipKneeRightAbductionDifference].Angle : currentAngle;
        }
        if (leftLeg)
        {
            maxDistance = GeneralStaticManager
                .GraphsReadings[MeasurementType.VarusValgusLeftAngleDistance.ToString()]
                .Max();

            curreentDistance = varValAtTime[footstrikesAtTime];
            curreentDistance = curreentDistance == 0 ? ReferenceManager.instance.angleManager._angles[MeasurementType.VarusValgusLeftAngleDistance].Angle : curreentDistance;
        }
        else
        {
            maxDistance = GeneralStaticManager
                .GraphsReadings[MeasurementType.VarusValgusRightAngleDistance.ToString()]
                .Max();

            curreentDistance = varValAtTime[footstrikesAtTime];
            curreentDistance = curreentDistance == 0 ? ReferenceManager.instance.angleManager._angles[MeasurementType.VarusValgusRightAngleDistance].Angle : curreentDistance;
        }
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
