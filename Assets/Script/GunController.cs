using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    //Ȱ��ȭ ����
    public static bool isActivate = false;

    //���� ������ ��
    [SerializeField]
    private Gun currentGun;

    //���� �ӵ� ���
    private float currentFireRate;

    //���� ����
    private bool isReload = false;
    [HideInInspector]
    private bool isFineSightMode = false;

    //���� ������ ��
    //[SerializeField]
    private Vector3 originPos;

    //ȿ���� ���
    private AudioSource audioSource;

    //������ �浹 ���� �޾ƿ�
    private RaycastHit hitInfo;
    
    //�ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCam;
    private Crosshair theCrosshair;


    //�ǰ� ����Ʈ
    [SerializeField]
    private GameObject hit_effect_prefab;
    




    private void Start()
    {
        //theCam = GetComponent<Camera>();
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        //originPos = transform.localPosition; //SerializeField�� �ۼ��߱� ������ �� ���� ���� �ʿ�Xx
        theCrosshair = FindObjectOfType<Crosshair>();

        //WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        //WeaponManager.currentWeaponAnim = currentGun.anim;
    }

    // Update is called once per frame
    void Update()
    {
        if (isActivate)
        {
            GunFireRateClac();
            TryFire();
            TryReload();
            TryFineSight();
        }
    }



    //����ӵ� ����
    private void GunFireRateClac()
    {
        if (currentFireRate > 0) currentFireRate -= Time.deltaTime;
    }
    //�߻� �õ�
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload)
        {
            Fire();
        }
    }
    //�߻� �� ���
    private void Fire()
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
            {
                Shoot();
            }
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
        }
    }
    //�߻� �� ���
    private void Shoot()
    {
        theCrosshair.FireAnimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; //���� �ӵ� ����
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();
        //�ѱ� �ݵ� �ڷ�ƾ ����
        StopAllCoroutines();
        StartCoroutine(RetroActioinCoroutine());

        //Debug.Log("�Ѿ� �߻���");
    }
    private void Hit()
    {
        if(Physics.Raycast(theCam.transform.position, theCam.transform.forward +
            new Vector3(Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                        Random.Range(-theCrosshair.GetAccuracy() - currentGun.accuracy, theCrosshair.GetAccuracy() + currentGun.accuracy),
                        0),
            out hitInfo, currentGun.range))
        {
            var clone = Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
            //Debug.Log(hitInfo.transform.name);
        }
    }


    //������ �õ�
    private void TryReload()
    {
        if(Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }
    //������ ���
    public void CancelReload()
    {
        if (isReload)
        {
            StopAllCoroutines();
            isReload = false;
        }
    }
    //������
    IEnumerator ReloadCoroutine()
    {
        if(currentGun.carryBulletCount > 0)
        {
            isReload = true;
            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.reloadTime);

            if(currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }
            isReload = false;
        }
        else
        {
            Debug.Log("������ �Ѿ��� �����ϴ�.");
        }
    }
    //������ �õ�
    private void TryFineSight()
    {
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }
    //������ ���
    public void CancelFineSight()
    {
        if (isFineSightMode) FineSight();
    }
    //������ ���� ����
    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode); //�� ��ġ ������ �ִϸ��̼�
        theCrosshair.FineSightAnimation(isFineSightMode); //ũ�ν���� �ִϸ��̼�
        if (isFineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeactivateCoroutine());
        }
    }
    //������ Ȱ��ȭ
    IEnumerator FineSightActivateCoroutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginePos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginePos, 0.2f);
            yield return null;
        }
    }
    //������ ��Ȱ��ȭ
    IEnumerator FineSightDeactivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }
    //�ݵ�
    IEnumerator RetroActioinCoroutine()
    {
        //�� �ΰ��� Vector3�� �̸� ������ ��, Start�Լ����� ���� �Ű��ִ� ���� �� ����
        //�޸� ����ȭ ���� ����(�ڼ��� ���� 3�� ����)
        Vector3 recoiBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z);
        Vector3 retroActionRecoiBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginePos.z);

        if (!isFineSightMode)
        {
            currentGun.transform.localPosition = originPos;
            //�ݵ� ����
            while(currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoiBack, 0.4f);
                yield return null;
            }
            //����ġ
            while(currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }
        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginePos;
            //�ݵ� ����
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoiBack, 0.4f);
                yield return null;
            }
            //����ġ
            while (currentGun.transform.localPosition != currentGun.fineSightOriginePos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginePos, 0.1f);
                yield return null;
            }
        }
    }
    //���� ���
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }

    public Gun GetGun()
    {
        return currentGun;
    }
    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }
    public void GunChange(Gun _gun)
    {
        if (WeaponManager.currentWeapon != null)
            WeaponManager.currentWeapon.gameObject.SetActive(false);

        currentGun = _gun;
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;

        currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        isActivate = true;
    }
}
