using UnityEngine;
using UnityEngine.InputSystem;

public class Gun : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;
    public float fireRate = 1f;
    void Start()
    {
        InvokeRepeating(nameof(Shoot), 0f, fireRate);
    }
    void Shoot()
    {
        var bullet = Instantiate(
            bulletPrefab,
            bulletSpawnPoint.position,
            bulletSpawnPoint.rotation
        );

        bullet.GetComponent<Rigidbody>().linearVelocity =
            bulletSpawnPoint.forward * bulletSpeed;
    }
}
