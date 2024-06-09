using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Player�� Transform
    public Transform player { get; private set; }
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
    // ���ݷ�
    public float atk = 10f;
    // �ִ��̵�����
    public float maxMoveDistance = 20f;
    // �ʱ� ��ġ��
    public Vector3 originPos { get; private set; }
    // ���� ü��
    float currentHealth = 100f;
    //
    public Animator anim { get; private set; }

    // ���� ����
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

// ���� �������̽�
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}

// ��� ����
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

// �̵� ����
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

// ���� ����
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

// ��ȯ ����
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

// ���� ����
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

// �ǰ� ����
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