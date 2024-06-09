using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Player의 Transform
    public Transform player { get; private set; }
    // 적 인지 범위
    public float IdealRange = 8f;
    // 공격 사거리
    public float AttackRange = 2f;
    // 이동 속력
    public float Speed = 2f;
    // 회전 속력
    public float rotSpeed = 2f;
    // 공격 딜레이
    public float attackDelay = 1f;
    // 공격력
    public float atk = 10f;
    // 최대이동범위
    public float maxMoveDistance = 20f;
    // 초기 위치값
    public Vector3 originPos { get; private set; }
    // 현재 체력
    float currentHealth = 100f;
    //
    public Animator anim { get; private set; }

    // 현재 상태
    IState currentState;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        originPos = transform.position;
        anim = GetComponent<Animator>();

        currentState = new IdleState(this);
    }

    void Update()
    {
        currentState.Execute();
    }

    public void TransitionState(IState newState)
    {
        if (currentState != null)
        {
            Debug.Log($"State Transition: {currentState.GetType().Name} -> {newState.GetType().Name}");
        }
        else
        {
            Debug.Log($"Initial State: {newState.GetType().Name}");
        }

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            TransitionState(new DieState(this));
        }
        else
        {
            TransitionState(new DamagedState(this));
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

// 상태 인터페이스
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}

// 대기 상태
public class IdleState : IState
{
    Enemy enemy;

    public IdleState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {

    }

    public void Execute()
    {
        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (dist < enemy.IdealRange)
        {
            enemy.TransitionState(new MoveState(enemy));
        }
    }

    public void Exit()
    {
       
    }
}

// 이동 상태
public class MoveState : IState
{
    Enemy enemy;

    public MoveState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.anim.SetBool("IsMoving", true);
    }

    public void Execute()
    {
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) > enemy.maxMoveDistance)
        {
            enemy.TransitionState(new BackToHomeState(enemy));
        }

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (dist < enemy.AttackRange)
        {
            enemy.TransitionState(new AttackState(enemy));
        }
        else
        {
            Vector3 dir = enemy.player.position - enemy.transform.position;
            dir.Normalize();

            enemy.transform.position += dir * enemy.Speed * Time.deltaTime;

            Quaternion rotation = Quaternion.LookRotation(dir);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, rotation, enemy.rotSpeed * Time.fixedDeltaTime);
        }
    }

    public void Exit()
    {
       
    }
}

// 공격 상태
public class AttackState : IState
{
    Enemy enemy;
    float currTime = 0f;

    public AttackState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.anim.SetBool("IsAttack", true);
    }

    public void Execute()
    {
        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);
        if (dist < enemy.AttackRange)
        {
            currTime += Time.deltaTime;
            if (currTime > enemy.attackDelay)
            {
                Debug.Log("Attack");
                if (enemy.player != null)
                {
                    PlayerController PC = enemy.player.GetComponent<PlayerController>();

                    if (PC != null)
                    {
                        PC.TakeDamage(enemy.atk);
                    }
                    currTime = 0;
                }
            }
        }
        else
        {
            enemy.TransitionState(new MoveState(enemy));
        }
    }

    public void Exit()
    {
        enemy.anim.SetBool("IsAttack", false);
    }
}

// 귀환 상태
public class BackToHomeState : IState
{
    Enemy enemy;

    public BackToHomeState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        
    }

    public void Execute()
    {
        float dist = Vector3.Distance(enemy.transform.position, enemy.originPos);

        if (dist > 0.1f)
        {
            Vector3 dir = enemy.originPos - enemy.transform.position;
            dir.Normalize();
            enemy.transform.position += dir * enemy.Speed * Time.deltaTime;
            Quaternion rotation = Quaternion.LookRotation(dir);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, rotation, enemy.rotSpeed * Time.fixedDeltaTime);
        }
        else
        {
            enemy.TransitionState(new IdleState(enemy));
        }
    }

    public void Exit()
    {
        enemy.anim.SetBool("IsMoving", false);
    }
}

// 얼음 상태
public class FrozenState : IState
{
    Enemy enemy;

    public FrozenState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.StartCoroutine(DamagedCoroutine());
    }

    public void Execute()
    {
        enemy.TransitionState(new MoveState(enemy));
    }

    public void Exit()
    {
        
    }

    IEnumerator DamagedCoroutine()
    {
        float originalSpeed = enemy.Speed;
        enemy.Speed = 0f;

        yield return new WaitForSeconds(1f);

        enemy.Speed = originalSpeed;
    }
}
public class DieState : IState
{
    Enemy enemy;

    public DieState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.anim.SetBool("IsDead", true);
    }

    public void Execute()
    {
        UnityEngine.Object.Destroy(enemy.gameObject, 1f);
    }

    public void Exit()
    {
        
    }
}

// 피격 상태
public class DamagedState : IState
{
    Enemy enemy;

    public DamagedState(Enemy enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        enemy.anim.SetTrigger("IsDamaged");
        enemy.TransitionState(new FrozenState(enemy));
    }

    public void Execute()
    {
        
    }

    public void Exit()
    {
        
    }
}