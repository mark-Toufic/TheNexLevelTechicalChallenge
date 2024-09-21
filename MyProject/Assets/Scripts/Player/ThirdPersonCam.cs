using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;
    public CinemachineFreeLook freeLookCam;
    public GameObject lockCam, freeCam;
    public float rotationSpeed;
    public PlayerController playerController; 

    private void Start()
    {
        if (freeLookCam == null)
            freeLookCam = GetComponent<CinemachineFreeLook>(); 

        if (playerController == null)
            playerController = player.GetComponent<PlayerController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    private void LateUpdate()
    {

        if (playerController.lockOn && playerController.lockOnTarget != null)
        {
            if (playerController.isControllingEnemy)
            {
                lockCam.SetActive(false);
                freeCam.SetActive(false);
            }
            else
            {
                lockCam.SetActive(true);
                freeCam.SetActive(false);

                freeLookCam.LookAt = playerController.lockOnTarget;
                playerObj.LookAt(playerController.lockOnTarget);
                orientation.forward = playerObj.forward;

                Vector3 currentRotation = playerObj.eulerAngles;
                currentRotation.x = 0;
                playerObj.eulerAngles = currentRotation;
            }

        }
        else
        {
            if (!playerController.isRolling)
            {
               lockCam.SetActive(false);
               freeCam.SetActive(true);

                Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
                orientation.forward = viewDir.normalized;

                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

                if (inputDir != Vector3.zero)
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }
    }
}
