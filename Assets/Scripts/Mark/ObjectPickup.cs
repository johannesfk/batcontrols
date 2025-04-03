using UnityEngine;
using UnityEngine.UI;  // Required for UI

public class ObjectPickup : MonoBehaviour
{
    public float pickupRange = 2f;  // Distance to pick up items
    public Transform player;  // Reference to the player
    public GameObject pickupUI;  // UI element to show

    private GameObject nearestObject;  // Object in range
    private GameObject heldObject;  // Picked-up object

    void Update()
    {
        DetectPickupObject();

        if (Input.GetKeyDown(KeyCode.E)) // Press E to pick up or drop
        {
            if (heldObject == null && nearestObject != null)
                PickUpObject(nearestObject);
            else
                DropObject();
        }
    }

    void DetectPickupObject()
    {
        nearestObject = null;
        Collider[] objectsInRange = Physics.OverlapSphere(player.position, pickupRange);

        foreach (Collider col in objectsInRange)
        {
            if (col.CompareTag("Bug"))
            {
                nearestObject = col.gameObject;
                break;  // Only pick the closest object
            }
        }

        // Show UI if an object is nearby, hide otherwise
        if (pickupUI != null)
            pickupUI.SetActive(nearestObject != null);
    }

    void PickUpObject(GameObject obj)
    {
        heldObject = obj;
        heldObject.SetActive(false); // Hide object
        pickupUI.SetActive(false);  // Hide UI
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.SetActive(true); // Show object again
            heldObject.transform.position = player.position + player.forward; // Drop slightly ahead
            heldObject = null;
        }
    }
}
