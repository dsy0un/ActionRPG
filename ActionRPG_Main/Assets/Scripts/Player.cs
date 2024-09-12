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

    public float rotSpeed; // 카메라 회전 속도
    float mouseX, mouseY; // 마우스의 각도

    public Transform camAxis; // 카메라 축

    bool jumpSign = true; // 점프 사인
    float speed = 5; // 이동 속도 
    float jumpPower = 10; // 점프력
    float gravity = -20; // 중력
    float yVelocity = 0; // 높이

    public CharacterController cc; // 캐릭터 컨트롤러
    public Animator anim; // 애니메이터
    public Transform playerCharacter; // 플레이어 캐릭터

    // UI
    public Image hpGauge, mpGauge; // HP 게이지, MP 게이지
    public float playerHP; // 플레이어 HP
    float targetHP; // 감소할 HP 양
    float maxHP; // 최대 HP
    float hpDownSpeed = 50.0f; // HP 감소 속도
    
    public enum PlayerHPState // 플레이어 HP 상태
    {
        None, HPDown
    }
    public PlayerHPState playerHPState = PlayerHPState.None;

    public float playerMP; // 플레이어 MP
    float targetMP; // 감소할 MP 양
    float maxMP; // 최대 MP
    float mpDownSpeed = 50.0f; // MP 감소 속도
    public float mpCost; // MP 사용량

    public enum PlayerMPState // 플레이어 MP 상태
    {
        None, MPDown
    }
    public PlayerMPState playerMPState = PlayerMPState.None;

    // 타켓 UI
    public GameObject targetUI; // 타켓 UI
    public Text targetName; // 타켓의 이름
    public Image targetHPGauge; // 타켓의 HP 게이지

    // 플레이어 공격
    public enum PlayerAttackState
    {
        None = -1, leftAttack, rightAttack
    }
    public PlayerAttackState playerAttackState = PlayerAttackState.None; // 플레이어 공격 상태
    public List<AnimationClip> punchAttack = new();
    float playerAttackTime; // 플레이어 공격 시간
    public List<float> punchDelay = new(); // 펀치 딜레이 타임
    public enum WeaponState
    {
        None, leftAttack, rightAttack
    }
    public WeaponState weaponState = WeaponState.None; // 무기 공격 상태
    public List<Transform> punchWeapon = new(); // 펀치 트랜스폼
    public float punchRange; // 펀치 공격 범위
    public float punchDamage; // 펀치 공격력

    // 인벤토리
    public Inventory inventory;

    // 아이템 장착 공간
    public Transform jointItem;
    GameObject handWeapon; // 들고 있는 무기

    // 마법
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

        targetHP = playerHP; // 목표 HP를 플레이어 HP로 초기화
        maxHP = playerHP; // 최대 HP를 플레이어 HP로 초기화

        targetMP = playerMP;
        maxMP = playerMP;
    }

    void Update()
    {
        switch (playerState)
        {
            case PlayerState.Live:
                CameraRotate(); // 카메라 회전
                PlayerMove(); // 플레이어 이동
                PlayerAttack(); // 플레이어 공격
                PlayerStatus(); // 플레이어 상태
                PlayerSight(); // 플레이어 시야
                PlayerGetItem(); // 아이템 획득
                PlayerSpell(); // 플레이어 마법
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
    /// 카메라 회전 함수
    /// </summary>
    void CameraRotate()
    {
        float h = Input.GetAxis("Mouse X"); // 마우스 X축 입력
        float v = Input.GetAxis("Mouse Y"); // 마우스 Y축 입력

        mouseX += h * rotSpeed * Time.deltaTime; // 마우스 X각도를 누적하여 계산
        mouseY += v * rotSpeed * Time.deltaTime; // 마우스 Y각도를 누적하여 계산

        mouseY = Mathf.Clamp(mouseY, -70.0f, 20.0f);

        camAxis.eulerAngles = new Vector3(-mouseY, mouseX, 0); // 카메라를 회전
        playerCharacter.eulerAngles = new Vector3(0, mouseX, 0); // 캐릭터 회전
    }

    /// <summary>
    /// 플레이어 이동 함수
    /// </summary>
    void PlayerMove()
    {
        float h = Input.GetAxis("Horizontal"); // 가로 키 입력
        float v = Input.GetAxis("Vertical"); // 세로 키 입력

        // 전방 달리기 애니메이션 함수 호출
        RunForwardAnim(h, v);

        Vector3 dir = new Vector3(h, 0, v) * speed;
        dir = camAxis.TransformDirection(dir);

        // 캐릭터 컨트롤러가 땅에 닿고 있을 때
        if (cc.isGrounded && !jumpSign)
        {
            anim.SetTrigger("Jump"); // 점프 애니메이션 실행
            yVelocity = 0;
            jumpSign = true;
        }

        // 점프 버튼을 눌렀을 때
        if (Input.GetButtonDown("Jump") && jumpSign)
        {
            anim.SetTrigger("Jump"); // 점프 애니메이션 실행
            yVelocity = jumpPower;
            jumpSign = false;
        }

        // 중력 적용
        yVelocity += gravity * Time.deltaTime;
        dir.y = yVelocity;

        // 플레이어 이동
        cc.Move(dir * Time.deltaTime);
    }

    /// <summary>
    /// 공격 함수
    /// </summary>
    void PlayerAttack()
    {
        switch (playerAttackState)
        {
            case PlayerAttackState.None:
                // 왼쪽 마우스 버튼을 누른다면
                if (Input.GetMouseButtonDown(0))
                {
                    anim.SetTrigger("PunchLeft"); // 왼쪽 공격 애니메이션 호출
                    weaponState = WeaponState.leftAttack; // 플레이어 무기 상태를 공격으로 변경
                    playerAttackState = PlayerAttackState.leftAttack;
                }
                // 오른쪽 마우스 버튼을 누른다면
                else if (Input.GetMouseButtonDown(1))
                {
                    anim.SetTrigger("PunchRight"); // 오른쪽 공격 애니메이션 호출
                    weaponState = WeaponState.rightAttack; // 플레이어 무기 상태를 공격으로 변경
                    playerAttackState = PlayerAttackState.rightAttack;
                }
                break;
            case PlayerAttackState.leftAttack:
                playerAttackTime += Time.deltaTime; // 플레이어 공격 시간을 흐르게 함
                if (playerAttackTime >= punchDelay[(int)playerAttackState])
                {
                    // 몬스터 공격 함수 호출
                    AttackCastOn();
                }
                // 플레이어 공격 시간이 펀치 애니메이션 길이보다 크거나 같아질 때
                if (playerAttackTime >= punchAttack[(int)playerAttackState].length)
                {
                    playerAttackTime = 0; // 플레이어 공격 시간 초기화
                    weaponState = WeaponState.None; // 무기 상태 초기화
                    playerAttackState = PlayerAttackState.None; // 플레이어 공격 상태 초기화
                }
                break;
            case PlayerAttackState.rightAttack:
                playerAttackTime += Time.deltaTime; // 플레이어 공격 시간을 흐르게 함
                if (playerAttackTime >= punchDelay[(int)playerAttackState])
                {
                    // 몬스터 공격 함수 호출
                    AttackCastOn();
                }
                // 플레이어 공격 시간이 펀치 애니메이션 길이보다 크거나 같아질 때
                if (playerAttackTime >= punchAttack[(int)playerAttackState].length)
                {
                    playerAttackTime = 0; // 플레이어 공격 시간 초기화
                    weaponState = WeaponState.None; // 무기 상태 초기화
                    playerAttackState = PlayerAttackState.None; // 플레이어 공격 상태 초기화
                }
                break;
        }
    }

    /// <summary>
    /// 플레이어 상태 함수
    /// </summary>
    void PlayerStatus()
    {
        // 플레이어 HP 상태
        switch (playerHPState)
        {
            // 플레이어의 HP가 감소하고 있는 상태
            case PlayerHPState.HPDown:
                // Mathf.MoveTowards(현재 값, 목표 값, 변화할 속도)
                playerHP = Mathf.MoveTowards(playerHP, targetHP, hpDownSpeed * Time.deltaTime);
                float perHP = playerHP / maxHP;
                hpGauge.fillAmount = perHP;
                // 플레이어의 HP양이 targetHP의 양과 같아지면
                if (playerHP == targetHP)
                {
                    if (playerHP > 0)
                    {
                        hpGauge.fillAmount = perHP;
                        // 플레이어의 상태를 None으로 초기화
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
    /// 전방 이동 애니메이션 함수
    /// </summary>
    /// <param name="h">Horizontal</param>
    /// <param name="v">Vertical</param>
    void RunForwardAnim(float h, float v)
    {
        anim.SetFloat("Right", h);
        anim.SetFloat("Forward", v);
    }

    /// <summary>
    /// 플레이어가 공격 받았을 때 HP가 감소하는 함수
    /// </summary>
    /// <param name="damage"></param>
    public void PlayerHPDown(float damage)
    {
        targetHP -= damage; // 피해입은 양 만큼 targetHP에서 뺀다.
        playerHPState = PlayerHPState.HPDown; // 플레이어 HP 상태를 HPDown 상태로 변경
    }

    /// <summary>
    /// 플레이어 시야 함수
    /// </summary>
    void PlayerSight()
    {
        int layerMask = 1 << 7 | 1 << 8; // 몬스터 레이어(7)와 아이템 레이어(8)만 인식하도록 한다.
        RaycastHit hit; // 충돌 정보
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 20.0f, layerMask))
        {
            switch (hit.transform.gameObject.layer)
            {
                case 7:
                    targetUI.SetActive(true);
                    targetName.text = hit.transform.GetComponent<Monster>().GetMonsterName(); // 타켓의 이름 가져오기
                    targetHPGauge.fillAmount = hit.transform.GetComponent<Monster>().GetMonsterHP(); // 타켓의 HP정보 가져오기
                    break;
                case 8:
                    targetUI.SetActive(true);
                    targetName.text = hit.transform.GetComponent<ItemSocket>().ItemView(); // 아이템 이름 가져오기
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
    /// 플레이어 공격 센서 On 함수
    /// </summary>
    void AttackCastOn()
    {
        switch (weaponState)
        {
            case WeaponState.leftAttack:
                {
                    int layerMask = 1 << 7; // 7번 레이어를 대입
                                            // hits 배열에 스피어캐스트 정보를 담음 (위치, 반지름, 방향, 거리, 레이어마스크)
                    RaycastHit[] hits = Physics.SphereCastAll(punchWeapon[(int)playerAttackState].position, punchRange, Vector3.up, 0, layerMask);

                    // 충돌하는 물체가 있다면
                    if (hits.Length > 0)
                    {
                        foreach (RaycastHit hit in hits)
                        {
                            hit.transform.SendMessage("MonsterHPDown", punchDamage); // 충돌한 대상(몬스터)에게 MonsterHPDown 함수를 찾아 호출
                            weaponState = WeaponState.None;
                            break;
                        }
                    }
                    break;
                }
            case WeaponState.rightAttack:
                {
                    int layerMask = 1 << 7; // 7번 레이어를 대입
                                            // hits 배열에 스피어캐스트 정보를 담음 (위치, 반지름, 방향, 거리, 레이어마스크)
                    RaycastHit[] hits = Physics.SphereCastAll(punchWeapon[(int)playerAttackState].position, punchRange, Vector3.up, 0, layerMask);

                    // 충돌하는 물체가 있다면
                    if (hits.Length > 0)
                    {
                        if (handWeapon)
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                hit.transform.SendMessage("MonsterHPDown", punchDamage + 30); // 충돌한 대상(몬스터)에게 MonsterHPDown 함수를 찾아 호출
                                weaponState = WeaponState.None;
                                break;
                            }
                        }
                        else
                        {
                            foreach (RaycastHit hit in hits)
                            {
                                hit.transform.SendMessage("MonsterHPDown", punchDamage); // 충돌한 대상(몬스터)에게 MonsterHPDown 함수를 찾아 호출
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
        int layerMask = 1 << 8; // 아이템 레이어(8)만 인식하도록 한다.
        RaycastHit hit; // 충돌 정보
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
    /// <param name="g">무기</param>
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
    /// 마법 공격
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
