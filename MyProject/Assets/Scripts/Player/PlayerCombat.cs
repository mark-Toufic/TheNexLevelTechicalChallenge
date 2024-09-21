using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public List<AttackSO> combo;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;
    public bool isAttacking = false;
    public Stamina staminaController;
    public float minimumStaminaRequired = 10f;
    public PlayerController playerController;

    Animator animator;
    [SerializeField] Weapon weapon;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = playerController ?? GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        ExitAttack();
        if(Input.GetKey(KeyCode.Mouse0) && !playerController.isHit && !playerController.isControllingEnemy && !playerController.isDead) 
        {
           Attack();   
        }
    }

    void Attack()
    {
        if (staminaController.playerStamina <= minimumStaminaRequired)
        {
            Debug.Log("Not enough stamina to attack.");
            return;
        }

        if (Time.time - lastComboEnd > 0.7 && comboCounter <= combo.Count)
        {
            animator.SetBool("isAttacking", true);
            animator.SetBool("isWalking", false);
            weapon.EnableTriggerBox();
            isAttacking = true;
            CancelInvoke("EndCombo");

            if(Time.time - lastClickedTime >= 0.8f)
            {
                animator.runtimeAnimatorController = combo[comboCounter].animtorOV;
                animator.Play("Attack", 0, 0);
                weapon.damage = combo[comboCounter].damage;
                weapon.staminaDrain = combo[comboCounter].staminaDrain;
                staminaController.playerStamina -= weapon.staminaDrain;
                comboCounter++;
                weapon.EnableTriggerBox();
                lastClickedTime = Time.time;
                if(comboCounter == combo.Count)
                {
                    comboCounter = 0;
                }
            }    
        }
    }
    void ExitAttack()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.80 && animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            Invoke("EndCombo", 0);
        }
    }

    void EndCombo()
    {
        comboCounter = 0;
        lastComboEnd = Time.time;
        animator.SetBool("isAttacking", false);
        weapon.DisableTriggerBox();
        isAttacking = false;
    }
}
