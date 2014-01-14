using UnityEngine;
using System.Collections;

public class Guy : Player {
	
	public enum GuyGear{shotgun,rifle}; //shotgun in appropriate (close) range is crit, rifle must be used appropriate distance (far) for crit
	
	public Guy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
		this.health=5;
	}

	public string GearEquipped(){
		if(this.gearEquipped == (int)GuyGear.shotgun) return "Shotgun";
		else return "Rifle";
	}

	public void Equip(int gear){
		if(gear!=(int)GuyGear.shotgun && gear!=(int)GuyGear.rifle) 
			Debug.Log("Guy.Equip() ERROR");
		else 
			this.gearEquipped = gear;	
	}

	public void DealDamage(Spy enemy){
		SpendPoint();
		switch(gearEquipped){
		case (int)GuyGear.shotgun:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				enemy.Die ();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //critical damage
				enemy.TakeDamage(4);
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistnace){ //small dmg
				enemy.TakeDamage(3);
			}else{
				enemy.TakeDamage(1);
			}
			break;
		case (int)GuyGear.rifle:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				enemy.Die ();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //little dmg
				enemy.TakeDamage(1);
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistnace){ //small dmg
				enemy.TakeDamage(3);
			}else{
				enemy.TakeDamage(4);
			}
			break;
		default:
			Debug.Log ("Error: Guy.cs public void DealDamage");
			break;
		}
	}
}
