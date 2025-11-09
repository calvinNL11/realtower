using UnityEngine;

public class TurretScript : MonoBehaviour
{
    public enum TargettingType { First, Strong, Weak, Last }

    GlobalData WorldAccessData;

    public LayerMask EnemyMask;
    public float Range = 25f;
    public GameObject Target;

    public float AttackSpeed = 1f;
    public float Damage = 5f;
    float Delay;

    void Start()
    {
        WorldAccessData = FindObjectOfType<GlobalData>();
        Delay = AttackSpeed;
    }

    void Update()
    {
        if (Target != null)
        {
            Vector3 look = new Vector3(Target.transform.position.x,
                                       transform.position.y,
                                       Target.transform.position.z);

            transform.LookAt(look);
            Attack();

            // target kwijt?
            if (Vector3.Distance(transform.position, Target.transform.position) > Range)
                Target = null;
        }
        else
        {
            LookForEnemies();
        }
    }

    void Attack()
    {
        Delay -= Time.deltaTime;

        if (Delay <= 0f)
        {
            Target.GetComponent<Enemy>().TakeDamage(Damage);
            Delay = AttackSpeed;
        }
    }

    void LookForEnemies()
    {
        float bestDist = Mathf.Infinity;
        GameObject bestTarget = null;

        foreach (GameObject obj in WorldAccessData.EnemiesInScene)
        {
            if (obj == null) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist > Range) continue;

            Enemy e = obj.GetComponent<Enemy>();
            if (e == null) continue;

            if (e.TrueDistance < bestDist)
            {
                bestDist = e.TrueDistance;
                bestTarget = obj;
            }
        }

        if (bestTarget != null)
            Target = bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}