using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("Stamina settings")]
    public float playerStamina = 100.0f;
    [SerializeField] public float maxStamina = 100.0f;
    [SerializeField] private float rollCost = 30.0f;
    [SerializeField] private float sprintCost; // Cost per second while sprinting
    [SerializeField] public float minStaminaForAction = 30.0f; // Minimum stamina to allow actions
    [HideInInspector] public bool hasRegenrated = true;
    public bool isSprinting = false;

    [Header("Stamina Regen Settings")]
    [SerializeField] private float staminaRegen = 0.5f;

    [Header("Stamina UI Settings")]
    [SerializeField] private Image staminaUI = null;
    [SerializeField] private CanvasGroup staminaGroup = null;
    private PlayerController playerController;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        staminaGroup.alpha = 0;
    }

    private void Update()
    {
        if (!isSprinting)
        {
            if (playerStamina < maxStamina - 0.01)
            {
                playerStamina += staminaRegen * Time.deltaTime;
                UpdateStamina(1);

                if (playerStamina >= 98)
                {
                    staminaGroup.alpha = 0;
                    playerStamina = 100;
                    hasRegenrated = true;
                }
            }

            if (playerStamina >= minStaminaForAction)
            {
                hasRegenrated = true;
            }
            else
            {
                hasRegenrated = false;
            }
        }
        if(playerController.lockOn)
        {
            UpdateStamina(1);
        }
        else if(!playerController.lockOn && playerStamina >= 98)
        {
            staminaGroup.alpha = 0;
        }
    }

    public void Sprinting()
    {
        if (playerStamina <= 0)
        {
            isSprinting = false;
            return;
        }

        if (hasRegenrated)
        {
            isSprinting = true;
            playerStamina -= sprintCost * Time.deltaTime;
            UpdateStamina(1);

            if (playerStamina <= 0)
            {
                playerStamina = 0;
                hasRegenrated = false;
                staminaGroup.alpha = 0;
            }
        }
    }

    public void StaminaRoll()
    {
        if (playerStamina >= rollCost && playerStamina > minStaminaForAction)
        {
            playerStamina -= rollCost;
            UpdateStamina(1);
        }
    }

    void UpdateStamina(int value)
    {
        staminaUI.fillAmount = playerStamina / maxStamina;

        if (value == 0)
        {
            staminaGroup.alpha = 0;
        }
        else
        {
            staminaGroup.alpha = 1;
        }
    }
}
