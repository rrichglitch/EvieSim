using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SmoothCamMoveNRot : MonoBehaviour
{
    public Transform target;
    public float posLerp = .1F;
    private Rigidbody targetBody;
    void Start(){
        targetBody = target.parent.GetComponent<Rigidbody>();
    }
    void LateUpdate(){
        Vector3 butterPos = transform.position+targetBody.velocity*Time.deltaTime;

        transform.position = Vector3.Lerp(butterPos, target.position, posLerp);
        transform.rotation = target.rotation;

        //emulate a lateUpdate for objects trying to follow the camera
        BroadcastMessage("CameraUpdate");
    }
}
