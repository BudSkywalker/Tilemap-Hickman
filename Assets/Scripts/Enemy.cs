using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
            FindObjectOfType<PlayerController>().EnemiesKilled++;
        }
        else if (collision.collider.CompareTag("Player"))
        {
            collision.rigidbody.AddForceAtPosition(-collision.relativeVelocity, collision.transform.position, ForceMode2D.Impulse);
            FindObjectOfType<PlayerController>().Lives--;
        }
    }

}
