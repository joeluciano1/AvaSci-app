using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CustomUIAnimator : MonoBehaviour
{
    [Header("Color Animation Type")]
    public bool ColorAnimation;
    public Color InitialColor;
    public Color Color2;
    public Image ImageToChangeColorOf;

    Vector3 initialPosition;
    Vector3 initialRotation;
    bool isValuesSet;
    
    public Vector3 PositionToAnimate;
    public Vector3 RotationToAnimate;
    float time;
    public bool isInstantiated;
    public float AnimationTime;
    // Start is called before the first frame update
    void Awake()
    {
        if(!isValuesSet)
        {
            initialPosition = transform.position;
            InitialColor = ImageToChangeColorOf == null ? Color.black : ImageToChangeColorOf.color;
            initialRotation = transform.eulerAngles;
            isValuesSet = true;
        }
    }
    private void OnEnable()
    {
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
        ImageToChangeColorOf.color = InitialColor;
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (ColorAnimation)
        {
            if (ImageToChangeColorOf != null)
            {
                ImageToChangeColorOf.color = Color.Lerp(InitialColor, Color2, 0.1f);
            }
        }

        transform.Translate(PositionToAnimate);
        transform.Rotate(RotationToAnimate);
        time += Time.deltaTime;

        if (time >= AnimationTime)
        {
            Destroy(gameObject);
        }
    }
}
