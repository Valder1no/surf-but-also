using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillFloor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawn_point;

    private void OnTriggerEnter(Collider other)
    {
            if (other.CompareTag("Player"))
            {
                player.transform.position = respawn_point.transform.position;
                RespawnObjects();
            }
    }

    private void RespawnObjects()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("respawnable");

        foreach (GameObject obj in objs)
        {
            obj.SetActive(true);

            BlueSlimeStatic slime = obj.GetComponent<BlueSlimeStatic>();
            
            if (slime != null)
            {
                obj.transform.position = slime.savedPos;
            }
        }
    }
}
