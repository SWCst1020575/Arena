using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.SceneManagement;

public class ManControl : MonoBehaviour
{
    public GameObject attackArea;

    private int maxHP;
    private string currentSceneName;

    public PlayerHPBar hpbar;

    public AudioSource audioPlayer;
    public AudioClip hurtSE;
    public AudioSource audioPlayer2;
    public AudioClip attackSE;

    public ParticleSystem hurtEffect;
    public ParticleSystem healEffect;

    public float forwardSpeed = 5.0f;
    public float backwardSpeed = 2.0f;
    public float rotateSpeed = 2f;
    public float jumpPower = 4.0f;
    public float useCurveHeight = 0.3f;

    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider col;

    private int idleState;
    private int locomotionState;
    private int jumpState;
    private int attackState;
    private int walkState;
    private int shootState;
    private int leftSideWalkState;
    private int rightSideWalkState;

    private bool lockMoving;

    private float origColliderHeight;
    private Vector3 origColliderCenter;

    public float mouseSensitive = 0.2f;
    public Transform manBody;
    public CharacterController manController;

    private GameState gameState;

    // Use this for initialization
    void Start()
    {
        // Get component
        animator = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
        col = gameObject.GetComponent<CapsuleCollider>();
        origColliderHeight = col.height;
        origColliderCenter = col.center;

        // Get State Hash
        idleState = Animator.StringToHash("Base Layer.WAIT00");
        locomotionState = Animator.StringToHash("Base Layer.Locomotion");
        jumpState = Animator.StringToHash("Base Layer.Jump00");
        attackState = Animator.StringToHash("Base Layer.Attack");
        walkState = Animator.StringToHash("Base Layer.Walk00_B");
        shootState = Animator.StringToHash("Base Layer.Shoot");
        leftSideWalkState = Animator.StringToHash("Base Layer.LeftSideWalk");
        rightSideWalkState = Animator.StringToHash("Base Layer.RightSideWalk");

        gameState = GameObject.Find("/GameState").GetComponent<GameState>();
        
        maxHP = 1000;
        gameState.Initialize(maxHP);
        hpbar.SetMaxHealth(maxHP);
        hpbar.SetHealth(gameState.playerHP);

        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        ManTransformControl();
        Anim();
        WastedCheck();
        hpbar.SetHealth(gameState.playerHP);

        // Demo
        if (Input.GetKeyDown(KeyCode.C))
        {
            hurtEffect.Play();

            audioPlayer.PlayOneShot(hurtSE);

            gameState.playerHP -= 2;

        }

        // Demo
        if (Input.GetKeyDown(KeyCode.X))
        {
            healEffect.Play();
            if (gameState.playerHP < 10)
                gameState.playerHP += 1;
        }
    }

    void Anim()
    {
        // Get Animator State
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        lockMoving = false;

        // Play animation
        if (state.fullPathHash == locomotionState)
        {
            // If the character is moving, enable Jump
            if (Input.GetKey(KeyCode.Space))
            {
                // If not in the transition state
                if (!animator.IsInTransition(0))
                {
                    // Enable Jump
                    animator.SetBool("Jump", true);
                    // Add jump force to the rigidbody
                    rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                }
            }
            else if (Input.GetKey(KeyCode.F))
            {
                animator.SetBool("Attack", true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!audioPlayer2.isPlaying)
                    audioPlayer2.PlayOneShot(attackSE);
                attackArea.SetActive(true);
                animator.SetBool("Shoot", true);
            }
        }
        else if (state.fullPathHash == jumpState)
        {
            // Disable Jump
            animator.SetBool("Jump", false);

            // Adjust collider's position
            this.AdjustCollider();
        }
        else if (state.fullPathHash == attackState)
        {
            animator.SetBool("Attack", false);
            lockMoving = true;
        }
        else if (state.fullPathHash == idleState)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                // If not in the transition state
                if (!animator.IsInTransition(0))
                {
                    // Enable Jump
                    animator.SetBool("Jump", true);
                    // Add jump force to the rigidbody
                    rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                }
            }
            else if (Input.GetKey(KeyCode.F))
            {
                animator.SetBool("Attack", true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!audioPlayer2.isPlaying)
                    audioPlayer2.PlayOneShot(attackSE);
                attackArea.SetActive(true);
                animator.SetBool("Shoot", true);
            }
        }
        else if (state.fullPathHash == walkState)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                // If not in the transition state
                if (!animator.IsInTransition(0))
                {
                    // Enable Jump
                    animator.SetBool("Jump", true);
                    // Add jump force to the rigidbody
                    rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                }
            }
            else if (Input.GetKey(KeyCode.F))
            {
                animator.SetBool("Attack", true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!audioPlayer2.isPlaying)
                    audioPlayer2.PlayOneShot(attackSE);
                attackArea.SetActive(true);
                animator.SetBool("Shoot", true);
            }
        }
        else if (state.fullPathHash == leftSideWalkState
            || state.fullPathHash == rightSideWalkState)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                // If not in the transition state
                if (!animator.IsInTransition(0))
                {
                    // Enable Jump
                    animator.SetBool("Jump", true);
                    // Add jump force to the rigidbody
                    rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                }
            }
            else if (Input.GetKey(KeyCode.F))
            {
                animator.SetBool("Attack", true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (!audioPlayer2.isPlaying)
                    audioPlayer2.PlayOneShot(attackSE);
                animator.SetBool("Shoot", true);
                attackArea.SetActive(true);
            }
        }
        else if (state.fullPathHash == shootState)
        {
            attackArea.SetActive(false);
            animator.SetBool("Shoot", false);
        }

        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        animator.SetFloat("Speed", v);

        // Use "Direction" to set as side walk
        animator.SetFloat("Direction", h);

    }

    public void ManTransformControl()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (z < 0f)
        {
            z /= 4f;
        }

        Vector3 move = transform.right * x
        + transform.forward * z + transform.up * -9.81f;

        manController.Move(forwardSpeed * Time.deltaTime * move);

        float mouseX = Input.GetAxis("Mouse X");
        mouseX *= mouseSensitive;
        manBody.Rotate(Vector3.up * mouseX);
    }

    public void AdjustCollider()
    {
        // Get Current Model Height
        Ray ray = new Ray(gameObject.transform.position, -Vector3.up);
        RaycastHit hitInfo = new RaycastHit();

        // If the heights differ too much, adjust the collider height
        if (Physics.Raycast(ray, out hitInfo))
        {
            if (hitInfo.distance > useCurveHeight)
            {
                float jumpHeight = animator.GetFloat("JumpHeight");
                // Adjust collider's height during the jump state
                // The height of the model will decrease due to the jump pose
                col.height = origColliderHeight - jumpHeight;
                // Adjust collider's center during the jump state
                // The center of the collider will rise with the character
                float adjCenterY = origColliderCenter.y + jumpHeight;
                col.center = new Vector3(0, adjCenterY, 0);
            }
            else
            {
                col.height = origColliderHeight;
                col.center = origColliderCenter;
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("cure"))
        {
            healEffect.Play();

            if (gameState.playerHP < maxHP)
                gameState.playerHP += 100;
        }
    }

    void WastedCheck()
    {
        if (gameState.playerHP <= 0)
        {   
            gameState.restart(maxHP);
            SceneManager.LoadScene(currentSceneName);
        }
    }

    public void AttackByMonster(int damage)
    {
        hurtEffect.Play();
        
        if (!audioPlayer.isPlaying)
            audioPlayer.PlayOneShot(hurtSE);

        gameState.playerHP -= damage;
    }

}
