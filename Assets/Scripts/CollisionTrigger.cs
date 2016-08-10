using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the platform collision
/// </summary>
public class CollisionTrigger : MonoBehaviour
{

    /// <summary>
    /// The platform collider
    /// </summary>
    [SerializeField]
    private BoxCollider2D platformCollider;

    /// <summary>
    /// The platform's trigger
    /// </summary>
    [SerializeField]
    private BoxCollider2D platformTrigger;

    /// <summary>
    /// If it collides with something
    /// </summary>
    /// <param name="other">The colliding object</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        //If the player collides with the platform
        if (other.tag == "Player" || other.tag == "Enemy") 
        {
            //Then ignore collision
            Physics2D.IgnoreCollision(platformCollider, other, true);
        }
    }

    /// <summary>
    /// When a trigger collision stops
    /// </summary>
    /// <param name="other">The colliding object</param>
    void OnTriggerExit2D(Collider2D other)
    {
        //If the player stop colliding
        if (other.tag == "Player" || other.tag == "Enemy")
        {
            //Stop the collision from ignoring the player
            Physics2D.IgnoreCollision(platformCollider, other , false);
        }
    }
	
}
