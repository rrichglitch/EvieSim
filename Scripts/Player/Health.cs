using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Health : MonoBehaviour
{
    public float health = 100;
    public PlayerInput input;
    void TakeDamage(float dmg){
        health -= dmg;
        if(health <= 0){
            input.enabled = false;
            StartCoroutine(Respawn());
        }
    }
    IEnumerator Respawn(){
        yield return new WaitForSeconds(3);
        transform.parent.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        health = 100;
        input.enabled = true;
    }

    void OnGUI(){
        GUILayout.BeginArea(new Rect(Screen.width/2-35,Screen.height-40,70,20));

        GUILayout.Label("Health: "+health);

        GUILayout.EndArea();
    }
}
