using UnityEngine;

public class DistanceToNode : MonoBehaviour
{
    public float Distance = 0f;
    public int Index = 0; // Node index

    void Start()
    {
        // beveiliging
        if (GameLoopManager.NodePositions == null ||

            GameLoopManager.NodePositions.Length <= Index + 1)
        {
            Debug.LogError("NodePositions is niet correct ingesteld.");
            return;
        }

        Distance = Vector3.Distance(
            transform.position,
            GameLoopManager.NodePositions[Index + 1]
        );
    }
}