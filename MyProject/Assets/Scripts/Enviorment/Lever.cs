using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class Lever : MonoBehaviour
{
    private Animator anim;
    public Animator lockedGate;
    private BoxCollider triggerBox;
    private bool isEnemyInTrigger = false;

    [Header("Aduio")]
    public AudioClip leverSound;
    public AudioClip doorSOund;
    private AudioSource audioSource;

    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            isEnemyInTrigger = true; // Set the flag to true when enemy enters
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            isEnemyInTrigger = false; // Reset the flag when enemy exits
        }
    }

    void Update()
    {
        if (isEnemyInTrigger && Input.GetKey(KeyCode.E))
        {
            audioSource.clip = leverSound;
            audioSource.clip = doorSOund;
            audioSource.Play();
            anim.SetBool("isPulled", true);
            lockedGate.SetBool("isOpened", true);
            Debug.Log("isClicking");
        }
    }
}
