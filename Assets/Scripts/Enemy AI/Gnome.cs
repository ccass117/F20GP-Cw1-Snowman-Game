using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gnome : FreezableEntity
{
    //Declare variables for jumping
    public float jumpForce = 3;
    int bigJump = 0;
    public float bigJumpMult = 3f;
    public float sideForce = 4;
    bool jumpAvailable = false;
    bool direction = false;
    bool onIce = false;

    Rigidbody rigidBody;

    private void Awake()
    {
        //initialise and freeze rotation of rigidbody
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        //small hop
        if (jumpAvailable == true && bigJump != 2)
        {
            rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //add additional sidejump force if on ice to simulate sliding
            if (onIce == true)
                {
                if (direction == false)
                    rigidBody.AddRelativeForce(Vector3.right * (sideForce/2), ForceMode.Impulse);
                else
                    rigidBody.AddRelativeForce(Vector3.left * (sideForce/2), ForceMode.Impulse);
            }
            bigJump++;
            jumpAvailable = false;
        }
        //big jump (every 3 jumps)
        if (jumpAvailable == true && bigJump == 2)
        {
            rigidBody.AddForce(Vector3.up * (jumpForce * bigJumpMult), ForceMode.Impulse);
            //force left then right (relative to gnome's rotation)
            if (direction == false)
            {
                rigidBody.AddRelativeForce(Vector3.left * sideForce, ForceMode.Impulse);
                direction = true;
            }
            else
            {
                rigidBody.AddRelativeForce(Vector3.right * sideForce, ForceMode.Impulse);
                direction = false;
            }
            //reset big jump counter
            bigJump=0;
            jumpAvailable = false;
        }
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;

        //re-enable jump
        if (collision.collider.gameObject.layer == (int)Layers.SolidGround)
            jumpAvailable = true;
        //kill if on deathfloor
        if (collision.gameObject.name == "LargeDeathFloor")
        {
            Destroy(this.gameObject);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Ice")
            onIce = true;
        else
            onIce = false;
    }
}
