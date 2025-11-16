using UnityEngine;

public class TurretScript : MonoBehaviour
{
    public enum TargetingType { First, Strong, Weak, Last }

    [Header("References")]
    GlobalData WorldAccessData;

    [Header("Targeting")]
    public LayerMask EnemyMask;
    public float Range = 25f;
    public GameObject Target;
    public bool AllowSharedTargets = false; // 👈 NEW toggle

    [Header("Attack Settings")]
    public float AttackSpeed = 5f;
    public float Damage = 10f;
    private float Delay;

    void Start()
    {
        WorldAccessData = FindObjectOfType<GlobalData>();
        Delay = AttackSpeed;
    }

    void Update()
    {
        if (Target != null)
        {
            Enemy e = Target.GetComponent<Enemy>();

            // Target destroyed or inactive
            if (e == null || !Target.activeInHierarchy)
            {
                ReleaseTarget();
                return;
            }

            // Rotate toward target
            Vector3 look = new Vector3(Target.transform.position.x,
                                       transform.position.y,
                                       Target.transform.position.z);
            transform.LookAt(look);

            // Attack if ready
            Attack();

            // Out of range? Lose target
            if (Vector3.Distance(transform.position, Target.transform.position) > Range)
                ReleaseTarget();
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
            if (Target != null)
            {
                Enemy e = Target.GetComponent<Enemy>();
                if (e != null)
                    e.TakeDamage(Damage);
            }

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

            // 👇 Skip enemies already targeted if shared targeting is OFF
            if (!AllowSharedTargets && e.IsTargeted) continue;

            // Example: using TrueDistance for path-based priority
            if (e.TrueDistance < bestDist)
            {
                bestDist = e.TrueDistance;
                bestTarget = obj;
            }
        }

        if (bestTarget != null)
        {
            Target = bestTarget;
            Enemy e = Target.GetComponent<Enemy>();

            // 👇 Mark as targeted only if exclusive targeting mode
            if (e != null && !AllowSharedTargets)
                e.IsTargeted = true;
        }
    }

    void ReleaseTarget()
    {
        if (Target != null)
        {
            Enemy e = Target.GetComponent<Enemy>();
            if (e != null && !AllowSharedTargets)
                e.IsTargeted = false;

            Target = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}