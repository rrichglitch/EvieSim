using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Flaregun : MonoBehaviour {
	
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
	public InputActionAsset iAA;
	public Rigidbody holder;
	//start is called before the first update
	void Start(){
		InputAction fire = iAA.FindAction("Fire");
		fire.performed += _ => Fire();

		InputAction reload = iAA.FindAction("Reload");
		reload.performed += _ => Reload();
	}
	void Fire(){
		if(!GetComponent<Animation>().isPlaying){
			if(currentRound > 0){
				Shoot();
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
	
	void Shoot()
	{
		currentRound--;
		if(currentRound <= 0){
			currentRound = 0;
		}
		
			GetComponent<Animation>().CrossFade("Shoot");
			GetComponent<AudioSource>().PlayOneShot(flareShotSound);
		
			
			Rigidbody bulletInstance;			
			bulletInstance = Instantiate(flareBullet,barrelEnd.position,barrelEnd.rotation) as Rigidbody; //INSTANTIATING THE FLARE PROJECTILE
			Physics.IgnoreCollision(bulletInstance.GetComponent<Collider>(), holder.GetComponent<Collider>());
			
			bulletInstance.velocity = holder.velocity + (barrelEnd.forward * bulletSpeed); //ADDING FORWARD VELOCITY TO THE FLARE PROJECTILE

			Destroy(Instantiate(muzzleParticles, barrelEnd.position,barrelEnd.rotation),3);	//INSTANTIATING THE GUN'S MUZZLE SPARKS	
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
