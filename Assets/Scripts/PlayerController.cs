using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityStandardAssets.CrossPlatformInput;
using Cinemachine;

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
    [SerializeField]
    GameObject projectile, winDisplay;
    [SerializeField]
    float projectileSpeed = 1000f;
    [SerializeField]
    TMP_Text livesDisplay, enemiesDisplay;
    [SerializeField]
    CinemachineVirtualCamera cvc;

    public short Lives
    {
        get
        {
            return _lives;
        }
        set
        {
            _lives = value;
            livesDisplay.text = "Lives: " + _lives;
            if (_lives <= 0) GameOver(false);
        }
    }

    public short EnemiesKilled
    {
        get
        {
            return _kills;
        }
        set
        {
            _kills = value;
            enemiesDisplay.text = "Killed: " + _kills + "/" + _enemiesTotal;
            if (_kills >= _enemiesTotal) GameOver(true);
        }
    }

    private short _lives = 3;
    private short _kills = 0;
    private int _enemiesTotal;
    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    bool isGrounded;
    bool hasJumpBeenTriggered;
    List<Collider2D> colliders = new();
    MobileControlRig rig;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rig = FindObjectOfType<MobileControlRig>();
        _enemiesTotal = GameObject.FindGameObjectsWithTag("Enemy").Length;
        EnemiesKilled += 0;
        Lives += 0;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("Walk", false);
        //Note: I know that I could have it change the speed based on the axis. I don't feel like doing that, I prefer the feel of it this way
        if (CrossPlatformInputManager.GetAxis("Horizontal") < -0.1f) MoveLeft();
        if (CrossPlatformInputManager.GetAxis("Horizontal") > 0.1f) MoveRight();
        if (CrossPlatformInputManager.GetAxis("Vertical") > 0.75f)
        {
            if(!hasJumpBeenTriggered) Jump();
            hasJumpBeenTriggered = true;
        }
        else
        {
            hasJumpBeenTriggered = false;
        }

        if (CrossPlatformInputManager.GetButtonDown("Shoot")) Shoot();

        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("Jump") && isGrounded) anim.SetTrigger("Land");
        
        //Camera fixed to level
        Camera.main.transform.position = transform.position + new Vector3(0, 0, -10);
        if (Camera.main.transform.position.y < 0) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, 0, -10);
        if (Camera.main.transform.position.y > 26) Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, 26, -10);
        if (Camera.main.transform.position.x < -13) Camera.main.transform.position = new Vector3(-13, Camera.main.transform.position.y, -10);
        if (Camera.main.transform.position.x > 19) Camera.main.transform.position = new Vector3(19, Camera.main.transform.position.y, -10);

        //Character fixed to level
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
    }

    void FixedUpdate()
    {
        rb.OverlapCollider(new ContactFilter2D().NoFilter(), colliders);
        if (colliders.Contains(groundCollider)) isGrounded = true;
        else isGrounded = false;

        rb.GetAttachedColliders(colliders);
        if(rb.velocity.y > 0.1f || CrossPlatformInputManager.GetAxis("Vertical") < -0.75f)
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Room Bounds"))
        {
            cvc = other.GetComponentInChildren<CinemachineVirtualCamera>();
            cvc.Priority = 10;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Room Bounds"))
        {

            cvc = other.GetComponentInChildren<CinemachineVirtualCamera>();
            cvc.Priority = 1;
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

    void Shoot()
    {
        float power = projectileSpeed;
        if (spriteRenderer.flipX) power *= -1;
        GameObject go = Instantiate(projectile, transform.position, transform.rotation);
        go.GetComponent<Rigidbody2D>().AddForce(new Vector2(power, 0));
        StartCoroutine(KillBullet(go));
        StartCoroutine(ShakeCam(cvc));
    }

    IEnumerator KillBullet(GameObject bullet)
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(bullet);
    }

    //I know I'm getting lazy on the names I don't feel great just trying to get this assignment done
    IEnumerator ShakeCam(CinemachineVirtualCamera c)
    {
        CinemachineBasicMultiChannelPerlin shake = c.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        shake.m_AmplitudeGain = 1;
        while(shake.m_AmplitudeGain > 0)
        {
            yield return new WaitForEndOfFrame();
            shake.m_AmplitudeGain -= 0.05f;
        }
    }


    private void GameOver(bool didWin)
    {
        winDisplay.SetActive(true);

        if(didWin) winDisplay.GetComponentInChildren<TMP_Text>().text = "You win!";
        else winDisplay.GetComponentInChildren<TMP_Text>().text = "You lost!";

        Destroy(FindObjectOfType<MobileControlRig>().gameObject);
    }
}
