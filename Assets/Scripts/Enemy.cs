﻿using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// The enemy's class
/// </summary>
public class Enemy : Character
{

    /// <summary>
    /// The enemy's current state
    /// changing this will change the enemys behaviour
    /// </summary>
    private IEnemyState currentState;

    /// <summary>
    /// The enemy's target
    /// </summary>
    public GameObject Target { get; set; }

    /// <summary>
    /// The enemy's melee range, at what range does the enemy need to use the sword
    /// </summary>
    [SerializeField]
    private float meleeRange;

    /// <summary>
    /// The enemy's throw range, how far can it start throwing knifes
    /// </summary>
    [SerializeField]
    private float throwRange;

    private Vector3 startPos;

    [SerializeField]
    private Transform leftEdge;

    [SerializeField]
    private Transform rightEdge;

    private Canvas healthCavas;

    /// <summary>
    /// Indicates if the enemy is in melee range
    /// </summary>
    public bool InMeleeRange
    {
        get
        {
            if (Target != null)
            {
                return Vector2.Distance(transform.position, Target.transform.position) <= meleeRange;

            }

            return false;
        }
    }

    /// <summary>
    /// Indicates if the enemy is in throw range
    /// </summary>
    public bool InThrowRange
    {
        get
        {
            if (Target != null)
            {
                return Vector2.Distance(transform.position, Target.transform.position) <= throwRange;

            }

            return false;
        }
    }

    /// <summary>
    /// Indicates if the enemy is dead
    /// </summary>
    public override bool IsDead
    {
        get
        {
            return healthStat.CurrentValue <= 0;
        }
    }

    // Use this for initialization
    public override void Start()
    {   //Calls the base start
        base.Start();

        this.startPos = transform.position;

        //Makes the RemoveTarget function listen to the player's Dead event
        Player.Instance.Dead += new DeadEventHandler(RemoveTarget);

        //Sets the enemy in idle state
        ChangeState(new IdleState());

        healthCavas = transform.GetComponentInChildren<Canvas>();
    }



    // Update is called once per frame
    void Update()
    {
        if (!IsDead) //If the enemy i alive
        {
            if (!TakingDamage) //if we aren't taking damage
            {
                //Execute the current state, this can make the enemy move or attack etc.
                currentState.Execute();
            }

            //Makes the enemy look at his target
            LookAtTarget();
        }

    }

    /// <summary>
    /// Removes the enemy's target
    /// </summary>
    public void RemoveTarget()
    {
        //Removes the target
        Target = null;

        //Changes the state to a patrol state
        ChangeState(new PatrolState());
    }

    /// <summary>
    /// Makes the enemy look at the target
    /// </summary>
    private void LookAtTarget()
    {
        //If we have a target
        if (Target != null)
        {
            //Calculate the direction
            float xDir = Target.transform.position.x - transform.position.x;

            //If we are turning the wrong way
            if (xDir < 0 && facingRight || xDir > 0 && !facingRight)
            {
                //Look in the right direction
                ChangeDirection();
            }
        }
    }

    /// <summary>
    /// Changes the enemy's state
    /// </summary>
    /// <param name="newState">The new state</param>
    public void ChangeState(IEnemyState newState)
    {
        //If we have a current state
        if (currentState != null)
        {
            //Call the exit function on the state
            currentState.Exit();
        }

        //Sets the current state as the new state
        currentState = newState;

        //Calls the enter function on the current state
        currentState.Enter(this);
    }

    /// <summary>
    /// Moves the enemy
    /// </summary>
	/// 
	/// 
	/// <summary>
	/// Gets the current direction
	/// </summary>
	/// <returns>The direction</returns>
	public Vector2 GetDirection()
	{
		return facingRight ? Vector2.right : Vector2.left;
	}

	/// <summary>
	/// If the enemy collides with an object
	/// </summary>
    public void Move()
    {
        //If the enemy isn't attacking
        if (!Attack)
        {
            if ((GetDirection().x > 0 && transform.position.x < rightEdge.position.x) || (GetDirection().x < 0 && transform.position.x > leftEdge.position.x))
            {
                //Sets the speed to 1 to player the move animation
                MyAnimator.SetFloat("speed", 1);

                //Moves the enemy in the correct direction
				if (GetDirection ().x > 0) {
					transform.Translate (GetDirection () * (movementSpeed * Time.deltaTime));
				} else {
					transform.Translate (GetDirection () * (-movementSpeed * Time.deltaTime));

				}
				print (GetDirection());
            }
            else if (currentState is PatrolState)
            {
                ChangeDirection();
            }

        }

    }

    /// <param name="other">The colliding object</param>
    public override void OnTriggerEnter2D(Collider2D other)
    {
        //calls the base on trigger enter
        base.OnTriggerEnter2D(other);

        //Calls OnTriggerEnter on the current state
        currentState.OnTriggerEnter(other);
    }

    /// <summary>
    /// Makes the enemy take damage
    /// </summary>
    /// <returns></returns>
    public override IEnumerator TakeDamage()
    {
        if (!healthCavas.isActiveAndEnabled)
        {
            healthCavas.enabled = true;
        }

        //Reduces the health
        healthStat.CurrentValue -= 10;

        if (!IsDead) //If the enemy isn't dead then play the damage animation
        {
            MyAnimator.SetTrigger("damage");
        }
        else //If the enemy is dead then make sure that we play the dead animation
        {
            GameObject coin = (GameObject)Instantiate(GameManager.Instance.CoinPrefab, new Vector3(transform.position.x, transform.position.y + 5), Quaternion.identity);
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), coin.GetComponent<Collider2D>());
            MyAnimator.SetTrigger("die");
            yield return null;
        }
    }

    /// <summary>
    /// Removes the enemy from the game
    /// </summary>
    public override void Death()
    {
        MyAnimator.ResetTrigger("die");
        MyAnimator.SetTrigger("idle");
        healthStat.CurrentValue = healthStat.MaxVal;
        transform.position = startPos;
        healthCavas.enabled = false;
    }

    public override void ChangeDirection()
    {
        Transform tmp = transform.FindChild("EnemyHealthBarCanvas").transform;
        Vector3 pos = tmp.position;
        tmp.SetParent(null);

        base.ChangeDirection();

        tmp.SetParent(transform);
        tmp.position = pos;
    }
}
