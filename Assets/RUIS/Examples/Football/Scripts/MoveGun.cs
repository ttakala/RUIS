using UnityEngine;
using System.Collections;

public class MoveGun : MonoBehaviour {
    RUISPSMoveWand moveController;
    public GameObject bulletPrefab;
    public Transform bulletSpawnSpot;

    public float bulletSpeed = 1.0f;

    public float shootingInterval = 0.3f;
    float timeSinceLastShot = 0;

    void Awake()
    {
        moveController = GetComponent<RUISPSMoveWand>();
    }

	void Update () {
        timeSinceLastShot += Time.deltaTime;

        if (moveController.triggerValue > 0.85 && timeSinceLastShot >= shootingInterval)
        {
            timeSinceLastShot = 0;
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnSpot.position, bulletSpawnSpot.rotation) as GameObject;
            bullet.rigidbody.AddForce(bulletSpawnSpot.forward * bulletSpeed, ForceMode.VelocityChange);

            Destroy(bullet, 5);
        }
	}
}
