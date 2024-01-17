using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public float RotationSpeed;
    public float angleToRotate;
    void Start()
    {
        StartCoroutine(Rotate());
    }

    public IEnumerator Rotate()
    {
        while (gameObject.activeSelf)
        {
            transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z - angleToRotate);
            yield return new WaitForSeconds(RotationSpeed);
        }
    }


}
