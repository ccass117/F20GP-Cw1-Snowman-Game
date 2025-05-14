using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnowBuddy : MonoBehaviour
{
    // movement stuff
    float acceleration = 55; // force multiplier to acceleration force
    float deceleration = 15; // force multiplier to deceleration force
    int currentMaxVelocity = 15; // when speed exceeds this value, set movespeed to this value isntead.
    int defaultMaxVelocity = 15;
    int grappleMaxVelocity = 25;
    int jumpForce = 10; // force of the jump
    public Vector3 Move {  get; private set; }
    bool jumpAvailable = true;
    public bool FlyCheat { get; private set; } = false;


    // self references
    Rigidbody rigidBody;
    SphereCollider sphereCollider;

    [SerializeField] Transform rotator;
    [SerializeField] IcePick icePickRef;
    [SerializeField] SnowBallProjectile snowBallRef;

    GameMgr mgr;

    // snow mass gaining
    Status status = Status.InAir;
    public bool Grounded { get { return status == Status.OnGround; } }
    float velocityGrowthScalar = 0.001f;  // scalar used for scaling growth according to velocity
    float constantGrowthScalar = 0.005f;  // scalar used for scaling growth according to a constant measurement
    float maxSnowSize = 3.5f;
    float minSnowSize = 0.75f;

    // friction type for when on ice
    public PhysicMaterial[] frictionType;
    bool onIce = false;
    bool onSnow = false;

    //hard reset timers
    float startTime = 0f;
    float holdTime = 1.5f;

    //Penguin catch and counters
    int penguinCounter = 0;
    int HighestPenguinCount;
    public GameObject MaxPenguinSign;

    TextMeshProUGUI _penguinCounterText;
    TextMeshProUGUI penguinCounterText
    {
        get
        {
            if (_penguinCounterText == null)
                _penguinCounterText = GameObject.Find("Penguins Text").GetComponent<TextMeshProUGUI>();
            
            return _penguinCounterText;
        }
    }



    private void Start()
    {
        //References
        MaxPenguinSign = GameObject.Find("Penguin Signpost");
        rigidBody = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        rigidBody.AddForce(new Vector3(10, 0, 0));
        mgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();

        //Get most penguins caught from PlayerPrefs, if none, assume 0.
        HighestPenguinCount = PlayerPrefs.GetInt("MaxPenguin", 0);
        
        //If the player is yet to get at 5+ penguins, do not show sign (will reappear on scene reload)
        if (HighestPenguinCount <= 5)
        {
            Destroy(MaxPenguinSign);
        }
        else
        {
            TMPro.TextMeshPro textMesh = MaxPenguinSign.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                //show most penguins caught, or congratulations message if all penguins have been caught
                textMesh.text = $"Most Penguins Caught: {HighestPenguinCount}, Can you catch all 74?";
                if (HighestPenguinCount >= 74)
                {
                    textMesh.text = $"Woah, you really caught all {HighestPenguinCount} penguins? Congrats!";
                }
            }
        }
    }

    void FixedUpdate()
    {
        // get directional inputs
        float xMov = Input.GetAxisRaw("Horizontal");
        float zMov = Input.GetAxisRaw("Vertical");

        // raise maxVelocity cap when grappling and moving on the ground
        currentMaxVelocity = (icePickRef.Hooked && Grounded) ? grappleMaxVelocity : defaultMaxVelocity;

        // get inputted movement relative to the rotator/camera rotation
        Move = (Utils.RemoveY(rotator.forward) * zMov + rotator.right * xMov).normalized * acceleration;
        if (icePickRef.Hooked && status != Status.OnGround) Move *= icePickRef.movementWhileHooked;

        // fancy maths for movement
        CalculateMovement();

        //note to marker:
        // if you are in doubt that the "fancy maths" is necessary, i've created a simply/naive version below.
        // if you comment out line 115, and un-comment line 122
        // you'll see what I mean if you play with this simplified version for a few minutes (especially when SnowBuddy is at full mass).
        // thank you :)
        //TESTINGCalculateMovement();

        // if falling, fall with increased (2x) gravity
        if (rigidBody.velocity.y <= 0)
            rigidBody.AddForce(Physics.gravity, ForceMode.Acceleration);

        if (status == Status.OnGround && onSnow == false) GainSize(-0.25f);
        else if (onSnow == true) GainSize(1);
    }

    private void Update()
    {
        //Evil Hacker code!! (This will get you ban from speedrun.com)
        if (Input.GetKeyDown(KeyCode.F1))
            FlyCheat = !FlyCheat;

        if (Input.GetKeyDown(KeyCode.Space) && (jumpAvailable || FlyCheat))
        {
            // if falling, reset vertical velocity
            if (rigidBody.velocity.y < 0) rigidBody.velocity -= new Vector3(0, rigidBody.velocity.y, 0);

            rigidBody.AddForce(Vector3.up * jumpForce * rigidBody.mass, ForceMode.Impulse);
            jumpAvailable = false;
            status = Status.InAir;
        }

        //Return to last checkpoint
        if (Input.GetKeyDown(KeyCode.R))
        {
            mgr.ReturnToSpawn();
            //Timer for scene reset
            startTime = Time.time;
        }

        //Hard reset if R held
        if (Input.GetKey(KeyCode.R))
        {
            //Defaults to holding for 1.5 seconds, 
            if (startTime + holdTime <= Time.time)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }


        if (Input.GetKeyDown(KeyCode.Mouse0)) icePickRef.Shoot();
        if (Input.GetKeyUp(KeyCode.Mouse0)) icePickRef.ResetProjectile();

        // shoot excess snow in the form of a snowball, which launches SnowBuddy in opposite direction
        if (Input.GetKeyDown(KeyCode.Mouse1) && transform.localScale.x > minSnowSize)
        {
            float excessSnow = transform.localScale.x - minSnowSize;
            transform.localScale = Vector3.one * minSnowSize;
            rigidBody.mass = minSnowSize;
            snowBallRef.Shoot(excessSnow);
        }
    }

    void GainSize(float dynamicScalar = 1, bool velocityScaling = true)
    {
        // check upper and lower limits
        if (dynamicScalar > 0 && transform.localScale.x >= maxSnowSize)
            return;
        if (dynamicScalar < 0 && transform.localScale.x <= minSnowSize)
            return;

        // scale with velocity or with constant
        float growth = (velocityScaling)
            ? dynamicScalar * (rigidBody.velocity.magnitude * velocityGrowthScalar)
            : dynamicScalar * constantGrowthScalar;

        transform.position += Vector3.one * growth * 0.51f;  // raise/lower position to account for growth
        transform.localScale += Vector3.one * growth;
        rigidBody.mass += growth / 2;
    }

    // allows SnowBuddy to maintain accurate movement control while recieving velocity changes from outside sources (e.g. Snowball launching self with impulse force)
    void CalculateMovement()
    {
        // redistribute velocity
        Vector3 nonVerticalVelocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);  // get velocity without y component

        // if the user is trying to move and current velocity > maximum velocity:
        // prevent them from speeding up, but allow them to direct and counteract their currently high velocity
        if (Move != Vector3.zero && nonVerticalVelocity.magnitude > currentMaxVelocity)
        {
            float A = Vector3.SignedAngle(nonVerticalVelocity * -1, Move, new Vector3(1, 0, 1));
            float aRadian = Mathf.Abs(A / 180);

            // reduce velocity in current direction
            rigidBody.AddForce(nonVerticalVelocity.normalized * (-1 * Move.magnitude * aRadian));
        }

        // apply new movement input
        rigidBody.AddForce(Move);


        // decelleration (if not inputting movement)
        if (Move.magnitude < 0.1f)
        {
            float currentDeceleration = onIce ? deceleration * 0.1f : deceleration; // Reduce deceleration on ice
            if (icePickRef.Hooked) currentDeceleration *= 0.3f;
            rigidBody.AddForce(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).normalized * currentDeceleration);
        }
    }

    // this is an alternate version of CalculateMovement, which proves that CalculateMovement is necessary
    void TESTINGCalculateMovement()
    {
        Vector3 nonVerticalVelocity = new Vector3(rigidBody.velocity.x, 0, rigidBody.velocity.z);  // get velocity without y component

        // apply new movement input
        if (nonVerticalVelocity.magnitude < currentMaxVelocity)
            rigidBody.AddForce(Move);


        // decelleration (if not inputting movement)
        if (Move.magnitude < 0.1f)
        {
            float currentDeceleration = onIce ? deceleration * 0.1f : deceleration; // Reduce deceleration on ice
            if (icePickRef.Hooked) currentDeceleration *= 0.3f;
            rigidBody.AddForce(new Vector3(-rigidBody.velocity.x, 0, -rigidBody.velocity.z).normalized * currentDeceleration);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;

        if (collision.collider.gameObject.layer == (int)Layers.SolidGround && collision.collider.gameObject.transform.position.y < transform.position.y)
        {
            jumpAvailable = true;
            status = Status.OnGround;
            icePickRef.HookAvailable = true;
        }

        if (collision.collider.gameObject.tag == "Snow")
        {
            onSnow = true;
        }
            
        if (collision.collider.gameObject.tag == "Ice")
        {
            onIce = true;
            sphereCollider.material = frictionType[1];
            acceleration = 15f;
        }
        else
        {
            onIce = false;
            sphereCollider.material = frictionType[0];
            acceleration = 55f;
        }
    }

    public void IncrementPenguinCounter()
    {
        penguinCounter++;
        penguinCounterText.text = $"Penguins: {penguinCounter}";
        if (penguinCounter % 5 == 0)
            mgr.SpawnGoldenPenguin();
        if (penguinCounter > PlayerPrefs.GetInt("MaxPenguin", 0))
        {
            PlayerPrefs.SetInt("MaxPenguin", penguinCounter);
            Debug.Log($"New Max Penguins: {PlayerPrefs.GetInt("MaxPenguin",0)}");
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.layer == (int)Layers.SolidGround)
            status = Status.InAir;
        if (collision.collider.gameObject.tag == "Snow")
            onSnow = false;
    }


    private void OnTriggerStay(Collider collision)
    {
        //Shrink the player as long as they are in heat
        if (collision.tag == "Heat")
        {
            GainSize(-5f, false);
        }
    }
}
