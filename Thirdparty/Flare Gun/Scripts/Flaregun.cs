using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using MLAPI;
using MLAPI.Messaging;

public class Flaregun : NetworkBehaviour {
	
	public Rigidbody flareBullet;
	public Transform barrelEnd;
	public GameObject muzzleParticles;
	public AudioClip flareShotSound;
	public AudioClip noAmmoSound;	
	public AudioClip reloadSound;	
	public int bulletSpeed = 50;
	public int maxSpareRounds = 5;
	public int spareRounds = 3;
	public int currentRound = 0;
	public PlayerInput pIn;
	public Rigidbody holder;
	//start is called before the first update
	void Start(){
		// foreach(InputActionMap iam in iAA.actionMaps){
		// 	Debug.Log(iam.name);
		// }
		// if(iAA.devices == null) Debug.Log("there are no devices!");
		// else
		// foreach(InputDevice iam in iAA.devices){
		// 	Debug.Log(iam.name);
		// }
		if(pIn != null){
			InputAction fire = pIn.actions.FindAction("Fire");
			fire.performed += _ => Fire();

			InputAction reload = pIn.actions.FindAction("Reload");
			reload.performed += _ => Reload();
		}
	}
    // void OnDisable(){
    //     pIn.actions.FindAction("Grapple").performed -= _ => Fire();
    //     pIn.actions.FindAction("Reload").performed -= _ => Reload();
    // }
	void Fire(){
		if(!GetComponent<Animation>().isPlaying){
			if(currentRound > 0){
				ShootServerRpc();
				currentRound--;
				GetComponent<Animation>().CrossFade("Shoot");
				GetComponent<AudioSource>().PlayOneShot(flareShotSound);
			}else{
				GetComponent<Animation>().Play("noAmmo");
				GetComponent<AudioSource>().PlayOneShot(noAmmoSound);
			}
		}
	}
	
	// Update is called once per frame
	// void Update () 
	// {
		
	// 	if(Input.GetButtonDown("Fire1") && !GetComponent<Animation>().isPlaying)
	// 	{
	// 		if(currentRound > 0){
	// 			Shoot();
	// 		}else{
	// 			GetComponent<Animation>().Play("noAmmo");
	// 			GetComponent<AudioSource>().PlayOneShot(noAmmoSound);
	// 		}
	// 	}
	// 	if(Input.GetKeyDown(KeyCode.R) && !GetComponent<Animation>().isPlaying)
	// 	{
	// 		Reload();
			
	// 	}
	
	// }
	[ServerRpc]
	void ShootServerRpc(){
		// Debug.Log("shoot called!");
		Vector3 barrelEndPos = barrelEnd.position;
		Quaternion barrelEndRot = barrelEnd.rotation;
		
		Rigidbody bulletInstance;
		bulletInstance = Instantiate(flareBullet,barrelEndPos,barrelEndRot) as Rigidbody; //INSTANTIATING THE FLARE PROJECTILE
		Physics.IgnoreCollision(bulletInstance.GetComponent<Collider>(), holder.GetComponent<Collider>());
		
		bulletInstance.velocity = holder.velocity + ((barrelEndRot *Vector3.forward) * bulletSpeed); //ADDING FORWARD VELOCITY TO THE FLARE PROJECTILE

		if(!(IsServer || IsHost)) Debug.Log("why are you boooming!?");
		GameObject go  = bulletInstance.gameObject;
		if(go != null) go.GetComponent<NetworkObject>().Spawn();

		go = Instantiate(muzzleParticles, barrelEndPos,barrelEndRot);
		go.GetComponent<NetworkObject>().Spawn();
		Destroy(go,3);	//INSTANTIATING THE GUN'S MUZZLE SPARKS	

		ShootClientRpc();
	}

	[ClientRpc]
	void ShootClientRpc(){

		if(IsOwner) return;
		if(currentRound <= 0){
			currentRound = 0;
		}
		currentRound--;
		GetComponent<Animation>().CrossFade("Shoot");
		GetComponent<AudioSource>().PlayOneShot(flareShotSound);
			
	}
	
	void Reload(){
		if(!GetComponent<Animation>().isPlaying){
			if(spareRounds >= 1 && currentRound == 0){
				GetComponent<AudioSource>().PlayOneShot(reloadSound);			
				spareRounds--;
				currentRound++;
				GetComponent<Animation>().CrossFade("Reload");
			}
		}
	}
}
