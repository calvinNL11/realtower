using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionCounter : MonoBehaviour
{
    public int maxHits = 3;
    private int currentHits = 0;

    public string nextSceneName = "real";
    public string triggerTag = "Enemy";  // ✅ Only objects with this tag can trigger it

    private void OnTriggerEnter(Collider other)
    {
        // ✅ Only count hits if the object has the correct tag
        if (!other.CompareTag(triggerTag))
            return;

        currentHits++;

        if (currentHits >= maxHits)
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}