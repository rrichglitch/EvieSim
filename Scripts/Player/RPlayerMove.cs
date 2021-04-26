using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RPlayerMove : MonoBehaviour
{
    public float walkSpeed = 20;
    public float force = 40;
    public float jumpBoost = 10F;
    public float groundDist = .4F;
    public LayerMask groundMask;
    [Range(0,1)] public float efficiency = .1F;
    private Transform groundCehck;
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 playerDirect = Vector3.zero;
    private bool jumpHeld = false;
    private Transform vCam;
    private Vector2 wasdIn = Vector2.zero;

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody>();
        groundCehck = transform.Find("GroundCheck");
        vCam = transform.Find("VCam");
    }
    void OnMove(InputValue  ctx){
        //player direct is the conversion from relative wasd to world direction
        wasdIn = ctx.Get<Vector2>();
    }

    void OnJump(InputValue  ctx){
        jumpHeld = ctx.isPressed;
    }
    void OnLook(InputValue  ctx){
        vCam.SendMessage("OnLook", ctx);
    }

    //force based movement
    void FixedUpdate(){
        //convert the last input to an absolute player direction
        //do this every frame or input will not seem to update with character rotation
        playerDirect = transform.TransformDirection(new Vector3(wasdIn.x, 0, wasdIn.y));

        //groundcheck
        isGrounded = Physics.CheckSphere(groundCehck.position, groundDist, groundMask);

        //get the current horizontal velocity
        Vector3 horVEl = rb.velocity;
        horVEl.y = 0;

        //add force to move
        if(rb.velocity.magnitude < walkSpeed) rb.AddForce(playerDirect*force);
        //only allow force added against a top+ speed velocity
        else{
            float dotProd = Vector3.Dot(horVEl.normalized, playerDirect);
            float pDirectMod = (2-(dotProd+1))/2;
            rb.AddForce(playerDirect*force*pDirectMod);
            // Debug.Log("dotProd is "+dotProd);
            // if(dotProd > .5) Debug.Log(horVEl.normalized+" compared to "+playerDirect);
        }
        
        //make a new curved velocity horizontal velocity and set it
        horVEl = Vector3.Lerp(horVEl, playerDirect * horVEl.magnitude, efficiency);
        rb.velocity = new Vector3(horVEl.x, rb.velocity.y, horVEl.z);

        //jump
        if(isGrounded && jumpHeld){
            rb.AddRelativeForce(new Vector3(0,jumpBoost,0), ForceMode.Impulse);
        }

        //moveposition based movement
    }
}
