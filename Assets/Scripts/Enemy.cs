using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int NodeIndex;
    public float MaxHealth = 10f;
    public float health;
    public float speed = 6f;
    public int ID;

    public float TrueDistance; // distance langs de path nodes

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
        GameLoopManager.EnqueueEnemyToRemove(this);
    }
}

