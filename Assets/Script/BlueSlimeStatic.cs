using UnityEngine;

public class BlueSlimeStatic : MonoBehaviour
{
    [SerializeField] private ParticleSystem blueDeathExplosion;
    public Vector3 savedPos;

    void Start()
    {
        savedPos = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(blueDeathExplosion, transform.position, Quaternion.identity);
            // kkads call's lai pieskaititu rezulatatu or smth
            gameObject.SetActive(false);
        }
    }
}
