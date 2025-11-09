using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;

    public float throwForce = 500f;
    public float pickUpRange = 5f;
    private float rotationSensitivity = 1f;

    private GameObject heldObj;
    private Rigidbody heldObjRb;
    private bool canDrop = true;

    private int holdLayer;

    void Start()
    {
        holdLayer = LayerMask.NameToLayer("block");

        // Als layer niet bestaat, fallback naar default (0)
        if (holdLayer == -1)
        {
            Debug.LogWarning("Layer 'block' bestaat niet! Script gebruikt Default layer (0).");
            holdLayer = 0;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObj == null)
            {
                TryPickUp();
            }
            else if (canDrop)
            {
                StopClipping();
                DropObject();
            }
        }

        if (heldObj != null)
        {
            MoveObject();
            RotateObject();

            if (Input.GetKeyDown(KeyCode.Mouse0) && canDrop)
            {
                StopClipping();
                ThrowObject();
            }
        }
    }

    // ============================================================
    // PICK UP 
    // ============================================================

    void TryPickUp()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, pickUpRange))
        {
            if (hit.transform.CompareTag("CanPickUp"))
            {
                PickUpObject(hit.transform.gameObject);
            }
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (!pickUpObj.TryGetComponent<Rigidbody>(out heldObjRb))
        {
            Debug.LogError("Object heeft GEEN rigidbody, kan niet oppakken!");
            return;
        }

        if (!pickUpObj.TryGetComponent<Collider>(out Collider col))
        {
            Debug.LogError("Object heeft GEEN collider, kan niet oppakken!");
            return;
        }

        // Check concave collider
        MeshCollider meshCol = pickUpObj.GetComponent<MeshCollider>();
        if (meshCol != null && meshCol.convex == false)
        {
            Debug.LogWarning("Convex staat uit op MeshCollider ? ingeschakeld om crash te voorkomen.");
            meshCol.convex = true;
        }

        heldObj = pickUpObj;

        // BELANGRIJK: eerst velocity resetten, DAN kinematic
        heldObjRb.linearVelocity = Vector3.zero;
        heldObjRb.angularVelocity = Vector3.zero;

        heldObjRb.isKinematic = true;

        // Parenting
        heldObj.transform.SetParent(holdPos);

        // Layer setten
        heldObj.layer = holdLayer;

        // Collision met player uitzetten
        Physics.IgnoreCollision(col, player.GetComponent<Collider>(), true);
    }

    // ============================================================
    // DROP
    // ============================================================

    void DropObject()
    {
        if (heldObj == null) return;

        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);

        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.SetParent(null);

        heldObj = null;
    }

    // ============================================================
    // MOVE OBJECT
    // ============================================================

    void MoveObject()
    {
        heldObj.transform.position = Vector3.Lerp(
            heldObj.transform.position,
            holdPos.position,
            Time.deltaTime * 15f
        );
    }

    // ============================================================
    // ROTATE OBJECT
    // ============================================================

    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R))
        {
            canDrop = false;

            float Xrot = Input.GetAxis("Mouse X") * rotationSensitivity;
            float Yrot = Input.GetAxis("Mouse Y") * rotationSensitivity;

            heldObj.transform.Rotate(Vector3.down, Xrot, Space.World);
            heldObj.transform.Rotate(Vector3.right, Yrot, Space.World);
        }
        else
        {
            canDrop = true;
        }
    }

    // ============================================================
    // THROW
    // ============================================================

    void ThrowObject()
    {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);

        heldObj.layer = 0;
        heldObjRb.isKinematic = false;

        heldObj.transform.SetParent(null);
        heldObjRb.AddForce(transform.forward * throwForce);

        heldObj = null;
    }

    // ============================================================
    // STOP CLIPPING
    // ============================================================

    void StopClipping()
    {
        if (heldObj == null) return;

        float clipRange = Vector3.Distance(heldObj.transform.position, transform.position);
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, clipRange);

        if (hits.Length > 1)
        {
            heldObj.transform.position = transform.position + new Vector3(0, -0.5f, 0);
        }
    }
}