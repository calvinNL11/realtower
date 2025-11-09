using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GameLoopManager : MonoBehaviour
{
    public static Vector3[] NodePositions;
    private static Queue<int> enemyIDsToSummon;
    private static Queue<Enemy> enemiesToRemove;

    public Transform NodeParent;
    public bool LoopShouldEnd;

    private void Start()
    {
        enemyIDsToSummon = new Queue<int>();
        enemiesToRemove = new Queue<Enemy>();

        int nodeCount = NodeParent.childCount;
        NodePositions = new Vector3[nodeCount];
        for (int i = 0; i < nodeCount; i++)
            NodePositions[i] = NodeParent.GetChild(i).position;

        EntitySummoner.init();

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
            if (EntitySummoner.EnemiesInGame.Count > 0)
            {
                int count = EntitySummoner.EnemiesInGame.Count;

                NativeArray<Vector3> nodes = new NativeArray<Vector3>(NodePositions, Allocator.TempJob);
                NativeArray<int> nodeIndices = new NativeArray<int>(count, Allocator.TempJob);
                NativeArray<float> speeds = new NativeArray<float>(count, Allocator.TempJob);
                TransformAccessArray enemyAccess = new TransformAccessArray(EntitySummoner.EnemiesInGameTransform.ToArray(), 2);

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

                    // Debug info
                    // Debug.Log($"Enemy {i} at node {nodeIndices[i]}");

                    // Remove when past last node
                    if (EntitySummoner.EnemiesInGame[i].NodeIndex >= NodePositions.Length)
                    {
                        EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                    }
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
                Debug.Log($"Removed enemy {e.ID}");
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
    [NativeDisableParallelForRestriction]
    public NativeArray<int> nodeIndex;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> EnemySpeed;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> NodePositions;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (nodeIndex[index] >= NodePositions.Length)
            return;

        Vector3 target = NodePositions[nodeIndex[index]];
        transform.position = Vector3.MoveTowards(transform.position, target, EnemySpeed[index] * deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            nodeIndex[index]++;
        }
    }
}