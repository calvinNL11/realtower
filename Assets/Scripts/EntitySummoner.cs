using System.Collections.Generic;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{
    public static List<Enemy> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransform;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> EnemyObjectPools;
    private static bool IsInitialized;

    static GlobalData global;

    // ✅ NEW — Resets everything for scene reloads
    public static void ResetStatics()
    {
        EnemiesInGame = new List<Enemy>();
        EnemiesInGameTransform = new List<Transform>();
        EnemyPrefabs = new Dictionary<int, GameObject>();
        EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();

        IsInitialized = false;  // Forces init() to run again next scene
    }

    public static void init()
    {
        // ✅ Runs every new scene (because ResetStatics sets IsInitialized = false)
        if (IsInitialized) return;

        global = GameObject.FindObjectOfType<GlobalData>();

        EnemyPrefabs = new Dictionary<int, GameObject>();
        EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();
        EnemiesInGame = new List<Enemy>();
        EnemiesInGameTransform = new List<Transform>();

        // Load all enemy spawn data
        Enemysummondata[] enemies = Resources.LoadAll<Enemysummondata>("Enemies");
        foreach (var enemy in enemies)
        {
            EnemyPrefabs.Add(enemy.EnemyID, enemy.EnemyPrefab);
            EnemyObjectPools.Add(enemy.EnemyID, new Queue<Enemy>());
        }

        IsInitialized = true;
    }

    public static Enemy SummonEnemy(int EnemyID)
    {
        Enemy summonedEnemy = null;

        if (!EnemyPrefabs.ContainsKey(EnemyID))
        {
            Debug.LogError($"Enemy ID {EnemyID} not found!");
            return null;
        }

        Queue<Enemy> pool = EnemyObjectPools[EnemyID];

        if (pool.Count > 0)
        {
            // Reuse from pool
            summonedEnemy = pool.Dequeue();
            summonedEnemy.gameObject.SetActive(true);
            summonedEnemy.Init();
        }
        else
        {
            // Instantiate new enemy
            GameObject newEnemy = Object.Instantiate(
                EnemyPrefabs[EnemyID],
                GameLoopManager.NodePositions[0], // Spawn at first node
                Quaternion.identity
            );

            summonedEnemy = newEnemy.GetComponent<Enemy>();
            summonedEnemy.Init();
        }

        EnemiesInGame.Add(summonedEnemy);
        EnemiesInGameTransform.Add(summonedEnemy.transform);

        if (global != null)
            global.EnemiesInScene.Add(summonedEnemy.gameObject);

        return summonedEnemy;
    }

    public static void RemoveEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        if (EnemyObjectPools.ContainsKey(enemy.ID))
            EnemyObjectPools[enemy.ID].Enqueue(enemy);

        enemy.gameObject.SetActive(false);

        EnemiesInGameTransform.Remove(enemy.transform);
        EnemiesInGame.Remove(enemy);

        if (global != null)
            global.EnemiesInScene.Remove(enemy.gameObject);
    }
}







