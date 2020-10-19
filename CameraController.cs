using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTarget;

    private void Update()
    {
        cameraTarget = LevelController.Instance.currentLevel.transform;
        transform.position = cameraTarget.position;
    }
}
