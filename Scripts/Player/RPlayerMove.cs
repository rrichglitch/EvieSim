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
    [Range(0,1)] public float horizontalCurving = .1F;
    private Transform groundCehck;
    private Rigidbody rb;
    public bool isGrounded;
    public Vector3 playerDirect {get; private set;} = Vector3.zero;
    private bool jumpHeld = false;
    private Transform vCam;
    public Vector2 wasdIn {get; private set;} = Vector2.zero;
    private float lastJump = 0;
    private float jumpCooldown = .3F;
    [Min(0)] public float surfaceJumpweight = .17F;
    [Min(0)] public float wallCounterGrav = 10;
    [Range(0,1)] public float wallFallCurving = .1F;

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

        //get the current horizontal velocity for multiple uses
        Vector3 horVEl = rb.velocity;
        horVEl.y = 0;

        //separate checks for ground and walls
        // isGrounded = Physics.CheckSphere(groundCehck.position, groundDist, groundMask);
        Collider[] collidResults = new Collider[1];
        RaycastHit groundInf;

        //ground check
        isGrounded = Physics.SphereCast(groundCehck.position, .5F, -transform.up, out groundInf, groundDist);

        //check for wall if not on ground
        if(!isGrounded){
            isGrounded = (Physics.OverlapSphereNonAlloc(groundCehck.position, wallDist, collidResults, groundMask) > 0);
            //if on a wall then start wall running
            if(isGrounded){
                WallRun(collidResults[0], ref groundInf, horVEl);
            }
        }
            
        //jump
        if(isGrounded && jumpHeld && Time.time - lastJump >= jumpCooldown){
            Vector3 calcJump = transform.up*jumpBoost;
            if(groundInf.normal != null){
                calcJump = Vector3.Lerp(transform.up, groundInf.normal, surfaceJumpweight)*jumpBoost;
                // Debug.Log("wall surface normal is "+groundInf.normal);
            }
            
            rb.AddForce(calcJump, ForceMode.Impulse);
            lastJump = Time.time;
        }

        //add force to move
        if(rb.velocity.magnitude < walkSpeed) rb.AddForce(playerDirect*force, ForceMode.Acceleration);
        //only allow force added against a top+ speed velocity
        else{
            float dotProd = Vector3.Dot(horVEl.normalized, playerDirect);
            float pDirectMod = (2-(dotProd+1))/2;
            rb.AddForce(playerDirect*force*pDirectMod);
            // Debug.Log("dotProd is "+dotProd);
            // if(dotProd > .5) Debug.Log(horVEl.normalized+" compared to "+playerDirect);
        }
        
        //make a new curved velocity horizontal velocity and set it
        horVEl = Vector3.Lerp(horVEl, playerDirect * horVEl.magnitude, horizontalCurving);
        rb.velocity = new Vector3(horVEl.x, rb.velocity.y, horVEl.z);

        //moveposition based movement
    }

    void WallRun(Collider collid, ref RaycastHit groundInf, Vector3 horVEl){
        Vector3 toSurf;
        //if the collider is not convex then approximate the surface info
        if(collid is MeshCollider && !((MeshCollider)collid).convex){
            toSurf = collid.ClosestPointOnBounds(groundCehck.position) - groundCehck.position;
            //if the groundcheck is inside of the concave collider then just shoot some rays and take the shortest
            if(toSurf.magnitude == 0){
                //shoot raycasts in 8 directions in a cricle around ground check and save the results
                RaycastHit[] hits = new RaycastHit[8];
                for(int i = 0; i<hits.Length; i++){
                    Quaternion directMod = Quaternion.AngleAxis((360/hits.Length)*i, Vector3.up);
                    Vector3 direction = directMod * transform.forward;
                    Physics.Raycast(groundCehck.position, direction, out hits[i], wallDist, groundMask);
                    Debug.DrawRay(groundCehck.position, direction);
                }
                //go through the results and pick the best fit for the wall in question
                RaycastHit curBest = new RaycastHit();
                for(int i = 0; i<hits.Length; i++){
                    if(hits[i].collider != null && hits[i].collider == collid){
                        if(curBest.collider == null)
                            curBest = hits[i];
                        else if(hits[i].distance < curBest.distance){
                            curBest = hits[i];
                        }
                    }
                }
                groundInf = curBest;
                // Debug.Log("this wall is concave!");
            }
            else{
                //use the closest point to cast a ray and grab info including the surface normal
                Physics.Raycast(groundCehck.position, toSurf.normalized, out groundInf, wallDist, groundMask);
            }
        }
        //otherwise get the surface info with precision
        else{
            toSurf = collid.ClosestPoint(groundCehck.position) - groundCehck.position;
            //use the closest point to cast a ray and grab info including the surface normal
            Physics.Raycast(groundCehck.position, toSurf.normalized, out groundInf, wallDist, groundMask);
        }

        //add counter gravity scaled by current horizontal velocity
        float scaledCounterGrav = wallCounterGrav * (horVEl.magnitude/walkSpeed);
        rb.AddForce(new Vector3(0,scaledCounterGrav,0), ForceMode.Acceleration);

        //if falling with some horizontal velocity then curve the vertical velocity towards parallel with the wall
        if(rb.velocity.y < 0 && horVEl.magnitude > .1F){
            //find which direction along the wall better fits the horVEl
            Vector3 along = Vector3.Cross(groundInf.normal, new Vector3(0,1,0));
            if(Vector3.Dot(along, horVEl) < 0) along = Vector3.Cross(groundInf.normal, new Vector3(0,-1,0));

            //create a curved falling velocity towards along the wall
            //curving will be scaled by horizontal speed
            Vector3 fallCurve = new Vector3(0,rb.velocity.y,0);
            fallCurve = Vector3.Lerp(fallCurve.normalized, along, wallFallCurving) * fallCurve.magnitude;

            //add the curved falling velocity in place of the uncurved
            rb.velocity = horVEl + fallCurve;
        }
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
