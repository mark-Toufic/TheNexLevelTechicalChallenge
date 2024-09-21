using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public float damage;
    public float hitCooldown = 1.2f; 

    private BoxCollider triggerBox;
    public EnemyController enemyController;
    private Dictionary<Collider, float> lastHitTimes = new Dictionary<Collider, float>();

    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(enemyController.isAlive)
        {
            if (other.CompareTag("Player"))
            {
                float currentTime = Time.time;

                if (!lastHitTimes.ContainsKey(other) || (currentTime - lastHitTimes[other]) >= hitCooldown)
                {
                    var player = other.GetComponent<PlayerHealth>();

                    if (player != null)
                    {
                        Debug.Log("HitPlayer");
                        Vector3 hitDirection = (other.transform.position - transform.position).normalized;
                        EnableTriggerBox();

                        player.TakeDamage(damage, hitDirection);


                        lastHitTimes[other] = currentTime;
                    }
                }
            }
        }
    }


    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
    }

    public void DisableTriggerBox()
    {
        triggerBox.enabled = false;
    }
}
