using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour {

    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, cam, player;
    private float maxDistance = 100f;
    private SpringJoint joint;
    private Quaternion desiredRotation;
    private float rotationSpeed = 5f;
    public  InputActionAsset iAA;
    private Quaternion startRot;
    private Transform connectedT;

    void Start(){
        lr = gunTip.GetComponent<LineRenderer>();
        iAA.FindAction("Grapple").performed += _ => StartGrapple();
        iAA.FindAction("Grapple").canceled += _ => StopGrapple();

        startRot = transform.localRotation;
    }

    void Update(){
        // if (Input.GetMouseButtonDown(0)) {
        //     StartGrapple();
        // }
        // else if (Input.GetMouseButtonUp(0)) {
        //     StopGrapple();
        // }

        if (!IsGrappling()) {
            desiredRotation = transform.parent.rotation *startRot;
        }
        else {
            desiredRotation = Quaternion.LookRotation(GetGrapplePoint() - transform.position)*startRot;
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }

    //Called after Update
    void CameraUpdate() {
        DrawRope();
    }

    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, whatIsGrappleable)) {
            connectedT = hit.transform;
            grapplePoint = connectedT.InverseTransformPoint(hit.point);
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = hit.rigidbody;
            joint.enableCollision = true;
            joint.connectedAnchor = hit.rigidbody == null? hit.point :grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, hit.point);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.9f;
            joint.minDistance = distanceFromPoint * 0.1f;

            //Adjust these values to fit your game.
            joint.spring = 4f;
            joint.damper = 6f;
            joint.massScale = 2f;

            lr.enabled = true;
            lr.positionCount = 2;
        }
    }


    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    void StopGrapple() {
        lr.positionCount = 0;
        lr.enabled = false;
        Destroy(joint);
    }

    
    void DrawRope() {
        //If not grappling, don't draw rope
        if (lr.positionCount < 2 || joint == null) return;
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, connectedT.TransformPoint(grapplePoint));
    }

    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return connectedT.TransformPoint(grapplePoint);
    }
}
