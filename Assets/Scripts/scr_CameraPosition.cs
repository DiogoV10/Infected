using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class scr_CameraPosition : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float followDistance = 10f;
    [SerializeField] private LayerMask ignoreLayer;

    private void Update()
    {
        Vector3 targetPosition = GetTargetPosition();

        transform.position = targetPosition;
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, followDistance, ignoreLayer))
        {
            return raycastHit.point;
        }
        else
        {
            return mainCamera.transform.position + mainCamera.transform.forward * followDistance;
        }
    }
}
