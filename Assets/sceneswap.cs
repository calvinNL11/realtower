using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchOnTouch : MonoBehaviour
{
    public string nextSceneName = "SampleScene";   // Vul hier de naam in van de scene waar je heen wilt

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))   // Check of de speler het block raakt
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
