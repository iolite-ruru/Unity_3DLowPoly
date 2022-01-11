using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�߻�޼ҵ尡 �ϳ� �̻� �ֱ� ������ �߻�Ŭ������ ��������~
public abstract class CloseWeaponController : MonoBehaviour
{
    //���� ������ Hand�� Ÿ�� ����(��� ����� �ƴ�)
    [SerializeField]
    protected CloseWeapon currentCloseWeapon;

    //���� ��?
    private bool isAttack = false;
    protected bool isSwing = false;

    protected RaycastHit hitInfo;

    /*�߻�Ŭ�����̱� ������ Update() ������ �� => �ڽ� Ŭ������ �־���
    // Update is called once per frame
    void Update()
    {
        if (isActivate) TryAttack();
    }
    */
    protected void TryAttack()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack)
            {
                //�ڷ�ƾ ����
                StartCoroutine(AttackCoroutine());

            }
        }
    }
    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentCloseWeapon.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayA);
        isSwing = true;

        //���� Ȱ��ȭ ����
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayB);
        isSwing = false;

        yield return new WaitForSeconds(currentCloseWeapon.attackDelay - currentCloseWeapon.attackDelayA - currentCloseWeapon.attackDelayB);
        isAttack = false;
    }
    /* HitCoroutine()�� ��ӹ��� Ŭ�������� ������ �� �ֵ��� ����
    protected IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                //�浹����
                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }
    */
    protected abstract IEnumerator HitCoroutine();
    protected bool CheckObject()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range))
        {

            return true;
        }
        return false;
    }
    //�����Լ�. �ϼ� �Լ������� �߰� ������ ������ �Լ�
    public virtual void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        if (WeaponManager.currentWeapon != null)
            WeaponManager.currentWeapon.gameObject.SetActive(false);

        currentCloseWeapon = _closeWeapon;
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;

        currentCloseWeapon.transform.localPosition = Vector3.zero;
        currentCloseWeapon.gameObject.SetActive(true);
    }
}
