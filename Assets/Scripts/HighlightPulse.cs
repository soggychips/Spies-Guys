using UnityEngine;
using System.Collections;

public class HighlightPulse : MonoBehaviour {
	
	private float lowAlpha;
	private float highAlpha;
	private bool increasing;
	
	
	void Awake(){
		lowAlpha=.1f;
		highAlpha=1.0f;
		Color color = transform.renderer.material.color;
		color.a = highAlpha;
		transform.renderer.material.color = color;
		increasing=false;
	}
	
	// Update is called once per frame
	void Update () {
		Color color =  transform.renderer.material.color;
		if(increasing){
			color.a += .7f*Time.deltaTime;
			transform.renderer.material.color = color;
			if(transform.renderer.material.color.a >= highAlpha) increasing = false;
		}else{
			color.a -= .7f*Time.deltaTime;
			transform.renderer.material.color = color;
			if(transform.renderer.material.color.a <= lowAlpha) increasing = true;
		}
	}
}

