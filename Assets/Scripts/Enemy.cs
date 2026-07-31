using UnityEngine;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(Health))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private Player _target;
    [SerializeField] private Animator _animator;

    public Vector3 boxSize = new Vector3(2f, 2f, 2f); 
    public float forwardOffset = 1.5f;                
    public LayerMask enemyLayer;

    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _damage = 10f;
    private float _nextAttackTime = 0f;

    private NavMeshAgent _agent; 
    private Health _health;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public event Action<Enemy> Died;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        _health.Died += ReleaseEnemy;
    }
    
    private void OnDisable()
    {
        _health.Died -= ReleaseEnemy;
    }

    private void Update()
    {
        if (_target == null) return;

        _agent.SetDestination(_target.transform.position);

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (Time.time >= _nextAttackTime)
            {
                TriggerAttack();
            }
        }
    }

    private void TriggerAttack()
    {
        _agent.isStopped = true; 

        _animator.SetTrigger(AttackHash);

        _nextAttackTime = Time.time + _attackCooldown;
        CheckHitBox();
    }

    public void CheckHitBox()
    {
        Vector3 boxCenter = transform.position + transform.forward * forwardOffset;

        Collider[] hitEnemies = Physics.OverlapBox(boxCenter, boxSize / 2f, transform.rotation, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            enemy.GetComponent<Health>().TakeDamage(_damage);
        }

        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.forward * forwardOffset, boxSize);
        Gizmos.matrix = originalMatrix;
    }

    public void SetBoss(int bossHP)
    {
        transform.localScale = new Vector3(2f, 2f, 1f);
        _health.SetBoss(bossHP);
        _agent.speed = 5f;
    }

    public void SetEnemy()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
        _health.SetEnemy();
        _agent.speed = 3.8f;
    }

    private void ReleaseEnemy()
    {
        Died?.Invoke(this);
    }
}
