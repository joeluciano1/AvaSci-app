using LightBuzz.AvaSci.Measurements;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LightBuzz.AvaSci.UI
{
    /// <summary>
    /// A visual component that allows the user to select a list of <see cref="MeasurementType"/>s to be displayed.
    /// </summary>
    public class MeasurementSelector : MonoBehaviour
    {
        [SerializeField] private GameObject _list;
        [SerializeField] public Toggle[] _toggles;
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private Image _arrow;

        private readonly HashSet<MeasurementType> _selectedMeasurements = new HashSet<MeasurementType>();

        private bool _listExpanded = false;

        /// <summary>
        /// Raised when the selection of measurements changes.
        /// </summary>
        public UnityEvent<MeasurementType[]> onMeasurementsChanged = new UnityEvent<MeasurementType[]>();

        private void Awake()
        {
            // Uncomment to load all measurements by deafult.

            // foreach (MeasurementType t in Enum.GetValues(typeof(MeasurementType)))
            // {
            //     if (t == MeasurementType.None) continue;
            //     _selectedMeasurements.Add(t);
            // }

            RaiseEvent();
        }

        /// <summary>
        /// Called when a <see cref="MeasurementToggle"/> is turned on or off.
        /// </summary>
        /// <param name="type">The <see cref="MeasurementType"/> to add or remove from the collection.</param>
        /// <param name="isOn">True to add the <see cref="MeasurementType"/> to the collection; false to remove it.</param>
        public void OnMeasurementSelected(MeasurementType type, bool isOn)
        {
            string itemName = Enum.GetName(typeof(MeasurementType), type);
            string jointName = itemName.Replace("Left", ",").Replace("Right", ",");
            Debug.Log("---------------------------" + jointName);


            if (isOn)
            {

                GraphManager alreadyPresentJointGraph = ReferenceManager.instance.graphManagers.FirstOrDefault(x => Enum.GetName(typeof(MeasurementType), x.JointType).Contains(jointName.Split(',')[0]) && Enum.GetName(typeof(MeasurementType), x.JointType).Contains(jointName.Split(',')[1]));

                if (alreadyPresentJointGraph == null)
                {
                    GraphManager graphManager = Instantiate(ReferenceManager.instance.GraphmanagerPrefab, ReferenceManager.instance.GraphmanagerPrefab.transform.parent);
                    graphManager.gameObject.SetActive(true);
                    graphManager.JointType = type;
                    //await System.Threading.Tasks.Task.Delay(5000);

                    // graphManager.Title.text = Enum.GetName(typeof(MeasurementType), type);
                    ReferenceManager.instance.graphManagers.Add(graphManager);
                    graphManager.MySineWave.graphChart.DataSource.GetCategoryFill(Enum.GetName(typeof(MeasurementType), type), out Material fillMat, out bool stretch);
                    graphManager.MySineWave.graphChart.DataSource.GetCategoryLine(Enum.GetName(typeof(MeasurementType), type), out Material lineMaterial, out double LineThickness, out ChartAndGraph.MaterialTiling lineTiling);
                    graphManager.MySineWave.graphChart.DataSource.GetCategoryPoint(Enum.GetName(typeof(MeasurementType), type), out Material pointMaterial, out double pointsize);

                    _selectedMeasurements.Add(type);

                    if (lineMaterial != null)
                    {
                        var colorLM = lineMaterial.GetColor("_Color");
                        colorLM = new Color(colorLM.r, colorLM.g, colorLM.b, 1f);

                        lineMaterial.SetColor("_Color", colorLM);
                    }

                    //if (fillMat != null)
                    //{
                    //    var colorFM = fillMat.GetColor("_ColorFrom");
                    //    colorFM = new Color(colorFM.r, colorFM.g, colorFM.b, 0.5f);

                    //    fillMat.SetColor("_ColorFrom", colorFM);

                    //    var colorTM = fillMat.GetColor("_ColorTo");
                    //    colorTM = new Color(colorTM.r, colorTM.g, colorTM.b, 0.1f);

                    //    fillMat.SetColor("_ColorTo", colorTM);
                    //}

                    if (pointMaterial != null)
                    {
                        var colorPM = pointMaterial.GetColor("_ColorFrom");
                        colorPM = new Color(colorPM.r, colorPM.g, colorPM.b, 1f);

                        pointMaterial.SetColor("_ColorFrom", colorPM);

                        var colorPMTo = pointMaterial.GetColor("_ColorTo");
                        colorPMTo = new Color(colorPMTo.r, colorPMTo.g, colorPMTo.b, 1f);

                        pointMaterial.SetColor("_ColorTo", colorPMTo);
                    }
                }
                else
                {

                    alreadyPresentJointGraph.SecondJointType = type;
                    //await System.Threading.Tasks.Task.Delay(5000);
                    //alreadyPresentJointGraph.Title.text += ", "+Enum.GetName(typeof(MeasurementType), type);

                    alreadyPresentJointGraph.MySineWave.graphChart.DataSource.GetCategoryFill(Enum.GetName(typeof(MeasurementType), type), out Material fillMat, out bool stretch);
                    alreadyPresentJointGraph.MySineWave.graphChart.DataSource.GetCategoryLine(Enum.GetName(typeof(MeasurementType), type), out Material lineMaterial, out double LineThickness, out ChartAndGraph.MaterialTiling lineTiling);
                    alreadyPresentJointGraph.MySineWave.graphChart.DataSource.GetCategoryPoint(Enum.GetName(typeof(MeasurementType), type), out Material pointMaterial, out double pointsize);

                    _selectedMeasurements.Add(type);

                    if (lineMaterial != null)
                    {
                        var colorLM = lineMaterial.GetColor("_Color");
                        colorLM = new Color(colorLM.r, colorLM.g, colorLM.b, 1f);

                        lineMaterial.SetColor("_Color", colorLM);
                    }

                    //if (fillMat != null)
                    //{
                    //    var colorFM = fillMat.GetColor("_ColorFrom");
                    //    colorFM = new Color(colorFM.r, colorFM.g, colorFM.b, 0.5f);

                    //    fillMat.SetColor("_ColorFrom", colorFM);

                    //    var colorTM = fillMat.GetColor("_ColorTo");
                    //    colorTM = new Color(colorTM.r, colorTM.g, colorTM.b, 0.1f);

                    //    fillMat.SetColor("_ColorTo", colorTM);
                    //}

                    if (pointMaterial != null)
                    {
                        var colorPM = pointMaterial.GetColor("_ColorFrom");
                        colorPM = new Color(colorPM.r, colorPM.g, colorPM.b, 1f);

                        pointMaterial.SetColor("_ColorFrom", colorPM);

                        var colorPMTo = pointMaterial.GetColor("_ColorTo");
                        colorPMTo = new Color(colorPMTo.r, colorPMTo.g, colorPMTo.b, 1f);

                        pointMaterial.SetColor("_ColorTo", colorPMTo);
                    }
                }
            }
            else
            {
                GeneralStaticManager.GraphsReadings.Remove(itemName);
                if (GeneralStaticManager.GraphsReadings.Count == 0)
                {
                    ReferenceManager.instance.graphManagers.ForEach(x => Destroy(x.gameObject));
                    ReferenceManager.instance.graphManagers.Clear();
                    return;
                }
                GraphManager alreadyPresentJointGraph = ReferenceManager.instance.graphManagers.FirstOrDefault(x => (x.JointType == type || x.SecondJointType == type) && (x.JointType != MeasurementType.None || x.SecondJointType != MeasurementType.None));
                if (alreadyPresentJointGraph == null)
                {
                    GraphManager graphManager = ReferenceManager.instance.graphManagers.FirstOrDefault(x => x.JointType == type || x.SecondJointType == type);
                    if (graphManager == null)
                    {
                        return;
                    }
                    ReferenceManager.instance.graphManagers.Remove(graphManager);
                    Destroy(graphManager.gameObject);
                    //if (lineMaterial != null)
                    //{
                    //    var colorLM = lineMaterial.GetColor("_Color");
                    //    colorLM = new Color(colorLM.r, colorLM.g, colorLM.b, 0f);

                    //    lineMaterial.SetColor("_Color", colorLM);
                    //}

                    ////if (fillMat != null)
                    ////{
                    ////    var colorFM = fillMat.GetColor("_ColorFrom");
                    ////    colorFM = new Color(colorFM.r, colorFM.g, colorFM.b, 0f);

                    ////    fillMat.SetColor("_ColorFrom", colorFM);

                    ////    var colorTM = fillMat.GetColor("_ColorTo");
                    ////    colorTM = new Color(colorTM.r, colorTM.g, colorTM.b, 0f);

                    ////    fillMat.SetColor("_ColorTo", colorTM);
                    ////}

                    //if (pointMaterial != null)
                    //{
                    //    var colorPM = pointMaterial.GetColor("_ColorFrom");
                    //    colorPM = new Color(colorPM.r, colorPM.g, colorPM.b, 0f);

                    //    pointMaterial.SetColor("_ColorFrom", colorPM);

                    //    var colorPMTo = pointMaterial.GetColor("_ColorTo");
                    //    colorPMTo = new Color(colorPMTo.r, colorPMTo.g, colorPMTo.b, 0f);

                    //    pointMaterial.SetColor("_ColorTo", colorPMTo);
                    //}
                }
                else
                {

                    string color = "";
                    if (alreadyPresentJointGraph.SecondJointType == type)
                    {
                        alreadyPresentJointGraph.MySineWave.graphChart.DataSource.GetCategoryLine(GeneralStaticManager.GetMeasurementTypeName(alreadyPresentJointGraph.JointType), out Material lineMat, out double value, out ChartAndGraph.MaterialTiling tiling);
                        alreadyPresentJointGraph.SecondJointType = MeasurementType.None;
                        if (alreadyPresentJointGraph.JointType != MeasurementType.None && lineMat != null)
                        {
                            color = ColorUtility.ToHtmlStringRGBA(lineMat.color);
                        }
                    }
                    else if (alreadyPresentJointGraph.JointType == type)
                    {
                        alreadyPresentJointGraph.MySineWave.graphChart.DataSource.GetCategoryLine(GeneralStaticManager.GetMeasurementTypeName(alreadyPresentJointGraph.SecondJointType), out Material lineMat, out double value, out ChartAndGraph.MaterialTiling tiling);
                        alreadyPresentJointGraph.JointType = MeasurementType.None;
                        if (alreadyPresentJointGraph.SecondJointType != MeasurementType.None && lineMat != null)
                        {
                            color = ColorUtility.ToHtmlStringRGBA(lineMat.color);
                        }
                    }

                    if (color != "")
                    {
                        alreadyPresentJointGraph.Title.text = $"<color=#{color}>{Enum.GetName(typeof(MeasurementType), alreadyPresentJointGraph.JointType == MeasurementType.None ? alreadyPresentJointGraph.SecondJointType : alreadyPresentJointGraph.JointType)}</color>";
                    }
                    alreadyPresentJointGraph.MySineWave.graphChart.DataSource.ClearCategory(itemName);
                    alreadyPresentJointGraph.MySineWave.graphChart.DataSource.RemoveCategory(itemName);
                    if (alreadyPresentJointGraph.JointType == MeasurementType.None && alreadyPresentJointGraph.SecondJointType == MeasurementType.None)
                    {
                        ReferenceManager.instance.graphManagers.Remove(alreadyPresentJointGraph);
                        Destroy(alreadyPresentJointGraph.gameObject);
                    }

                }
                _selectedMeasurements.Remove(type);
            }


            RaiseEvent();
        }

        /// <summary>
        /// Called when the button is clicked to expand or collapse the list of measurements.
        /// </summary>
        public void OnButtonClick()
        {
            _listExpanded = !_listExpanded;

            var r = _arrow.transform.rotation;

            _arrow.transform.rotation = Quaternion.Euler(_listExpanded ? 180.0f : 0.0f, r.y, r.z);
            _list.SetActive(_listExpanded);
        }

        /// <summary>
        /// Specifies whether the user can select measurements.
        /// User can select measurements before starting a recording.
        /// </summary>
        /// <param name="value">True to enable selection; false otherwise.</param>
        public void SetEnable(bool value)
        {
            foreach (Toggle toggle in _toggles)
            {
                toggle.interactable = value;
            }
        }

        private void RaiseEvent()
        {
            MeasurementType[] types = new MeasurementType[_selectedMeasurements.Count];

            _selectedMeasurements.CopyTo(types);

            onMeasurementsChanged?.Invoke(types);

            _label.text = $"Selected measurements: {_selectedMeasurements.Count}";
        }
    }
}