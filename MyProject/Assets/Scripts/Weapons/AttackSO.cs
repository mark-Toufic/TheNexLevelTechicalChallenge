using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Normal Attack")]
public class AttackSO : ScriptableObject
{
    public AnimatorOverrideController animtorOV;
    public float damage;
    public float staminaDrain;
}
