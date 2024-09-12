using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public enum MonsterState
    {
        IDLE, MOVE, ATTACK, DEAD, DEADDOWN
    }
    public MonsterState monsterState = MonsterState.IDLE;
    public CharacterController cc;
    public Transform target;
    public Animator monsterAnim;

    public float targetRange; //타겟 거리 감지
    public float attackRange;
    public float weaponRange; //무기 유효 범위

    public float enemySpeed;
    public float rotSpeed;
    float attackTime;
    float setAttackTime;
    int rndAtk, oldRndAtk;

    public List<AnimationClip> attackClip = new();
    public int addAnimNum;
    public Transform weaponPos;

    public enum WeaponState
    {
        None, Attack
    }
    public WeaponState weaponState = WeaponState.None;
    public float weaponDamage;
    public List<float> attackDelay = new();

    public enum MonsterHPState
    {
        None, HPDown
    }
    public MonsterHPState monsterHPState = MonsterHPState.None;
    public string monsterName;
    public string GetMonsterName()
    {
        return monsterName;
    }
    public float monsterHP;
    float monsterTargetHP, monsterMaxHP;
    float monsterPerHP;
    public float GetMonsterHP()
    {
        return monsterPerHP;
    }
    float monsterHPDownSpeed = 70;

    public AnimationClip monsterDeadClip;
    float monsterDeadTime = 0;

    public ItemDropManager itemDropManager;

    //기즈모를 그리는 함수
    private void OnDrawGizmos()
    {
        //와이어 스피어 그림(중심, 반지름)
        Gizmos.DrawWireSphere(transform.position, targetRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.DrawWireSphere(weaponPos.position, weaponRange);
    }

    private void Start()
    {
        monsterTargetHP = monsterHP;
        monsterMaxHP = monsterHP;
        monsterPerHP = monsterHP / monsterMaxHP;
    }

    private void Update()
    {
        MonsterAction();
        MonsterStatus();
    }

    void MonsterAction()
    {
        switch (monsterState)
        {
            case MonsterState.IDLE:
                {
                    int layerMask = 1 << 6; //6번 레이러 대입

                    //hits 배열에 스피어캐스트 정보를 담는다(위치, 반지름, 방향, 거리, 레이어마스크)
                    RaycastHit[] hits = Physics.SphereCastAll(transform.position, targetRange, Vector3.up, 0, layerMask);
                    if (hits.Length > 0)
                    {
                        foreach (var hit in hits)
                        {
                            target = hit.transform;
                        }
                        if (target)
                        {
                            MonsterAnimUpdate(1);
                            monsterState = MonsterState.MOVE;
                        }
                    }
                    break;
                }
            case MonsterState.MOVE:
                {
                    if (target)
                    {
                        //타겟을 바라보는 기능
                        Vector3 relationPos = new Vector3(target.position.x, 0, target.position.z)
                                            - new Vector3(transform.position.x, 0, transform.position.z);
                        Quaternion rotation = Quaternion.LookRotation(relationPos);

                        //RotateTowards : 대상을 향해서 일정한 속도로 회전.(원본, 회전할 목표, 회전 속도)
                        transform.rotation = Quaternion.RotateTowards
                                            (transform.rotation, rotation, rotSpeed * Time.deltaTime);

                        cc.SimpleMove(transform.forward * enemySpeed); //몬스터이동

                        float distance = Vector3.Distance(transform.position, target.position);

                        if (distance <= attackRange)
                        {
                            rndAtk = Random.Range(0, attackClip.Count);
                            MonsterAnimUpdate(rndAtk + addAnimNum);
                            setAttackTime = attackClip[rndAtk].length; //애니메이션클립 시간을 대입
                            oldRndAtk = rndAtk;
                            weaponState = WeaponState.Attack;
                            monsterState = MonsterState.ATTACK;
                        }

                        if (distance > targetRange)
                        {
                            MonsterAnimUpdate(0);
                            monsterState = MonsterState.IDLE;
                        }
                    }
                    break;
                }
            case MonsterState.ATTACK:
                {
                    attackTime += Time.deltaTime;

                    //몬스터 공격 시간이 해당 공격의 지연시간보다 클 경우
                    if (attackTime >= attackDelay[rndAtk])
                    {
                        AttackCastOn();
                    }

                    if (attackTime >= setAttackTime)
                    {
                        //과거 공격 번호가 과거 공격 번호와 같다면 다시 한번 뽑기
                        while (rndAtk == oldRndAtk)
                        {
                            rndAtk = Random.Range(0, attackClip.Count);
                        }
                        MonsterAnimUpdate(rndAtk + addAnimNum);
                        setAttackTime = attackClip[rndAtk].length;
                        oldRndAtk = rndAtk;
                        attackTime = 0;
                        weaponState = WeaponState.Attack;
                    }

                    float distance = Vector3.Distance(transform.position, target.position);
                    if (distance > attackRange)
                    {
                        MonsterAnimUpdate(1);
                        monsterState = MonsterState.MOVE;
                    }
                    break;
                }
            case MonsterState.DEAD:
                {
                    monsterDeadTime += Time.deltaTime;
                    if (monsterDeadTime >= monsterDeadClip.length * 2)
                    {
                        itemDropManager.ItemDropStart(transform.position);
                        monsterDeadTime = 0;
                        monsterState = MonsterState.DEADDOWN;
                    }
                    break;
                }
            case MonsterState.DEADDOWN:
                {
                    transform.Translate(0, (-enemySpeed / 2) * Time.deltaTime, 0);
                    monsterDeadTime += Time.deltaTime;
                    if (monsterDeadTime >= monsterDeadClip.length * 2)
                        Destroy(gameObject);
                    break;
                }
        }
    }

    void MonsterStatus()
    {
        switch (monsterHPState)
        {
            case MonsterHPState.HPDown:
                monsterHP = Mathf.MoveTowards(monsterHP, monsterTargetHP, monsterHPDownSpeed * Time.deltaTime);
                monsterPerHP = monsterHP / monsterMaxHP;
                if (monsterHP == monsterTargetHP)
                {
                    if (monsterHP > 0)
                    {
                        monsterPerHP = monsterHP / monsterMaxHP;
                        monsterHP = monsterTargetHP;
                        monsterHPState = MonsterHPState.None;
                    }
                    else
                    {
                        monsterHP = 0;
                        monsterPerHP = monsterHP / monsterMaxHP;
                        MonsterAnimUpdate(5);
                        monsterState = MonsterState.DEAD;
                        monsterHPState = MonsterHPState.None;
                    }
                }
                break;
        }
    }

    void MonsterAnimUpdate(int i)
    {
        monsterAnim.SetInteger("MonsterState", i);
    }

    void AttackCastOn()
    {
        switch (weaponState)
        {
            case WeaponState.Attack:
                int layerMask = 1 << 6; //6번 레이러 대입

                //hits 배열에 스피어캐스트 정보를 담는다(위치, 반지름, 방향, 거리, 레이어마스크)
                RaycastHit[] hits = Physics.SphereCastAll
                                            (weaponPos.position, weaponRange, Vector3.up, 0, layerMask);
                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        hit.transform.SendMessage("PlayerHPDown", weaponDamage);
                        weaponState = WeaponState.None;
                    }
                }
                break;
        }
    }

    public void MonsterHPDown(float damage)
    {
        monsterTargetHP -= damage;
        monsterHPState = MonsterHPState.HPDown;
    }
}
