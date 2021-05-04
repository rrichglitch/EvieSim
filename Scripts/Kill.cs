using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kill : MonoBehaviour
{
    void OnTriggerEnter(Collider collid){
        collid.gameObject.SendMessage("TakeDamage",999);
    }
}
