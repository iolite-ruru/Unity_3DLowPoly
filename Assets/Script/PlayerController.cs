using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //���ǵ� ���� ����
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed;

    [SerializeField]
    private float jumpForce;

    //���� ����
    private bool isWalk = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    //������ üũ ����
    private Vector3 lastPos;

    //�ɾ��� �� �󸶳� ������ �����ϴ� ����
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    //�� ���� ����
    private CapsuleCollider capsuleCollider;

    //ī�޶� �ΰ���
    [SerializeField]
    private float lookSensitivity;

    //ī�޶� ���� ����
    [SerializeField]
    private float cameraRatationLimit; //����
    private float currentCameraRotationX = 0; //����. �����ص� �⺻���� 0��.

    //�ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCamera; //ī�޶� ������Ʈ �ҷ���. ����ȭ��. ��? ī�޶�ü�� �÷��̾ü�� �ִ°� �ƴ�. �� �ڽ����� ����.
    private Rigidbody myRigid;
    private GunController theGunController;
    private Crosshair theCrosshair;


    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        //theCamera = FindObjectOfType<Camera>(); //��� ��ü ������ ī�޶� ������Ʈ ������ �־���. ������ ������? ������.. �׷��ϱ� �ø���������ʵ� ���ڴٰ�~
        myRigid = GetComponent<Rigidbody>(); // 9����(private Rigidbody myRigid;) ���� [SerializeField]�� �ν�����â���� �ű�� �Ͱ� ���� ȿ��
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();

        //�ʱ�ȭ
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y; //originPosY = transform.position.y;�� �ƴ�!! �갡 �ٲ�� �÷��̾ ���� ����.
        //���� ī�޶�� �÷��̾ ��������. ������� ��ǥ ����� ���� position�� �ƴ� localPosition ���
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun();
        TryCrouch();
        Move();
        MoveCheck();
        CameraRotation();
        CharacterRotation();
    }


    //�ɱ� �õ�
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }
    //������ �ɴ� �Լ�
    private void Crouch()
    {
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);
        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        StartCoroutine(CrouchCoroutine()); //�����Ÿ� ���� �� �ڿ������� ������� �ϱ� ����!
    }
    //�ε巴�� �ɴ� ����
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;
        while(_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15) break; //�� ���ǹ��� ���ٸ� ��� ������
            yield return null; //null=�� ������ ���
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }
    private void IsGround()
    {
        //���⼭ -transfrom.up �� ���� �ȴٸ� ������ ����. ������ ���� ���͸� ����!
        //(�� ��ġ����, ��� ��������, �� �Ÿ���ŭ)
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrosshair.JumpingAnimation(!isGround);
    }
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }
    private void Jump()
    {
        if (isCrouch)
            Crouch(); //���� ���¿��� �����ϸ� ���� ���� ����
        myRigid.velocity = transform.up * jumpForce;
    }
    //�޸��� �õ�
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }
    //�޸��� ����
    private void Running()
    {
        if (isCrouch) Crouch();

        theGunController.CancelFineSight();

        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;
    }
    //�޸��� ���
    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    //������ ����
    private void Move()
    {
        float _moveDirX = Input.GetAxisRaw("Horizontal"); //�Է��� �Ͼ�� -1(��), 0(�Է�x), 1(��) ���ϵ�. Horizontal�� ����Ƽ �⺻ ����
        float _moveDirZ = Input.GetAxisRaw("Vertical"); //�Է��� �Ͼ�� -1(��), 0(�Է�x), 1(��) ���ϵ�

        Vector3 _moveHorizontal = transform.right * _moveDirX; //transform? ������Ʈ(�ν����� â ��ܿ� ��ġ)�� �������ִ� ��ġ�� � ����Ʈ�� ���ڴ�.
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed; //���⿡ �ӵ� ������. ���� ���� �����ذ��� ����
        //normalized ��� ����? ��ǥ��� �׷�����. xz(1, 1), xz(0.5, 0.5)�� ����. ����ȭ ����.

        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
        //Time.deltaTime(�� 0.016) ��� ����? 1�ʵ��� _velocity��ŭ �̵���Ű�� ����. �׷��� ������ �����̵�����
    }
    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            if (Vector3.Distance(lastPos, transform.position) >= 0.01f) isWalk = true;
            else isWalk = false;
            lastPos = transform.position;

            theCrosshair.WalkingAnimation(isWalk);
        }
        
    }
    private void CameraRotation()
    {
        //���� ī�޶� ȸ��
        float _xRotation = Input.GetAxisRaw("Mouse Y"); //1, -1 ���ϵ�
        float _cameraRotationX = _xRotation * lookSensitivity; //�ΰ����� ���� ȭ���� õõ�� �����̵��� ����
        currentCameraRotationX -= _cameraRotationX;
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRatationLimit, cameraRatationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
    private void CharacterRotation()
    {
        //�¿� ĳ���� ȸ��
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
        //���ο����� ���ʹ�(4����) ������ �̷����. �츮�� ���� ���ϰ� ���Ϸ�(3����) ����.���� �鸮�´�� �Ἥ �̸� �̰� �ƴҼ���.
    }
}
