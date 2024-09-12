using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        Live, Dead
    }
    public PlayerState playerState = PlayerState.Live;

    public float rotSpeed; // ī�޶� ȸ�� �ӵ�
    float mouseX, mouseY; // ���콺�� ����

    public Transform camAxis; // ī�޶� ��

    bool jumpSign = true; // ���� ����
    float speed = 5; // �̵� �ӵ� 
    float jumpPower = 10; // ������
    float gravity = -20; // �߷�
    float yVelocity = 0; // ����

    public CharacterController cc; // ĳ���� ��Ʈ�ѷ�
    public Animator anim; // �ִϸ�����
    public Transform playerCharacter; // �÷��̾� ĳ����

    // UI
    public Image hpGauge, mpGauge; // HP ������, MP ������
    public float playerHP; // �÷��̾� HP
    float targetHP; // ������ HP ��
    float maxHP; // �ִ� HP
    float hpDownSpeed = 50.0f; // HP ���� �ӵ�
    
    public enum PlayerHPState // �÷��̾� HP ����
    {
        None, HPDown
    }
    public PlayerHPState playerHPState = PlayerHPState.None;

    public float playerMP; // �÷��̾� MP
    float targetMP; // ������ MP ��
    float maxMP; // �ִ� MP
    float mpDownSpeed = 50.0f; // MP ���� �ӵ�
    public float mpCost; // MP ��뷮

    public enum PlayerMPState // �÷��̾� MP ����
    {
        None, MPDown
    }
    public PlayerMPState playerMPState = PlayerMPState.None;

    // Ÿ�� UI
    public GameObject targetUI; // Ÿ�� UI
    public Text targetName; // Ÿ���� �̸�
    public Image targetHPGauge; // Ÿ���� HP ������

    // �÷��̾� ����
    public enum PlayerAttackState
    {
        None = -1, leftAttack, rightAttack
    }
    public PlayerAttackState playerAttackState = PlayerAttackState.None; // �÷��̾� ���� ����
    public List<AnimationClip> punchAttack = new();
    float playerAttackTime; // �÷��̾� ���� �ð�
    public List<float> punchDelay = new(); // ��ġ ������ Ÿ��
    public enum WeaponState
    {
        None, leftAttack, rightAttack
    }
    public WeaponState weaponState = WeaponState.None; // ���� ���� ����
    public List<Transform> punchWeapon = new(); // ��ġ Ʈ������
    public float punchRange; // ��ġ ���� ����
    public float punchDamage; // ��ġ ���ݷ�

    // �κ��丮
    public Inventory inventory;

    // ������ ���� ����
    public Transform jointItem;
    GameObject handWeapon; // ��� �ִ� ����

    // ����
    public GameObject magicPrefab;
    public Transform magicPos;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(punchWeapon[0].position, punchRange);
        Gizmos.DrawWireSphere(punchWeapon[1].position, punchRange);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        mouseX = camAxis.rotation.eulerAngles.y;
        mouseY = camAxis.rotation.eulerAngles.x;

        targetHP = playerHP; // ��ǥ HP�� �÷��̾� HP�� �ʱ�ȭ
        maxHP = playerHP; // �ִ� HP�� �÷��̾� HP�� �ʱ�ȭ

        targetMP = playerMP;
        maxMP = playerMP;
    }

    void Update()
    {
        switch (playerState)
        {
            case PlayerState.Live:
                CameraRotate(); // ī�޶� ȸ��
                PlayerMove(); // �÷��̾� �̵�
                PlayerAttack(); // �÷��̾� ����
                PlayerStatus(); // �÷��̾� ����
                PlayerSight(); // �÷��̾� �þ�
                PlayerGetItem(); // ������ ȹ��
                PlayerSpell(); // �÷��̾� ����
                CursorState();
                break;
        }
    }

    void CursorState()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            if (!Cursor.visible)
            {
                Cursor.visible = true;
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl)) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// ī�޶� ȸ�� �Լ�
    /// </summary>
    void CameraRotate()
    {
        float h = Input.GetAxis("Mouse X"); // ���콺 X�� �Է�
        float v = Input.GetAxis("Mouse Y"); // ���콺 Y�� �Է�

        mouseX += h * rotSpeed * Time.deltaTime; // ���콺 X������ �����Ͽ� ���
        mouseY += v * rotSpeed * Time.deltaTime; // ���콺 Y������ �����Ͽ� ���

        mouseY = Mathf.Clamp(mouseY, -70.0f, 20.0f);

        camAxis.eulerAngles = new Vector3(-mouseY, mouseX, 0); // ī�޶� ȸ��
        playerCharacter.eulerAngles = new Vector3(0, mouseX, 0); // ĳ���� ȸ��
    }

    /// <summary>
    /// �÷��̾� �̵� �Լ�
    /// </summary>
    void PlayerMove()
    {
        float h = Input.GetAxis("Horizontal"); // ���� Ű �Է�
        float v = Input.GetAxis("Vertical"); // ���� Ű �Է�

        // ���� �޸��� �ִϸ��̼� �Լ� ȣ��
        RunForwardAnim(h, v);

        Vector3 dir = new Vector3(h, 0, v) * speed;
        dir = camAxis.TransformDirection(dir);

        // ĳ���� ��Ʈ�ѷ��� ���� ��� ���� ��
        if (cc.isGrounded && !jumpSign)
        {
            anim.SetTrigger("Jump"); // ���� �ִϸ��̼� ����
            yVelocity = 0;
            jumpSign = true;
        }

        // ���� ��ư�� ������ ��
        if (Input.GetButtonDown("Jump") && jumpSign)
        {
            anim.SetTrigger("Jump"); // ���� �ִϸ��̼� ����
            yVelocity = jumpPower;
            jumpSign = false;
        }

        // �߷� ����
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // �÷��̾� �̵�
        cc.Move(dir * Time.deltaTime);
    }

    /// <summary>
    /// ���� �Լ�
    /// </summary>
    void PlayerAttack()
    {
        switch (playerAttackState)
        {
            case PlayerAttackState.None:
                // ���� ���콺 ��ư�� �����ٸ�
                if (Input.GetMouseButtonDown(0))
                {
                    anim.SetTrigger("PunchLeft"); // ���� ���� �ִϸ��̼� ȣ��
                    weaponState = WeaponState.leftAttack; // �÷��̾� ���� ���¸� �������� ����
                    playerAttackState = PlayerAttackState.leftAttack;
                }
                // ������ ���콺 ��ư�� �����ٸ�
                else if (Input.GetMouseButtonDown(1))
                {
                    anim.SetTrigger("PunchRight"); // ������ ���� �ִϸ��̼� ȣ��
                    weaponState = WeaponState.rightAttack; // �÷��̾� ���� ���¸� �������� ����
                    playerAttackState = PlayerAttackState.rightAttack;
                }
                break;
            case PlayerAttackState.leftAttack:
                playerAttackTime += Time.deltaTime; // �÷��̾� ���� �ð��� �帣�� ��
                if (playerAttackTime >= punchDelay[(int)playerAttackState])
                {
                    // ���� ���� �Լ� ȣ��
                    AttackCastOn();
                }
                // �÷��̾� ���� �ð��� ��ġ �ִϸ��̼� ���̺��� ũ�ų� ������ ��
                if (playerAttackTime >= punchAttack[(int)playerAttackState].length)
                {
                    playerAttackTime = 0; // �÷��̾� ���� �ð� �ʱ�ȭ
                    weaponState = WeaponState.None; // ���� ���� �ʱ�ȭ
                    playerAttackState = PlayerAttackState.None; // �÷��̾� ���� ���� �ʱ�ȭ
                }
                break;
            case PlayerAttackState.rightAttack:
                playerAttackTime += Time.deltaTime; // �÷��̾� ���� �ð��� �帣�� ��
                if (playerAttackTime >= punchDelay[(int)playerAttackState])
                {
                    // ���� ���� �Լ� ȣ��
                    AttackCastOn();
                }
                // �÷��̾� ���� �ð��� ��ġ �ִϸ��̼� ���̺��� ũ�ų� ������ ��
                if (playerAttackTime >= punchAttack[(int)playerAttackState].length)
                {
                    playerAttackTime = 0; // �÷��̾� ���� �ð� �ʱ�ȭ
                    weaponState = WeaponState.None; // ���� ���� �ʱ�ȭ
                    playerAttackState = PlayerAttackState.None; // �÷��̾� ���� ���� �ʱ�ȭ
                }
                break;
        }
    }

    /// <summary>
    /// �÷��̾� ���� �Լ�
    /// </summary>
    void PlayerStatus()
    {
        // �÷��̾� HP ����
        switch (playerHPState)
        {
            // �÷��̾��� HP�� �����ϰ� �ִ� ����
            case PlayerHPState.HPDown:
                // Mathf.MoveTowards(���� ��, ��ǥ ��, ��ȭ�� �ӵ�)
                playerHP = Mathf.MoveTowards(playerHP, targetHP, hpDownSpeed * Time.deltaTime);
                float perHP = playerHP / maxHP;
                hpGauge.fillAmount = perHP;
                // �÷��̾��� HP���� targetHP�� ��� ��������
                if (playerHP == targetHP)
                {
                    if (playerHP > 0)
                    {
                        hpGauge.fillAmount = perHP;
                        // �÷��̾��� ���¸� None���� �ʱ�ȭ
                        playerHPState = PlayerHPState.None;
                    }
                    else
                    {
                        hpGauge.fillAmount = 0;
                        anim.SetTrigger("Dead");
                        StartCoroutine(RPGGameReset());
                        playerHPState = PlayerHPState.None;
                        playerState = PlayerState.Dead;
                    }
                }
                break;
        }

        switch (playerMPState)
        {
            case PlayerMPState.MPDown:
                {
                    playerMP = Mathf.MoveTowards(playerMP, targetMP, (mpDownSpeed * 3) * Time.deltaTime);
                    float perMP = playerMP / maxMP;
                    mpGauge.fillAmount = perMP;
                    if (playerMP == targetMP)
                    {
                        perMP = playerMP / maxMP;
                        mpGauge.fillAmount = perMP;
                        playerMPState = PlayerMPState.None;
                    }
                    break;
                }
            case PlayerMPState.None:
                {
                    playerMP = Mathf.MoveTowards(playerMP, maxMP, (mpDownSpeed / 2) * Time.deltaTime);
                    targetMP = playerMP;
                    float perMP = playerMP / maxMP;
                    mpGauge.fillAmount = perMP;
                    break;
                }
        }
    }

    /// <summary>
    /// ���� �̵� �ִϸ��̼� �Լ�
    /// </summary>
    /// <param name="h">Horizontal</param>
    /// <param name="v">Vertical</param>
    void RunForwardAnim(float h, float v)
    {
        anim.SetFloat("Right", h);
        anim.SetFloat("Forward", v);
    }

    /// <summary>
    /// �÷��̾ ���� �޾��� �� HP�� �����ϴ� �Լ�
    /// </summary>
    /// <param name="damage"></param>
    public void PlayerHPDown(float damage)
    {
        targetHP -= damage; // �������� �� ��ŭ targetHP���� ����.
        playerHPState = PlayerHPState.HPDown; // �÷��̾� HP ���¸� HPDown ���·� ����
    }

    /// <summary>
    /// �÷��̾� �þ� �Լ�
    /// </summary>
    void PlayerSight()
    {
        int layerMask = 1 << 7 | 1 << 8; // ���� ���̾�(7)�� ������ ���̾�(8)�� �ν��ϵ��� �Ѵ�.
        RaycastHit hit; // �浹 ����
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, layerMask))
        {
            switch (hit.transform.gameObject.layer)
            {
                case 7:
                    targetUI.SetActive(true);
                    targetName.text = hit.transform.GetComponent<Monster>().GetMonsterName(); // Ÿ���� �̸� ��������
                    targetHPGauge.fillAmount = hit.transform.GetComponent<Monster>().GetMonsterHP(); // Ÿ���� HP���� ��������
                    break;
                case 8:
                    targetUI.SetActive(true);
                    targetName.text = hit.transform.GetComponent<ItemSocket>().ItemView(); // ������ �̸� ��������
                    break;
            }
        }
        else
        {
            targetName.text = "";
            targetUI.SetActive(false);
        }
    }

    /// <summary>
    /// �÷��̾� ���� ���� On �Լ�
    /// </summary>
    void AttackCastOn()
    {
        switch (weaponState)
        {
            case WeaponState.leftAttack:
                {
                    int layerMask = 1 << 7; // 7�� ���̾ ����
                                            // hits �迭�� ���Ǿ�ĳ��Ʈ ������ ���� (��ġ, ������, ����, �Ÿ�, ���̾��ũ)
                    RaycastHit[] hits = Physics.SphereCastAll(punchWeapon[(int)playerAttackState].position, punchRange, Vector3.up, 0, layerMask);

                    // �浹�ϴ� ��ü�� �ִٸ�
                    if (hits.Length > 0)
                    {
                        foreach (RaycastHit hit in hits)
                        {
                            hit.transform.SendMessage("MonsterHPDown", punchDamage); // �浹�� ���(����)���� MonsterHPDown �Լ��� ã�� ȣ��
                            weaponState = WeaponState.None;
                            break;
                        }
                    }
                    break;
                }
            case WeaponState.rightAttack:
                {
                    int layerMask = 1 << 7; // 7�� ���̾ ����
                                            // hits �迭�� ���Ǿ�ĳ��Ʈ ������ ���� (��ġ, ������, ����, �Ÿ�, ���̾��ũ)
                    RaycastHit[] hits = Physics.SphereCastAll(punchWeapon[(int)playerAttackState].position, punchRange, Vector3.up, 0, layerMask);

                    // �浹�ϴ� ��ü�� �ִٸ�
                    if (hits.Length > 0)
                    {
                        if (handWeapon)
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                hit.transform.SendMessage("MonsterHPDown", punchDamage + 30); // �浹�� ���(����)���� MonsterHPDown �Լ��� ã�� ȣ��
                                weaponState = WeaponState.None;
                                break;
                            }
                        }
                        else
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                hit.transform.SendMessage("MonsterHPDown", punchDamage); // �浹�� ���(����)���� MonsterHPDown �Լ��� ã�� ȣ��
                                weaponState = WeaponState.None;
                                break;
                            }
                        }
                    }
                    break;
                }
        }
    }

    void PlayerGetItem()
    {
        int layerMask = 1 << 8; // ������ ���̾�(8)�� �ν��ϵ��� �Ѵ�.
        RaycastHit hit; // �浹 ����
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, layerMask))
        {
            switch (hit.transform.gameObject.layer)
            {
                case 8:
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        inventory.itemDropSocket = hit.transform.parent.parent.gameObject;
                        inventory.InventoryUpdate(hit.transform.gameObject);
                        Destroy(hit.transform.gameObject);
                    }
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventory.inventory.activeSelf)
            {
                inventory.InventoryOpen(false);
            }
            else
            {
                inventory.InventoryOpen(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="g">����</param>
    public void ItemEquip(GameObject g)
    {
        if (handWeapon)
        {
            Destroy(handWeapon);
            handWeapon = null;
        }
        GameObject weapon = Instantiate(g, jointItem.position, jointItem.rotation, jointItem);
        weapon.transform.localEulerAngles = new Vector3(-90.0f, 180.0f, 0.0f);
        handWeapon = weapon;
    }

    /// <summary>
    /// ���� ����
    /// </summary>
    void PlayerSpell()
    {
        if (targetMP >= mpCost)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameObject magic = Instantiate(magicPrefab, magicPos.position, magicPos.rotation);
                targetMP -= mpCost;
                playerMPState = PlayerMPState.MPDown;
            }
        }
    }

    IEnumerator RPGGameReset()
    {
        yield return new WaitForSeconds(3.0f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
