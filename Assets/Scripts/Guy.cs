using UnityEngine;
using System.Collections;

public class Guy : Player {
	
	public enum GuyGear{shotgun,rifle};
	
	public Guy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
	}
	
	public void Equip(int gear){
		if(gear!=(int)GuyGear.shotgun && gear!=(int)GuyGear.rifle) 
			Debug.Log("Guy.Equip() ERROR");
		else 
			this.gearEquipped = gear;	
	}
}
