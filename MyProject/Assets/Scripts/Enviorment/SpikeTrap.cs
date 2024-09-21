using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpikeTrap : MonoBehaviour
{
    private bool isActivated = false;
    private Animator animator;
    public AudioClip spikeSound;
    private AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = spikeSound;
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

            audioSource.Play();
            isActivated = true;
            animator.SetBool("isActivated", true);
            GetComponent<Collider>().enabled = false;
        }
    }

}
