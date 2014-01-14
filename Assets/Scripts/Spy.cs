using UnityEngine;
using System.Collections;

public class Spy : Player{
	
	public enum SpyGear: int{empGun,shockRifle};
	
	public Spy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
		this.health=5;
	}

	public string GearEquipped(){
		if(this.gearEquipped==(int)SpyGear.empGun) return "EMP Gun";
		else return "Shock Rifle";
	}
	
	public void Equip(int gear){
		if(gear!=(int)SpyGear.empGun && gear!=(int)SpyGear.shockRifle) 
			Debug.Log("Spy.Equip() ERROR");
		else 
			this.gearEquipped = gear;	
	}

	public void DealDamage(Guy enemy){
		SpendPoint();
		switch(gearEquipped){
		case (int)SpyGear.shockRifle:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				enemy.Die ();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //critical damage
				enemy.TakeDamage(2);
				enemy.Shock();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistnace){ //small dmg
				enemy.TakeDamage(1);
				enemy.Shock();
			}else{
				enemy.Shock ();
			}
			break;
		case (int)SpyGear.empGun:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				enemy.Die ();
			}
			break;
		default:
			Debug.Log ("Error: Guy.cs public void DealDamage");
			break;
		}
	}
	
}
