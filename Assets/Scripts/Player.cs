using UnityEngine;
using System.Collections;
using System;

public delegate void DeadEventHandler();

public class Player : Character
{
    private static Player instance;

    private IUseable useable;

    private int coins;

    [SerializeField]
    private float climbSpeed;

    private Vector2 startPos;

    public event DeadEventHandler Dead;

    [SerializeField]
    private Transform[] groundPoints;

    [SerializeField]
    private float groundRadius;

    [SerializeField]
    private LayerMask whatIsGround;

    [SerializeField]
    private bool airControl;

    [SerializeField]
    private float jumpForce;

    private bool immortal = false;

    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private float immortalTime;

    private float direction;

    private bool move;

    private float btnHorizontal;

    public Rigidbody2D MyRigidbody { get; set; }

    public bool Slide { get; set; }

    public bool Jump { get; set; }

    public bool OnGround { get; set; }

    public bool OnLadder { get; set; }

    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<Player>();
            }
            return instance;
        }
    }

    public override bool IsDead
    {
        get
        {
            if (healthStat.CurrentValue <= 0)
            {
                OnDead();
            }

            return healthStat.CurrentValue <= 0;
        }
    }

    // Use this for initialization
    public override void Start()
    {
        base.Start();
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        MyRigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!TakingDamage && !IsDead)
        {
            if (transform.position.y <= -14f)
            {
                Death();

            }
            HandleInput();
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!TakingDamage && !IsDead)
        {
            float horizontal = Input.GetAxis("Horizontal");

            float vertical = Input.GetAxis("Vertical");

            OnGround = IsGrounded();

            if (move)
            {
                this.btnHorizontal = Mathf.Lerp(btnHorizontal, direction, Time.deltaTime * 2);
                //HandleMovement(btnHorizontal);
                Flip(direction);
            }
            else
            {
                HandleMovement(horizontal,vertical);

                Flip(horizontal);
            }



            HandleLayers();
        }

    }


    private void Use()
    {
        if (useable != null)
        {
            useable.Use();
        }
    }

    public void OnDead()
    {
        if (Dead != null)
        {
            Dead();
        }
    }



    private void HandleMovement(float horizontal, float vertical)
    {
        if (MyRigidbody.velocity.y < 0)
        {
            MyAnimator.SetBool("land", true);
        }
        if (!Attack && !Slide && (OnGround || airControl))
        {
            MyRigidbody.velocity = new Vector2(horizontal * movementSpeed, MyRigidbody.velocity.y);
        }
        if (Jump && MyRigidbody.velocity.y == 0 && !OnLadder)
        {
            MyRigidbody.AddForce(new Vector2(0, jumpForce));
        }
        if (OnLadder)
        {
            MyAnimator.speed = vertical != 0 ? Mathf.Abs(vertical) : Mathf.Abs(horizontal);
            MyRigidbody.velocity = new Vector2(horizontal * climbSpeed, vertical * climbSpeed);
        }

        MyAnimator.SetFloat("speed", Mathf.Abs(horizontal));
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !OnLadder)
        {
            MyAnimator.SetTrigger("jump");
            Jump = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            MyAnimator.SetTrigger("attack");
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            MyAnimator.SetTrigger("slide");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            MyAnimator.SetTrigger("throw");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Use();
        }
    }

    private void Flip(float horizontal)
    {
        if (horizontal > 0 && !facingRight || horizontal < 0 && facingRight)
        {
            ChangeDirection();
        }
    }

    private bool IsGrounded()
    {
        if (MyRigidbody.velocity.y <= 0)
        {
            foreach (Transform point in groundPoints)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(point.position, groundRadius, whatIsGround);

                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i].gameObject != gameObject)
                    {
                        return true;
                    }
                }

            }
        }
        return false;
    }



    private void HandleLayers()
    {
        if (!OnGround)
        {
            MyAnimator.SetLayerWeight(1, 1);
        }
        else
        {
            MyAnimator.SetLayerWeight(1, 0);
        }
    }

    public override void ThrowKnife(int value)
    {
        if (!OnGround && value == 1 || OnGround && value == 0)
        {
            base.ThrowKnife(value);
        }
    }

    private IEnumerator IndicateImmortal()
    {
        while (immortal)
        {
            spriteRenderer.enabled = false;

            yield return new WaitForSeconds(.1f);

            spriteRenderer.enabled = true;

            yield return new WaitForSeconds(.1f);
        }
    }

    public override IEnumerator TakeDamage()
    {
        if (!immortal)
        {
           healthStat.CurrentValue -= 10;

            if (!IsDead)
            {
                MyAnimator.SetTrigger("damage");
                immortal = true;

                StartCoroutine(IndicateImmortal());
                yield return new WaitForSeconds(immortalTime);

                immortal = false;
            }
            else
            {
                MyAnimator.SetLayerWeight(1, 0);
                MyAnimator.SetTrigger("die");
            }

        }
    }

    public override void Death()
    {
        MyRigidbody.velocity = Vector2.zero;
        MyAnimator.SetTrigger("idle");
        healthStat.CurrentValue = healthStat.MaxVal;
        transform.position = startPos;
    }

    public void BtnJump()
    {
        MyAnimator.SetTrigger("jump");
        Jump = true;
    }

    public void BtnAttack()
    {
        MyAnimator.SetTrigger("attack");
    }

    public void BtnSlide()
    {
        MyAnimator.SetTrigger("slide");
    }

    public void BtnTrow()
    {
        MyAnimator.SetTrigger("throw");
    }

    public void BtnMove(float direction)
    {
        this.direction = direction;
        this.move = true;
    }

    public void BtnStopMove()
    {
        this.direction = 0;
        this.btnHorizontal = 0;
        this.move = false;
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Coin")
        {
            GameManager.Instance.CollectedCoins++;
            Destroy(other.gameObject);
        }
    }

    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Useable")
        {
            useable = other.GetComponent<IUseable>();
        }

        base.OnTriggerEnter2D(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Useable")
        {
            useable = null;
        }
    }

}
