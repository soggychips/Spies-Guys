using UnityEngine;
using System.Collections;

public class testScan : MonoBehaviour {
	int i;
	// Use this for initialization
	void Start () {
		i=0;
	}
	
	// Update is called once per frame
	void Update () {
		if(i==0){
			ScanningLineTest();
			i++;
		}
	}
	
	public void ScanningLineTest(){
		Vector2 start = new Vector2(3,8);
		Vector2 end = new Vector2(0,0);
		Vector2 vect = end-start;
		double norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
		Debug.Log ("norm = "+norm);
		Vector2 unitVect = new Vector2((float)(vect.x/norm),(float)(vect.y/norm));
		Debug.Log ("vector = "+ vect.ToString());
		Debug.Log ("Unit vector = ["+unitVect.x+","+unitVect.y+"]");
		Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
		while(roundedLocation!=end){
			start+=unitVect;
			roundedLocation = new Vector2((int)start.x,(int)start.y);
			Debug.Log ("location = ["+start.x+","+start.y+"]");
			Debug.Log ("rounded location = ["+(int)start.x+","+(int)start.y+"]");
		}
	}
}
