using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public float staminaDrain;
    public float hitCooldown = 1.2f; // Time in seconds before hitting again

    private BoxCollider triggerBox;
    private Dictionary<Collider, float> lastHitTimes = new Dictionary<Collider, float>();

    void Start()
    {
        triggerBox = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object has the "Enemy" tag
        if (other.CompareTag("Enemy"))
        {
            float currentTime = Time.time;

            if (!lastHitTimes.ContainsKey(other) || (currentTime - lastHitTimes[other]) >= hitCooldown)
            {
                // Get the Enemy component from the collided object
                var enemy = other.GetComponent<Enemy>();

                // If the Enemy component is found, apply damage
                if (enemy != null)
                {
                    Debug.Log("Hit");
                    Vector3 hitDirection = (other.transform.position - transform.position).normalized;

                    enemy.TakeDamage(damage, hitDirection);

                    // Update the last hit time
                    lastHitTimes[other] = currentTime;
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
