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
		while(start!=end){
			start+=unitVect;
			Debug.Log ("location = ["+start.x+","+start.y+"]");
			Debug.Log ("rounded location = ["+(int)start.x+","+(int)start.y+"]");
			if(start.x<0 || start.y<0){ 
				Debug.Log("ScanningLineTest error");
				return;
			}
		}
	}
}
