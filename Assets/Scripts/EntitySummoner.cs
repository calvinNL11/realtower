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

    public static void init()
    {
        if (IsInitialized) return;

        global = GameObject.FindObjectOfType<GlobalData>();

        EnemyPrefabs = new Dictionary<int, GameObject>();
        EnemyObjectPools = new Dictionary<int, Queue<Enemy>>();
        EnemiesInGame = new List<Enemy>();
        EnemiesInGameTransform = new List<Transform>();

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
            summonedEnemy = pool.Dequeue();
            summonedEnemy.gameObject.SetActive(true);
            summonedEnemy.Init();
        }
        else
        {
            GameObject newEnemy = Object.Instantiate(
                EnemyPrefabs[EnemyID],
                GameLoopManager.NodePositions[0],
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








