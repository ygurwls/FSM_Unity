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
        Damaged,
        Die
    }

    // EnemyState����
    EnemyState enemyState;
    // Player�� Transform
    Transform player;
    // �� ���� ����
    public float IdealRange = 8;
    // ���� ��Ÿ�
    public float AttackRange = 2;
    // �̵� �ӷ�
    public float Speed = 2;
    // ���� ������
    public float attackDelay = 1;
    // ���� �ð�
    float currTime = 0;
    // ���ݷ�
    public float atk = 10;

    // Start is called before the first frame update
    void Start()
    {
        enemyState = EnemyState.Idle;
        
        player = GameObject.Find("Player").transform;
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
                player.GetComponent<PlayerController>().TakeDamage(atk);
                currTime = 0;
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

    }

    void Damaged()
    {

    }

    void Die()
    {

    }
}
