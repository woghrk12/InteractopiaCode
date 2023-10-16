using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    #region Variables

    [SerializeField] private float followSpeed = 10f;

    private Transform cameraTransform = null;
    private Transform targetTransform = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        cameraTransform = this.transform;
    }

    private void FixedUpdate()
    {
        if (targetTransform == null) { return; }

        Vector3 targetPos = new Vector3(targetTransform.position.x, targetTransform.position.y, cameraTransform.position.z);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPos, Time.deltaTime * followSpeed);
    }

    #endregion Unity Events

    #region Methods

    public void SetTarget(Transform target)
    {
        targetTransform = target;
    }

    #endregion Methods
}
