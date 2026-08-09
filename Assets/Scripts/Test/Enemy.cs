using RECode.REFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private BehaviorTreeAsset treeAsset;
    [SerializeField]
    private Transform pos1;
    [SerializeField]
    private Transform pos2;
    [SerializeField]
    private float patrolSpeed;
    [SerializeField]
    private float chaseSpeed;
    [SerializeField]
    private float checkPlayerRadius;

    private BehaviorTree tree;
    private Rigidbody2D rb2D;
    private Transform currentPos;
    private Transform playerPos;

    private void Awake()
    {
        tree=treeAsset.GetTree();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        tree.blackboard.SetValue<UnityAction>("EnemyPatrol",Patrol);
        tree.blackboard.SetValue<UnityAction>("EnemyChase", Chase);
    }

    private void OnDisable()
    {
        tree.blackboard.RemoveKey("EnemyPatrol");
        tree.blackboard.RemoveKey("EnemyChase");
        tree.blackboard.RemoveKey("CheckPlayer");
    }

    private void Start()
    {
        currentPos = pos1;
    }

    private void Update()
    {
        tree.blackboard.SetValue<bool>("CheckPlayer", isFindPlayer());
        tree.Tick();
    }

    public void Patrol()
    {
        Vector2 dir = (currentPos.position - transform.position).normalized;
        rb2D.velocity=dir*patrolSpeed;
        if(Vector2.Distance(transform.position,currentPos.position)<0.1f)
        {
            currentPos=currentPos==pos1 ? pos2 : pos1;
        }
    }

    public bool isFindPlayer()
    {
        Collider2D collider= Physics2D.OverlapCircle(transform.position, checkPlayerRadius, LayerMask.GetMask("Player"));
        if(collider!=null)
        {
            playerPos = collider.transform;
        }
        return collider != null;
    }

    public void Chase()
    {
        Vector2 dir = (playerPos.position - transform.position).normalized;
        rb2D.velocity=dir*chaseSpeed;
    }

    public void Stay()
    {
        Debug.Log("Stay");
        rb2D.velocity = Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color= Color.red;
        Gizmos.DrawWireSphere(transform.position, checkPlayerRadius);
    }
}
