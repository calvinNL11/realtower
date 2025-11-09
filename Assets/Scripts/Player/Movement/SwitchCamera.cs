

using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    public GameObject Camera_1;
    public GameObject Camera_2;

    private bool isCamera1Active = true;
    private bool canSwitch = true;   // cooldown flag
    public float cooldownTime = 3f;  // 3 seconden cooldown

    private void Start()
    {
        ActivateCamera1();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canSwitch)
        {
            ManageCamera();
        }
    }

    public void ManageCamera()
    {
        if (isCamera1Active)
        {
            ActivateCamera2();
        }
        else
        {
            ActivateCamera1();
        }

        // cooldown starten
        StartCoroutine(Cooldown());
    }

    private void ActivateCamera1()
    {
        Camera_1.SetActive(true);
        Camera_2.SetActive(false);
        isCamera1Active = true;
    }

    private void ActivateCamera2()
    {
        Camera_1.SetActive(false);
        Camera_2.SetActive(true);
        isCamera1Active = false;
    }

    private System.Collections.IEnumerator Cooldown()
    {
        canSwitch = false;
        yield return new WaitForSeconds(cooldownTime);
        canSwitch = true;
    }
}


