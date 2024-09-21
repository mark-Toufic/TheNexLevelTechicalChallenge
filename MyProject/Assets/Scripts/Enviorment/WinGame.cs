using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WinGame : MonoBehaviour
{
    private bool isActivated = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && (other.CompareTag("Player"))) 
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.Win();
            }

            isActivated = true;
            GetComponent<Collider>().enabled = false;
        }
    }
}
