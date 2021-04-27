using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OmniDash : MonoBehaviour
{
    public InputActionAsset iAA;
    public RPlayerMove moveScript;
    public Transform cam;
    public float speed = 30;
    public float duration = .5F;
    public float endMod = .5F;
    private bool dashing = false;
    private Vector3 dashDirection;
    private Rigidbody rb;
    private float cutOut;
    private float speedToAdd;
    public int maxCharges = 2;
    public int charges = 2;
    public float cooldown = 3;
    private float startCooldown = -1;
    // [SerializeField, Range(0,1)] private float maintainVelocity = .5F;
    // Start is called before the first frame update
    void Start(){
        iAA.FindAction("Dash").performed += _ => Dash();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate(){
        if(charges < maxCharges && Time.time - startCooldown > cooldown){
            charges++;
            if(charges >= maxCharges) startCooldown = -1;
            else startCooldown = Time.time;
        }
        if(dashing){
            rb.velocity = dashDirection*(cutOut+speedToAdd);
        }
    }
    void Dash(){
        if(!dashing && charges > 0){
            float wasdDegrees = Mathf.Atan2(moveScript.wasdIn.x, moveScript.wasdIn.y)*Mathf.Rad2Deg;
            //if moving forward then the dash follows the camera, otherwise it follows the playerDirect
            dashDirection = Mathf.Abs(wasdDegrees) < 45? cam.forward: moveScript.playerDirect;

            //get "cut out" of current velocity aligned with dash direction
            cutOut = Vector3.Dot(dashDirection, rb.velocity.normalized) * rb.velocity.magnitude;

            //calculate the speed to add to the cutout based on how fast the cutOut already is
            //as not to dash any faster than double the dash speed
            speedToAdd = cutOut <= speed? speed: Mathf.Lerp(speed,0,(cutOut-speed)/speed);

            charges--;
            startCooldown = Time.time;
            dashing = true;
            StartCoroutine(DashOff());
        }
    }
    IEnumerator DashOff(){
        yield return new WaitForSeconds(duration);
        dashing = false;
        rb.velocity = dashDirection*(cutOut+(speedToAdd*endMod));;
    }
}
