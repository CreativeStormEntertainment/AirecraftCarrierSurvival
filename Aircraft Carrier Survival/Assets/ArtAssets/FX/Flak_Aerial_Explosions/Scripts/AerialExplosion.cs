using UnityEngine;
using System.Collections;

public class AerialExplosion : MonoBehaviour {
	
	public ParticleSystem ExplodeVideoParticles;
	public  ParticleSystem SparkTrailsParticles;
	public  ParticleSystem SparkParticles;
	public  AudioSource ExplodeAudio;

	void  Update (){

		if (Input.GetButtonDown("Fire1")) //check to see if the left mouse was pushed.
		{ 
			// Stop any previous explosions
			ExplodeVideoParticles.Clear();
			SparkParticles.Clear();
			SparkTrailsParticles.Clear();
			Explosion();      
		}

	}



	void  Explosion (){

		ExplodeVideoParticles.Play();
		SparkParticles.Play();
		SparkTrailsParticles.Play();
		ExplodeAudio.Play();


	}


}