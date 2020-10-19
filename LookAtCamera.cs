using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private void Update()
    {
        transform.rotation = Camera.main.transform.rotation;
        /*
        Vector3 origRot = transform.eulerAngles;
        transform.LookAt(Camera.main.transform);
        origRot.y = transform.eulerAngles.y;
        transform.eulerAngles = origRot;
        */
    }
}
