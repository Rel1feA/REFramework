using RECode.REFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RECode.Controller2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private float speed;
        [SerializeField]
        private float jumpForce;
        [SerializeField]
        private LayerMask groundLayer;
        [SerializeField]
        [Range(0f, 2f)]
        private float DownGravityScaleMultiply;

        private Rigidbody2D rb2D;
        private Collider2D col2D;
        private float inputX;
        private Vector2 groundBoxOrigin;
        private Vector2 groundBoxSize;
        private float normalGravityScale;

        private void Awake()
        {
            rb2D = GetComponent<Rigidbody2D>();
            col2D = GetComponent<Collider2D>();
        }

        private void Start()
        {
            HandleInput();
            SetGroundBoxValue();
            normalGravityScale=rb2D.gravityScale;
        }

        private void Update()
        {
            GetInput();
            SetGroundBoxValue();
        }

        private void FixedUpdate()
        {
            ChangeGravityScale();
            Move(inputX);
        }

        public void Move(float input)
        {
            rb2D.velocity=new Vector2(input*speed,rb2D.velocity.y);
        }

        public void Jump()
        {
            if(isOnGround())
            {
                rb2D.velocity += Vector2.up * jumpForce;
            }
        }

        public void GetInput()
        {
            inputX = InputManager.Instance.GetFloat(InputConstants.Action_Move);
        }

        public void HandleInput()
        {
            InputManager.Instance.BindAction(InputConstants.Action_Jump, Jump, InteractionType.Performed);
        }

        public bool isOnGround()
        {
            return Physics2D.OverlapBox(groundBoxOrigin,groundBoxSize, 0, groundLayer);
        }

        public void SetGroundBoxValue()
        {
            groundBoxOrigin = (Vector2)col2D.bounds.center + Vector2.down * col2D.bounds.extents.y;
            groundBoxSize = new Vector2((col2D.bounds.extents.x - 0.1f) * 2, 0.2f);
        }

        public void ChangeGravityScale()
        {
            if(rb2D.velocity.y<0&&!isOnGround())
            {
                rb2D.gravityScale = normalGravityScale * DownGravityScaleMultiply;
            }
            else
            {
                rb2D.gravityScale=normalGravityScale;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundBoxOrigin,groundBoxSize);
        }
    }
    
}


