using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodePhysic : MonoBehaviour
{
    public float blastRad = 5;
    public float force = 5;
    public void Boom(){
        Collider[] collids = Physics.OverlapSphere(transform.position, blastRad);
        int count = 0;
        foreach(Collider nearby in collids){
            Rigidbody rb = nearby.GetComponent<Rigidbody>();
            if(rb != null){
                rb.AddExplosionForce(force, transform.position, blastRad);
                count++;
            }
        }
        Debug.Log("went boom on "+ count);
    }
}
