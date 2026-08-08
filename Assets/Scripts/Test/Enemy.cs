using RECode.REFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        EventCenter.Instance.AddListener("EnemyPatrol",Patrol);
        EventCenter.Instance.AddListener("EnemyChase", Chase);
        EventCenter.Instance.AddFuncListener<bool>("CheckPlayer",isFindPlayer);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener("EnemyPatrol", Patrol);
        EventCenter.Instance.RemoveListener("EnemyChase", Chase);
        EventCenter.Instance.RemoveFuncListener<bool>("CheckPlayer", isFindPlayer);
    }

    private void Start()
    {
        currentPos = pos1;
    }

    private void Update()
    {
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
