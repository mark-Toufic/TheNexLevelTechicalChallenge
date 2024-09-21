using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    public Transform cam;

    private void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
