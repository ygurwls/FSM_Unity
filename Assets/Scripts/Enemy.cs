using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    enum EnemyState
    {
        Idle,
        Move,
        Attack,
        Return,
        Frozen,
        Damaged,
        Die
    }

    // EnemyState변수
    EnemyState enemyState;
    // Player의 Transform
    Transform player;
    // 적 인지 범위
    public float IdealRange = 8f;
    // 공격 사거리
    public float AttackRange = 2f;
    // 이동 속력
    public float Speed = 2f;
    // 공격 딜레이
    public float attackDelay = 1f;
    // 누적 시간
    float currTime = 0f;
    // 공격력
    public float atk = 10f;
    // 최대이동범위
    public float maxMoveDistance = 20f;
    // 초기 위치값
    Vector3 originPos;
    // 현재 체력
    float currentHealth = 100f;

    // Start is called before the first frame update
    void Start()
    {
        enemyState = EnemyState.Idle;
        
        player = GameObject.Find("Player").transform;

        originPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        switch(enemyState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Move:
                Move();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.Return:
                Return();
                break;
            case EnemyState.Frozen:
                Frozen();
                break;
            case EnemyState.Damaged:
                Damaged();
                break;
            case EnemyState.Die:
                Die();
                break;
            default:
                break;
        }
    }

    void Idle()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < IdealRange)
        {
            enemyState = EnemyState.Move;
            print("State: Idle->Move");
        }
    }

    void Move()
    {
        if(Vector3.Distance(transform.position, player.position) > maxMoveDistance)
        {
            enemyState = EnemyState.Return;
            print("State: Move->Return");
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if(dist < AttackRange)
        {
            enemyState = EnemyState.Attack;
            print("State: Move->Attack");
        }
        else
        {
            Vector3 dir = player.position - transform.position;
            dir.Normalize();

            transform.position += dir * Speed * Time.deltaTime;
        }
    }

    void Attack()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < AttackRange)
        {
            currTime += Time.deltaTime;
            if(currTime > attackDelay)
            {
                print("Attack");
                if (player != null)
                {
                    PlayerController PC = player.GetComponent<PlayerController>();

                    if (PC != null)
                    {
                        PC.TakeDamage(atk);
                    }
                    currTime = 0;
                }
            }
        }
        else
        {
            enemyState = EnemyState.Move;
            print("State: Attack->Move");
        }
    }

    void Return()
    {
        float dist = Vector3.Distance(transform.position, originPos);

        if (dist > 0.1f)
        {
            Vector3 dir = originPos - transform.position;
            dir.Normalize();
            transform.position += dir * Speed * Time.deltaTime;
        }
        else
        {
            enemyState = EnemyState.Idle;
            print("State: Return->Idle");
        }
    }

    void Frozen()
    {
        StartCoroutine(DamagedCoroutine());
        print("State: Frozen->Move");
        enemyState = EnemyState.Move;
    }

    void Damaged()
    {
        enemyState = EnemyState.Frozen;
        print("State: Damaged->Frozen");
    }

    IEnumerator DamagedCoroutine()
    {

        float originalSpeed = Speed;
        Speed = 0f;

        yield return new WaitForSeconds(1f);

        Speed = originalSpeed;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            print("State: AnyState -> Die");
            Die();
        }
        else
        {
            enemyState = EnemyState.Damaged;
            print("State: AnyState -> Damaged");
            print("EnemyCurrentHealth = " + currentHealth);
            Damaged();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, IdealRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, maxMoveDistance);
    }

}
