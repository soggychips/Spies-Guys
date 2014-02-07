using UnityEngine;
using System.Collections;

public class Guy : Player {
	
	public enum GuyGear{shotgun,rifle}; //shotgun in appropriate (close) range is crit, rifle must be used appropriate distance (far) for crit
	
	public Guy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
		this.health=5;
		this.currentSprintDistance=Player.sprintDistance;
	}

	public int GearEquipped{
		get{return this.gearEquipped;}
	}

	public string GearEquipped_String(){
		if(this.gearEquipped == (int)GuyGear.shotgun) return "Shotgun";
		else return "Rifle";
	}

	public void Equip(int gear){
		if(gear!=(int)GuyGear.shotgun && gear!=(int)GuyGear.rifle) 
			Debug.Log("Guy.Equip() ERROR");
		else 
			this.gearEquipped = gear;

		switch(this.gearEquipped){
		case (int)GuyGear.rifle:
			this.gearType = (int)GearTypes.weapon;
			break;
		case (int)GuyGear.shotgun:
			this.gearType = (int)GearTypes.weapon;
			break;
		}
	}

	public void PredictDamage(Spy enemy){
		int predictedDamage = 0;
		Debug.Log ("Enemy's health before attacking: "+enemy.Health);
		switch(gearEquipped){
		case (int)GuyGear.shotgun:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=2){
				Debug.Log ("The enemy is strangled to death!");
				predictedDamage = enemy.Health;
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //critical damage
				Debug.Log ("The enemy takes a shotgun blast to the gut!");
				predictedDamage = 4;
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistance){ //small dmg
				Debug.Log ("The enemy is hit in the shoulder by the shotgun shrapnel!");
				predictedDamage = 3;
			}else{
				Debug.Log ("You're too far, the shotgun only scraped the enemy.");
				predictedDamage = 1;
			}
			break;
		case (int)GuyGear.rifle:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				Debug.Log ("The enemy is strangled to death!");
				predictedDamage = enemy.Health;
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //little dmg
				Debug.Log ("The enemy is too close to aim at! Your hipfire hit their fingers at best.");
				predictedDamage = 1;
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistance){ //small dmg
				Debug.Log ("The enemy is hit in the leg!");
				predictedDamage = 3;
			}else{
				Debug.Log ("The enemy is hit in the gut!");
				predictedDamage = 4;
			}
			break;
		default:
			Debug.Log ("Error: Guy.cs public void DealDamage");
			break;
		}
		Debug.Log("Enemy's health after attack: "+(enemy.Health-predictedDamage));
	}

	public void DealDamage(Spy enemy){
		SpendPoint();
		switch(gearEquipped){
		case (int)GuyGear.shotgun:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=2){
				Debug.Log ("The enemy is strangled to death!");
				enemy.Die ();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //critical damage
				Debug.Log ("The enemy takes a shotgun blast to the gut!");
				enemy.TakeDamage(4);
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistance){ //small dmg
				Debug.Log ("The enemy is hit in the shoulder by the shotgun shrapnel!");
				enemy.TakeDamage(3);
			}else{
				enemy.TakeDamage(1);
				Debug.Log ("You're too far, the shotgun only scraped the enemy's side.");
			}
			break;
		case (int)GuyGear.rifle:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				Debug.Log ("The enemy is strangled to death!");
				enemy.Die ();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //little dmg
				Debug.Log ("The enemy is too close to aim at! Your hipfire hit their fingers at best.");
				enemy.TakeDamage(1);
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistance){ //small dmg
				Debug.Log ("The enemy is hit in the leg!");
				enemy.TakeDamage(3);
			}else{
				enemy.TakeDamage(4);
				Debug.Log ("The enemy is hit in the gut!");
			}
			break;
		default:
			Debug.Log ("Error: Guy.cs public void DealDamage");
			break;
		}
	}
}
