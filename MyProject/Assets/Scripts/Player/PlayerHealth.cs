using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 100f;
    public float maxHealth = 100f;
    public Animator playerAnim;
    public PlayerController playerController;

    [Header("Health Regen Settings")]
    [SerializeField] private float healthRegen = 0.5f;
    [SerializeField] private float regenDelay = 5f;
    private float lastHitTime;

    [Header("Health UI Settings")]
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Image healthUI = null;
    [SerializeField] private Image easeHealthUI = null;
    [SerializeField] private CanvasGroup healthGroup = null;
    [SerializeField] private GameObject deadUI = null;
    [SerializeField] private GameObject aliveUI = null;
    [SerializeField] private GameObject winUI = null;
    public ThirdPersonCam cameraController;

    [Header("Aduio")]
    public AudioClip HitSound;
    private AudioSource audioSource;


    private float targetHealthFill;

    void Start()
    {
        targetHealthFill = health / maxHealth;
        UpdateHealthUI();
        playerController = playerController ?? GetComponent<PlayerController>();
        playerAnim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = HitSound;

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<ThirdPersonCam>();
        }
    }

    private void Update()
    {
        if (easeHealthUI.fillAmount != targetHealthFill)
        {
            easeHealthUI.fillAmount = Mathf.Lerp(easeHealthUI.fillAmount, targetHealthFill, lerpSpeed * Time.deltaTime);
        }

        if (Time.time - lastHitTime > regenDelay && health < maxHealth)
        {
            RegenerateHealth();
        }

        if (playerController.lockOn)
        {
            healthGroup.alpha = 1;
        }
        else if (!playerController.lockOn && health >= 98)
        {
            healthGroup.alpha = 0;
        }
    }

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        targetHealthFill = health / maxHealth;
        UpdateHealthUI();
        playerAnim.SetTrigger("Hit");
        audioSource.Play();
        lastHitTime = Time.time;
        playerController.isHit = true;
        StartCoroutine(Hit());
        if (health <= 0)
        {
            Die();
        }
    }

    public void Win()
    {
        winUI.SetActive(true);
        deadUI.SetActive(false);
        aliveUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        if (cameraController != null)
        {
            cameraController.enabled = false; // Disable camera control
        }
        playerController.enabled = false;

        if (playerController != null)
        {
            playerController.isDead = true; // Set the isDead flag to true
        }
    }
    public void Die()
    {
        playerAnim.SetTrigger("Die");
        deadUI.SetActive(true);
        aliveUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        if (cameraController != null)
        {
            cameraController.enabled = false; // Disable camera control
        }
        playerController.enabled = false;

        if (playerController != null)
        {
            playerController.isDead = true; // Set the isDead flag to true
        }
    }

    IEnumerator Hit()
    {
        yield return new WaitForSeconds(0.6f);
        playerController.isHit = false;
    }

    private void UpdateHealthUI()
    {

        healthUI.fillAmount = health / maxHealth;
        healthGroup.alpha = (health == maxHealth) ? 0 : 1;
    }

    private void RegenerateHealth()
    {
        health += healthRegen * Time.deltaTime;
        health = Mathf.Clamp(health, 0, maxHealth);
        targetHealthFill = health / maxHealth;
        UpdateHealthUI();
    }
}
