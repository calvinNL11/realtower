using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;
using System.Collections;
using System.Collections.Generic;

public class GameLoopManager : MonoBehaviour
{
    public static Vector3[] NodePositions;
    private static Queue<int> enemyIDsToSummon;
    private static Queue<Enemy> enemiesToRemove;

    public Transform NodeParent;
    public bool LoopShouldEnd;

    // ✅ NEW — Reset all static data safely
    public static void ResetStatics()
    {
        NodePositions = null;
        enemyIDsToSummon = new Queue<int>();
        enemiesToRemove = new Queue<Enemy>();

        EntitySummoner.ResetStatics();   // <-- Make sure this exists in EntitySummoner
    }

    private void Start()
    {
        // ✅ Always reset all static data on scene load
        ResetStatics();

        // Set node positions again
        int nodeCount = NodeParent.childCount;
        NodePositions = new Vector3[nodeCount];
        for (int i = 0; i < nodeCount; i++)
            NodePositions[i] = NodeParent.GetChild(i).position;

        // Initialize summoner
        EntitySummoner.init();

        // Rebuild queues
        enemyIDsToSummon = new Queue<int>();
        enemiesToRemove = new Queue<Enemy>();

        // Spawn test enemy every second
        InvokeRepeating(nameof(SummonTest), 0f, 1f);
        StartCoroutine(GameLoop());
    }

    void SummonTest()
    {
        EnqueueEnemyIDToSummon(1);
    }

    IEnumerator GameLoop()
    {
        while (!LoopShouldEnd)
        {
            // === Spawn new enemies ===
            while (enemyIDsToSummon.Count > 0)
            {
                int id = enemyIDsToSummon.Dequeue();
                EntitySummoner.SummonEnemy(id);
            }

            // === Move enemies ===
            int count = EntitySummoner.EnemiesInGame.Count;

            if (count > 0)
            {
                NativeArray<Vector3> nodes = new NativeArray<Vector3>(NodePositions, Allocator.TempJob);
                NativeArray<int> nodeIndices = new NativeArray<int>(count, Allocator.TempJob);
                NativeArray<float> speeds = new NativeArray<float>(count, Allocator.TempJob);

                TransformAccessArray enemyAccess =
                    new TransformAccessArray(EntitySummoner.EnemiesInGameTransform.ToArray());

                for (int i = 0; i < count; i++)
                {
                    nodeIndices[i] = EntitySummoner.EnemiesInGame[i].NodeIndex;
                    speeds[i] = EntitySummoner.EnemiesInGame[i].speed;
                }

                MoveEnemiesJob job = new MoveEnemiesJob
                {
                    NodePositions = nodes,
                    nodeIndex = nodeIndices,
                    EnemySpeed = speeds,
                    deltaTime = Time.deltaTime
                };

                JobHandle handle = job.Schedule(enemyAccess);
                handle.Complete();

                for (int i = 0; i < count; i++)
                {
                    EntitySummoner.EnemiesInGame[i].NodeIndex = nodeIndices[i];

                    if (EntitySummoner.EnemiesInGame[i].NodeIndex >= NodePositions.Length)
                        EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                }

                nodes.Dispose();
                nodeIndices.Dispose();
                speeds.Dispose();
                enemyAccess.Dispose();
            }

            // === Remove enemies ===
            while (enemiesToRemove.Count > 0)
            {
                Enemy e = enemiesToRemove.Dequeue();
                EntitySummoner.RemoveEnemy(e);
            }

            yield return null;
        }
    }

    public static void EnqueueEnemyIDToSummon(int ID)
    {
        enemyIDsToSummon.Enqueue(ID);
    }

    public static void EnqueueEnemyToRemove(Enemy e)
    {
        enemiesToRemove.Enqueue(e);
    }
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction] public NativeArray<int> nodeIndex;
    [NativeDisableParallelForRestriction] public NativeArray<float> EnemySpeed;
    [NativeDisableParallelForRestriction] public NativeArray<Vector3> NodePositions;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (nodeIndex[index] >= NodePositions.Length)
            return;

        Vector3 target = NodePositions[nodeIndex[index]];
        transform.position = Vector3.MoveTowards(transform.position, target, EnemySpeed[index] * deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
            nodeIndex[index]++;
    }
}
