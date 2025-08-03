using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private Transform currentSpawnPoint;

    void Start()
    {
        GameObject initial = new GameObject("InitialSpawnPoint");
        initial.transform.position = transform.position;
        currentSpawnPoint = initial.transform;
    }

    public void Respawn()
    {
        StartCoroutine(RespawnSmooth());
    }

    private IEnumerator RespawnSmooth()
    {
        CharacterController cc = GetComponent<CharacterController>();
        cc.enabled = false;

        transform.position = currentSpawnPoint.position;

        GetComponent<PlayerMovement.PlayerMovement>()?.ResetVelocity();

        yield return null;

        cc.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeathZone"))
        {
            Respawn();
        }
        if (other.CompareTag("Spike"))
        {
            Respawn();
        }
    }

    public void UpdateSpawn(Transform newSpawn)
    {
        currentSpawnPoint = newSpawn;
    }
}
