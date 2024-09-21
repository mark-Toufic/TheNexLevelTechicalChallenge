using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 100f;
    public float maxHealth = 100f;

    [SerializeField] private Weapon weapon;
    public Animator enemyAnim;
    public EnemyController enemyController;
    private BoxCollider enemyCollider;
    public ParticleSystem spark;
    public ParticleSystem blood;

    [Header("Health Regen Settings")]
    [SerializeField] private float healthRegen = 0.5f;

    [Header("Health UI Settings")]
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private Image healthUI = null;
    [SerializeField] private Image easeHealthUI = null;
    [SerializeField] private CanvasGroup healthGroup = null;

    [Header("Aduio")]
    public AudioClip HitSound;
    public AudioClip dieSound;
    private AudioSource audioSource;



    private float targetHealthFill;

    private void Start()
    {
        enemyCollider = GetComponent<BoxCollider>();
        enemyController = GetComponent<EnemyController>(); 
        enemyAnim = GetComponent<Animator>();
        targetHealthFill = health / maxHealth;
        UpdateHealthUI();
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = HitSound;
    }

    private void Update()
    {
        if (easeHealthUI.fillAmount != targetHealthFill)
        {
            easeHealthUI.fillAmount = Mathf.Lerp(easeHealthUI.fillAmount, targetHealthFill, lerpSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount, Vector3 hitDirection)
    {
        if (!enemyController.isAlive) return;

        health -= amount;
        health = Mathf.Clamp(health, 0, maxHealth);
        targetHealthFill = health / maxHealth;
        enemyAnim.SetTrigger("isHit");
        audioSource.Play();
        spark.Play();
        blood.Play();

        UpdateHealthUI(); 

        if (health <= 0)
        {
            enemyController.RevertControl();
            enemyController.isControlled = false;
            enemyAnim.SetTrigger("isDead");
            enemyController.isAlive = false;
            healthGroup.alpha = 0;
            enemyCollider.enabled = false;
            audioSource.clip = dieSound; 
            audioSource.Play();
            Die();
        }
        else
        {
            enemyController.CancelAttack(); 
        }
    }

    public void SpikeTrap()
    {
        enemyController.RevertControl();
        enemyController.isControlled = false;
        enemyAnim.SetTrigger("isDead");
        enemyController.isAlive = false;
        healthGroup.alpha = 0;
        enemyCollider.enabled = false;
        audioSource.clip = dieSound;
        audioSource.Play();
        Die();
    }

    private void UpdateHealthUI()
    {
        healthUI.fillAmount = health / maxHealth;
        healthGroup.alpha = (health == maxHealth) ? 0 : 1;
    }

    public void RegenerateHealth()
    {
        health += healthRegen * Time.deltaTime;
        health = Mathf.Clamp(health, 0, maxHealth);
        targetHealthFill = health / maxHealth;
        UpdateHealthUI();
    }

    public void Die()
    {
        Destroy(gameObject, 5f);
    }
}
