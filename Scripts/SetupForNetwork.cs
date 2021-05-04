using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class SetupForNetwork : NetworkBehaviour
{
    public List<Component> removeFirst = new List<Component>();
    public List<GameObject> defaultLayer = new List<GameObject>();
    void Start(){
        if(!IsOwner){
            for(int i = 0; i < removeFirst.Count; i++){
                Destroy(removeFirst[i]);
            }
            foreach(GameObject go in defaultLayer){
                go.layer = 0;
            }

        //     Component[] allComponents = GetComponentsInChildren<Component>();
        //     foreach(Component c in allComponents){
        //         if(!(c is Transform || c is Rigidbody || c is Collider || c is MeshFilter 
        //         || c is MeshRenderer || whiteList.Contains(c) || c.GetType().Name.Contains("Network"))){
        //             Destroy(c);
        //         }
        //     }
        //     Debug.Log("Stripped!");
        }
        else 
            Destroy(Camera.main.gameObject);

    }
}
