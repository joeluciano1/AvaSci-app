using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightBuzz.BodyTracking;
using TMPro;
using UnityEngine;

public class ResearchMeasurementManager : MonoBehaviour
{
    public static ResearchMeasurementManager instance;
    public Body LightbuzzBody;

    public List<TempBodyDataSaver> tempBodyDataSavers = new List<TempBodyDataSaver>();
    public List<ResearchProjectCompleteBodyData> researchProjectCompleteBodyDatas = new List<ResearchProjectCompleteBodyData>();
    public List<ResearchProjectCompleteBodyData> SelectedBodyDatas = new List<ResearchProjectCompleteBodyData>();
    public ResearchProjectCompleteBodyData bodyDataPrefab;
    public Transform SkeletonMangerTransform;
    public float selectedJoints;
    public float distance;

    public TMP_Text DistanceNotifier;

    private void Awake()
    {
        instance = this;
    }
    [ContextMenu("Test")]
    public void Test()
    {
        GameObject go = GameObject.Find("Scripts/Managers");
        Debug.Log(go.transform.Find("ResearchMeasurementManager").name);
    }
    public void LateUpdate()
    {
        if (LightbuzzBody != null)
        {
            foreach (var item in LightbuzzBody.Joints)
            {
                Debug.Log(item.Value.Type.ToString());
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

                    var alreadyAdded = tempBodyDataSavers.FirstOrDefault(x => x.bodyJointType == item.Value.Type);
                    if (alreadyAdded != null)
                    {
                        if (tempBodyDataSaver.PointPosition == null)
                        {
                            tempBodyDataSavers.Remove(alreadyAdded);

                        }
                        else
                        {
                            tempBodyDataSavers[tempBodyDataSavers.IndexOf(alreadyAdded)] = tempBodyDataSaver;
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
                    var alreadyAdded = tempBodyDataSavers.FirstOrDefault(x => x.bodyJointType == item.Value.Type);
                    if (alreadyAdded != null)
                        tempBodyDataSavers.Remove(alreadyAdded);
                }

            }

            foreach (var item in tempBodyDataSavers)
            {
                var alreadyAdded = researchProjectCompleteBodyDatas.FirstOrDefault(x => x.name == item.bodyJointType.ToString());
                if (alreadyAdded == null)
                {
                    ResearchProjectCompleteBodyData researchProjectCompleteBodyData = Instantiate(bodyDataPrefab, bodyDataPrefab.transform.parent);
                    researchProjectCompleteBodyData.gameObject.SetActive(true);
                    researchProjectCompleteBodyData.gameObject.name = item.bodyJointType.ToString();
                    researchProjectCompleteBodyData.PositionShowCase.text = $"{item.bodyJointType}\n{Mathf.Round(item.Position3D.x * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.y * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.z * 100.0f) * 0.01f}";
                    researchProjectCompleteBodyData.transform.position = item.PointPosition.position;
                    researchProjectCompleteBodyData.Position3D = item.Position3D;
                    researchProjectCompleteBodyDatas.Add(researchProjectCompleteBodyData);
                }
                else
                {

                    alreadyAdded.PositionShowCase.text = $"{item.bodyJointType}\n{Mathf.Round(item.Position3D.x * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.y * 100.0f) * 0.01f}, {Mathf.Round(item.Position3D.z * 100.0f) * 0.01f}";
                    alreadyAdded.transform.position = item.PointPosition.position;
                    alreadyAdded.Position3D = item.Position3D;
                    alreadyAdded.gameObject.SetActive(true);
                }
            }
            List<ResearchProjectCompleteBodyData> ObjectsToDestroy = new List<ResearchProjectCompleteBodyData>();
            foreach (var item in researchProjectCompleteBodyDatas)
            {
                if (!tempBodyDataSavers.Any(x => x.bodyJointType.ToString() == item.gameObject.name))
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
            distance = Vector3.Distance(SelectedBodyDatas[0].Position3D, SelectedBodyDatas[1].Position3D);
            distance = Mathf.Round(distance * 100.0f) * 0.01f;
            // DistanceNotifier.transform.parent.GetComponent<RectTransform>().anchoredPosition = (SelectedBodyDatas[0].GetComponent<RectTransform>().anchoredPosition + SelectedBodyDatas[1].GetComponent<RectTransform>().anchoredPosition) / 2;
            DistanceNotifier.text = "Distance Between Selected Points: " + distance.ToString() + " meters";
        }
        else
        {
            DistanceNotifier.text = "";
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