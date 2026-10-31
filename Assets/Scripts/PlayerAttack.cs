using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject AircraftBody;
    public GameObject GunBarrelRight;
    public GameObject BulletPrefab;
    public GameObject KamikazeWeaponPrefab;
    public float KamikazeDelay;
    public float ShootDelay;
    public float BurstDelay;
    public float LightDuration;
    public Statistics Statistics;
    private float kamikazeDelayTime;
    private float shootDelayTime;
    private float burstDelayTime;
    private float lightDurationTime = 0.2f;
    private Light GunBarrelRightLight;
    private FireController fireController;
    private int burstCapacity;
    private int shotsFired;
    private KamikazeController kamikazeController;
    private ButtonLoader buttonLoader;
    private IEnumerable<BoxCollider> boxColliders;

    void Start()
    {
        boxColliders = GetComponentsInParent<BoxCollider>();
        shootDelayTime = Time.time;
        burstDelayTime = Time.time;
        GunBarrelRightLight = GunBarrelRight.GetComponent<Light>();
        fireController = GameObject.FindGameObjectWithTag(Tags.FireController).GetComponent<FireController>();
        kamikazeController = GameObject.FindGameObjectWithTag(Tags.KamikazeController).GetComponent<KamikazeController>();
        buttonLoader = GameObject.FindGameObjectWithTag(Tags.KamikazeButton).GetComponent<ButtonLoader>();
    }

    void Update()
    {
        if (shotsFired == 0 && fireController.IsFirePressed())
        {
            burstDelayTime = Time.time + BurstDelay;
            burstCapacity = 3;
        }

        if (shotsFired < burstCapacity && Time.time > shootDelayTime)
        {
            shootDelayTime = Time.time + ShootDelay;
            lightDurationTime = Time.time + LightDuration;
            Shoot();
            EnableLightEffect();
        }

        if (shotsFired >= burstCapacity && Time.time > burstDelayTime)
        {
            Reload();
        }

        if (Time.time > lightDurationTime)
        {
            DisableLightEffect();
        }

        if (kamikazeController.IsKamikazeFireTouched() && Time.time > kamikazeDelayTime)
        {
            buttonLoader.LoadButton();
            kamikazeDelayTime = Time.time + KamikazeDelay;
            KamikazeShoot();
        }
    }

    private void KamikazeShoot()
    {
        SetColliders(false);
        Instantiate(KamikazeWeaponPrefab, AircraftBody.transform.position, AircraftBody.transform.rotation);
        StartCoroutine(EnableColliders());
    }

    private void SetColliders(bool status)
    {
        foreach (var boxCollider in boxColliders)
        {
            boxCollider.enabled = status;
        }
    }

    private IEnumerator EnableColliders()
    {
        yield return new WaitForSeconds(1);
        SetColliders(true);
    }

    public void Shoot()
    {
        Statistics.BurstsFired++;
        shotsFired++;
        Instantiate(BulletPrefab, GunBarrelRight.transform.position, GunBarrelRight.transform.rotation);
    }

    private void EnableLightEffect()
    {
        GunBarrelRightLight.enabled = true;
    }

    private void DisableLightEffect()
    {
        GunBarrelRightLight.enabled = false;
    }

    private void Reload()
    {
        burstCapacity = 0;
        shotsFired = 0;
    }
}
