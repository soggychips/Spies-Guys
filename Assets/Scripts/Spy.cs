using UnityEngine;
using System.Collections;

public class Spy : Player{
	
	public enum SpyGear: int{empGun,shockRifle};
	
	public Spy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
	}
	
	public void Equip(int gear){
		if(gear!=(int)SpyGear.empGun && gear!=(int)SpyGear.shockRifle) 
			Debug.Log("Spy.Equip() ERROR");
		else 
			this.gearEquipped = gear;	
	}
	
}
