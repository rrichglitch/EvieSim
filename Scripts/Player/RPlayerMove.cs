using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RPlayerMove : MonoBehaviour
{
    [Min(0)] public float walkSpeed = 20;
    // [Min(0)] public float airSpeed = 20;
    private float maxSpeed;
    [Min(0)] public float force = 40;
    [Min(0)] public int stoppingPower = 2;
    [Min(0)] public float jump = 10F;
    [Min(0)] public float jumpScaled = 10F;
    [Min(0)] public float groundDist = .4F;
    [Min(0)] public float wallDist = .4F;
    public LayerMask groundMask;
    [Range(0,1)] public float velocityCurving = .1F;
    private Transform groundCehck;
    private Rigidbody rb;
    [Min(0)] public float groundFric = 2;
    private bool isGrounded = false;
    private bool wallRunning = false;
    public Vector3 playerDirect {get; private set;} = Vector3.zero;
    public Vector3 camDirect {get; private set;} = Vector3.zero;
    private bool jumpHeld = false;
    private Transform vCam;
    public Vector2 wasdIn {get; private set;} = Vector2.zero;
    private float lastJump = 0;
    private float jumpCooldown = .3F;
    [Range(0,1)] public float surfaceJumpweight = .17F;
    [Min(0)] public float wallCounterGrav = 10;
    [Range(0,1)] public float wallFallCurving = .1F;
    public float currentSpeed = 0;

    // Start is called before the first frame update
    void Start(){
        rb = GetComponent<Rigidbody>();
        groundCehck = transform.Find("GroundCheck");
        vCam = transform.Find("VCam");
        maxSpeed = walkSpeed;
    }
    void OnMove(InputValue  ctx){
        //player direct is the conversion from relative wasd to world direction
        wasdIn = ctx.Get<Vector2>();
    }

    void OnJump(InputValue  ctx){
        jumpHeld = ctx.isPressed;
    }
    // void OnLook(InputValue  ctx){
    //     vCam.SendMessage("OnLook", ctx);
    // }

    //force based movement
    void FixedUpdate(){
        //convert the last input to an absolute player direction
        //do this every frame or input will not seem to update with character rotation
        playerDirect = transform.TransformDirection(new Vector3(wasdIn.x, 0, wasdIn.y));
        camDirect = Quaternion.AngleAxis(vCam.eulerAngles.x, transform.right)*playerDirect;

        //get the current horizontal velocity for multiple uses
        Vector3 horVel = rb.velocity;
        horVel.y = 0;

        //separate checks for ground and walls
        // isGrounded = Physics.CheckSphere(groundCehck.position, groundDist, groundMask);
        Collider[] collidResults = new Collider[1];
        RaycastHit groundInf;

        //ground check
        isGrounded = Physics.SphereCast(groundCehck.position, .5F, -transform.up, out groundInf, groundDist);

        //check for wall if not on ground
        if(!isGrounded){
            wallRunning = (Physics.OverlapSphereNonAlloc(groundCehck.position, wallDist, collidResults, groundMask) > 0);
            //if on a wall then start wall running
            if(wallRunning){
                WallRun(collidResults[0], ref groundInf, horVel);
            }
        }
        else{
            wallRunning = false;

            //add ground friction
            rb.AddForce(-rb.velocity.normalized * groundFric, ForceMode.Acceleration);
        }
            
        //jump if on the ground or a wall
        if((isGrounded || wallRunning) && jumpHeld && Time.time - lastJump >= jumpCooldown){
            Vector3 calcJump = transform.up*jump;
            if(groundInf.normal != null){
                calcJump = Vector3.Lerp(transform.up, groundInf.normal, surfaceJumpweight)*jump;
                // Debug.Log("wall surface normal is "+groundInf.normal);
            }
            
            rb.AddForce(calcJump, ForceMode.Impulse);
            lastJump = Time.time;
        }

        //add force to keep from moving
        if(playerDirect.magnitude==0 && horVel.magnitude > 0.001){
            Vector3 smoothHorVel = horVel.normalized* Mathf.Sqrt(horVel.magnitude/walkSpeed)*walkSpeed;
            rb.AddForce(-Vector3.ClampMagnitude(smoothHorVel, walkSpeed) * stoppingPower, ForceMode.Acceleration);
            // Debug.Log("smoothedVel is "+smoothHorVel+" while velocity is "+rb.velocity);
        }
        //add force to move
        else if(horVel.magnitude < maxSpeed) rb.AddForce(playerDirect*force, ForceMode.Acceleration);
        //only allow force added against a top+ speed velocity
        else{
            float dotProd = Vector3.Dot(horVel.normalized, playerDirect);
            float pDirectMod = (2-(dotProd+1))/2;
            rb.AddForce(playerDirect*force*pDirectMod);
            // Debug.Log("dotProd is "+dotProd);
            // if(dotProd > .5) Debug.Log(horVEl.normalized+" compared to "+playerDirect);
        }
        
        //make a new curved velocity and set it
        float wasdDegrees = Mathf.Atan2(wasdIn.x, wasdIn.y)*Mathf.Rad2Deg;
        if(wallRunning){
            Vector3 ampedVertCamDirect = camDirect;
            ampedVertCamDirect.y = Mathf.Sqrt(camDirect.y);
            Vector3 curvedVel = Vector3.Lerp(rb.velocity, ampedVertCamDirect.normalized * rb.velocity.magnitude, velocityCurving*(horVel.magnitude/walkSpeed));
            rb.velocity = curvedVel;
        }
        //if not wallrunning then velocity curning should only be horizontal
        else{
            horVel = Vector3.Lerp(horVel, playerDirect * horVel.magnitude, velocityCurving);
            rb.velocity = new Vector3(horVel.x, rb.velocity.y, horVel.z);
        }

        currentSpeed = rb.velocity.magnitude;

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
        float scaledCounterGrav = wallCounterGrav * Mathf.Clamp((horVEl.magnitude/walkSpeed),0,1);
        rb.AddForce(new Vector3(0,scaledCounterGrav,0), ForceMode.Acceleration);

        //if falling with horizontal velocity then curve the vertical velocity towards parallel with the wall
        if(rb.velocity.y < 0 && horVEl.magnitude > .1F){
            //find which direction along the wall better fits the horVEl
            Vector3 along = Vector3.Cross(groundInf.normal, new Vector3(0,1,0));
            if(Vector3.Dot(along, horVEl) < 0) along = Vector3.Cross(groundInf.normal, new Vector3(0,-1,0));

            //create a curved falling velocity towards along the wall
            //curving will be scaled by horizontal speed
            Vector3 fallCurve = new Vector3(0,rb.velocity.y,0);
            fallCurve = Vector3.Lerp(fallCurve.normalized, along, wallFallCurving*(horVEl.magnitude/walkSpeed)) * fallCurve.magnitude;

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
