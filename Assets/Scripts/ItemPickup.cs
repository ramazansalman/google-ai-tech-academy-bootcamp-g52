using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public static bool itemCollected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            itemCollected = true;
            Destroy(gameObject);
        }
    }
}
