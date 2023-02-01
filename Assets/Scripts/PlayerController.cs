using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    LayerMask groundMask;
    [SerializeField]
    float speed = 2f;
    [SerializeField]
    float jumpStrength = 3.75f;
    [SerializeField]
    Collider2D groundCollider;

    private int _pickups = 0;
    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool isGrounded;
    List<Collider2D> colliders = new();

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Walk", false);
        if (Input.GetKey(KeyCode.A)) MoveLeft();
        if (Input.GetKey(KeyCode.D)) MoveRight();
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) Jump();
        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("Jump") && isGrounded) anim.SetTrigger("Land");
        Camera.main.transform.position = transform.position + new Vector3(0, 0, -10);
        if (Camera.main.transform.position.y < 0) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, 0, -10);
        if (Camera.main.transform.position.y > 26) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, 26, -10);
        if (Camera.main.transform.position.x < -13) Camera.main.transform.position = new Vector3(-13, Camera.main.transform.position.y, -10);
        if (Camera.main.transform.position.x > 19) Camera.main.transform.position = new Vector3(19, Camera.main.transform.position.y, -10);

        if (transform.position.y > 29.5f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            transform.position = new Vector3(transform.position.x, 29.5f, transform.position.z);
        }
        if (transform.position.y < -3.5f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            transform.position = new Vector3(transform.position.x, -3.5f, transform.position.z);
        }
        if (transform.position.x > 26.5f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            transform.position = new Vector3(26.5f, transform.position.y, transform.position.z);
        }
        if (transform.position.x < -20.5f)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            transform.position = new Vector3(-20.5f, transform.position.y, transform.position.z);
        }

        foreach(Touch t in Input.touches)
        {
            if (t.phase == TouchPhase.Began) Debug.Log("Hi");
        }
    }

    void FixedUpdate()
    {
        rb.OverlapCollider(new ContactFilter2D().NoFilter(), colliders);
        if (colliders.Contains(groundCollider)) isGrounded = true;
        else isGrounded = false;

        rb.GetAttachedColliders(colliders);
        if(rb.velocity.y > 0.1f || Input.GetKey(KeyCode.S))
        {
            foreach (Collider2D c in colliders)
            {
                c.isTrigger = true;
            }
        }
        else
        {
            foreach (Collider2D c in colliders)
            {
                c.isTrigger = false;
            }
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector2.up * jumpStrength, ForceMode2D.Impulse);
            rb.GetAttachedColliders(colliders);
            foreach (Collider2D c in colliders)
            {
                c.isTrigger = true;
            }
            anim.SetTrigger("Jump");
        }
    }

    void MoveLeft()
    {
        spriteRenderer.flipX = true;
        Move(-Time.deltaTime);
    }

    void MoveRight()
    {
        spriteRenderer.flipX = false;
        Move(Time.deltaTime);
    }

    void Move(float velo)
    {
        anim.SetBool("Walk", true);
        transform.position += speed * velo * Vector3.right;
    }
}
