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
        BackToHome,
        Frozen,
        Damaged,
        Die
    }

    // EnemyState����
    EnemyState enemyState;
    // Player�� Transform
    Transform player;
    // �� ���� ����
    public float IdealRange = 8f;
    // ���� ��Ÿ�
    public float AttackRange = 2f;
    // �̵� �ӷ�
    public float Speed = 2f;
    // ȸ�� �ӷ�
    public float rotSpeed = 2f;
    // ���� ������
    public float attackDelay = 1f;
    // ���� �ð�
    float currTime = 0f;
    // ���ݷ�
    public float atk = 10f;
    // �ִ��̵�����
    public float maxMoveDistance = 20f;
    // �ʱ� ��ġ��
    Vector3 originPos;
    // ���� ü��
    float currentHealth = 100f;
    //
    Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        enemyState = EnemyState.Idle;
        
        player = GameObject.Find("Player").transform;

        originPos = transform.position;

        anim = GetComponent<Animator>();
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
            case EnemyState.BackToHome:
                BackToHome();
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
            anim.SetBool("IsMoving", true);
        }
    }

    void Move()
    {
        if(Vector3.Distance(transform.position, player.position) > maxMoveDistance)
        {
            enemyState = EnemyState.BackToHome;
            print("State: Move->Return");
        }

        float dist = Vector3.Distance(transform.position, player.position);
        if(dist < AttackRange)
        {
            enemyState = EnemyState.Attack;
            print("State: Move->Attack");
            anim.SetBool("IsAttack", true);
        }
        else
        {
            Vector3 dir = player.position - transform.position;
            dir.Normalize();

            transform.position += dir * Speed * Time.deltaTime;

            Quaternion rotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotSpeed * Time.fixedDeltaTime);
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
            anim.SetBool("IsAttack", false);
        }
    }

    void BackToHome()
    {
        float dist = Vector3.Distance(transform.position, originPos);

        if (dist > 0.1f)
        {
            Vector3 dir = originPos - transform.position;
            dir.Normalize();
            transform.position += dir * Speed * Time.deltaTime;
            Quaternion rotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotSpeed * Time.fixedDeltaTime);
        }
        else
        {
            enemyState = EnemyState.Idle;
            print("State: Return->Idle");
            anim.SetBool("IsMoving", false);
        }
    }

    void Frozen()
    {
        StartCoroutine(DamagedCoroutine());
        print("State: Frozen->Move");
        enemyState = EnemyState.Move;
        anim.SetBool("IsMoving", true);
    }

    void Damaged()
    {
        enemyState = EnemyState.Frozen;
        print("State: Damaged->Frozen");
        anim.SetTrigger("IsDamaged");
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
        Destroy(gameObject, 1f);
        anim.SetBool("IsDead", true);
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
