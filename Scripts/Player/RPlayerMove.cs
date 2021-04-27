using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RPlayerMove : MonoBehaviour
{
    [Min(0)] public float walkSpeed = 20;
    [Min(0)] public float force = 40;
    [Min(0)] public float jumpBoost = 10F;
    [Min(0)] public float groundDist = .4F;
    [Min(0)] public float wallDist = .4F;
    public LayerMask groundMask;
    [Range(0,1)] public float efficiency = .1F;
    private Transform groundCehck;
    private Rigidbody rb;
    public bool isGrounded;
    private Vector3 playerDirect = Vector3.zero;
    private bool jumpHeld = false;
    private Transform vCam;
    private Vector2 wasdIn = Vector2.zero;
    private float lastJump = 0;
    private float jumpCooldown = .3F;
    [Min(0)] public float surfaceJumpweight = .17F;
    [Min(0)] public float wallCounterGrav = 10;

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

        //separate checks for ground and walls
        // isGrounded = Physics.CheckSphere(groundCehck.position, groundDist, groundMask);
        Collider[] collidResults = new Collider[1];
        RaycastHit groundInf;

        //ground check
        isGrounded = Physics.SphereCast(groundCehck.position, .5F, -transform.up, out groundInf, groundDist);

        //check for wall if not on ground
        if(!isGrounded){
            isGrounded = (Physics.OverlapSphereNonAlloc(groundCehck.position, wallDist, collidResults, groundMask) > 0);
            //if on a wall then grab the surface normal and add some force against gravity
            if(isGrounded){
                Vector3 toSurf = collidResults[0].ClosestPoint(groundCehck.position) - groundCehck.position;
                Physics.Raycast(groundCehck.position, toSurf.normalized, out groundInf, wallDist, groundMask);
                rb.AddForce(new Vector3(0,wallCounterGrav,0), ForceMode.Acceleration);
            }
        }
            
        //jump
        if(isGrounded && jumpHeld && Time.time - lastJump >= jumpCooldown){
            Vector3 calcJump = transform.up*jumpBoost;
            if(groundInf.normal != null){
                calcJump = Vector3.Lerp(transform.up, groundInf.normal, surfaceJumpweight)*jumpBoost;
                // Debug.Log(groundInf.normal);
            }
            
            rb.AddForce(calcJump, ForceMode.Impulse);
            lastJump = Time.time;
        }

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

        //moveposition based movement
    }

    void OnDrawGizmos(){
        if(groundCehck != null){
            Gizmos.DrawWireSphere(groundCehck.position, wallDist);
            Vector3 endOfGroundCheck = groundCehck.position;
            endOfGroundCheck.y -= groundDist;
            Gizmos.DrawLine(groundCehck.position, endOfGroundCheck);
        }
    }
}
