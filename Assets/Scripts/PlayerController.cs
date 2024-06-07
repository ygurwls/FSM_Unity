using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update

    private Vector3 dir = Vector3.zero;
    private float speed = 10.0f;
    private float rotSpeed = 20.0f;
    private Rigidbody playerRb;
    //플레이어 현재 체력
    float currentHealth = 100;
    // 공격력
    public float attackPower = 10f;
    // Enemy의 Transform
    Transform enemy;

    void Start()
    {
        playerRb = this.GetComponent<Rigidbody>();
        enemy = GameObject.Find("Enemy").transform;
    }

    // Update is called once per frame
    void Update()
    {
        dir.x = Input.GetAxis("Horizontal");
        dir.z = Input.GetAxis("Vertical");
        dir.Normalize();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }
    private void FixedUpdate()
    {
        if(dir != Vector3.zero)
        {
            if (Mathf.Sign(transform.forward.x) != Mathf.Sign(dir.x) || Mathf.Sign(transform.forward.z) != Mathf.Sign(dir.z))
            {
                transform.Rotate(0, 1, 0);
            }
            transform.forward = Vector3.Lerp(transform.forward, dir, rotSpeed * Time.deltaTime);
        }

        playerRb.MovePosition(this.gameObject.transform.position + dir * speed * Time.deltaTime);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        print("PlayerCurrentHealth = " + currentHealth);
    }

    void Attack()
    {
        // 적이 있는지 확인
        if (enemy != null)
        {
            // 적의 Enemy 스크립트 참조
            Enemy enemyScript = enemy.GetComponent<Enemy>();

            // 적에게 데미지 입히기
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackPower);
            }
        }
    }
}
