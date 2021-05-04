using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//put this script on the PlayerContainer holding the camera and body
public class Blink : MonoBehaviour
{
    public PlayerInput pIn;
    private Vector3 portTo;
    private bool held = false;
    public float maxDist = 10;
    private new Transform camera;
    private Transform player;
    public LayerMask hitsPortBeam;
    public float bumpAway = .01F;
    public float checkDiam = .5F;
    private Rigidbody body;
    // Start is called before the first frame update
    void Start(){
        camera = transform.Find("Main Camera");
        player = transform.Find("PlayerBody");
        body = player.GetComponent<Rigidbody>();

        if(pIn != null){
            pIn.actions.FindAction("Blink").performed += _ => Target();
            pIn.actions.FindAction("Blink").canceled += _ => Port();
        }
    }

    // Update is called once per frame
    void Update(){
        portTo = transform.position;
        if(held){
            portTo = Beam();
        }
    }

    void Target(){
        held = true;
        portTo = Beam();
    }

    void Port(){
        held = false;
        body.velocity = Vector3.zero;
        transform.position = portTo;
    }

    //returns furthest spot along the beam possible
    Vector3 Beam(){
        RaycastHit hitInf = new RaycastHit();
        if(!Physics.SphereCast(camera.position, checkDiam, camera.forward, out hitInf, maxDist, hitsPortBeam))
            return transform.position + (camera.forward*maxDist);
        else{
            Vector3 toRet = transform.position + (camera.forward*hitInf.distance);
            //make sure the bump is always towards where the player currently is and never away
            //otherwise the bump can sometimes push the player through a thin wall
            if(Vector3.Dot(hitInf.normal, camera.forward) <= 0)
                return toRet+(hitInf.normal*bumpAway);
            else return toRet-(hitInf.normal*bumpAway);
        }
    }
}
