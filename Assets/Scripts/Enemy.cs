using System.Diagnostics;
using UnityEngine;

[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
public class Enemy : MonoBehaviour
{
    public int NodeIndex;
    public float MaxHealth = 10f;
    public float health;
    public float speed = 6f;
    public int ID;

    public float TrueDistance; // distance langs de path nodes

    // 👇 Added field: lets turrets know if this enemy is already targeted
    [HideInInspector]
    public bool IsTargeted = false;

    public void Init()
    {
        health = MaxHealth;
        NodeIndex = 0;

        transform.position = GameLoopManager.NodePositions[0]
            - (GameLoopManager.NodePositions[1] - GameLoopManager.NodePositions[0]).normalized * 0.1f;
    }

    public void Update()
    {
        // bereken afstand tot volgende node
        if (NodeIndex < GameLoopManager.NodePositions.Length)
            TrueDistance = Vector3.Distance(transform.position, GameLoopManager.NodePositions[NodeIndex]);
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        // 👇 Make sure any turret targeting this enemy will release it
        IsTargeted = false;

        // Notify game manager that this enemy should be removed
        GameLoopManager.EnqueueEnemyToRemove(this);
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }
}
