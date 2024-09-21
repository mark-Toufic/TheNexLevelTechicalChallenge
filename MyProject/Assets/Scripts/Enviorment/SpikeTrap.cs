using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpikeTrap : MonoBehaviour
{
    private bool isActivated = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && (other.CompareTag("Player") || other.CompareTag("Enemy")))
        {

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.SpikeTrap();
            }

            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.Die();
            }

            isActivated = true;
            animator.SetBool("isActivated", true);
            GetComponent<Collider>().enabled = false;
        }
    }

}
