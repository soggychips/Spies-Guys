﻿using UnityEngine;
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

	public void PredictDamage(Guy enemy){
		int predictedDamage = 0;
		Debug.Log ("Targetted enemy's health before attacking: "+enemy.Health);
		switch(gearEquipped){
		case (int)SpyGear.shockRifle:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				Debug.Log ("The enemy will be strangled to death! He is heavy, you will be too tired to move!");
				predictedDamage = enemy.Health;
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //critical damage
				Debug.Log ("A shoulder shot, and the enemy will be shocked!");
				predictedDamage = 2;
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistnace){ //small dmg
				Debug.Log ("A shot to the forearm, and the enemy will be shocked!");
				predictedDamage = 1;
			}else{
				Debug.Log ("You're too far to cause damage, but the enemy will be shocked!");
			}
			break;
		case (int)SpyGear.empGun:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				Debug.Log ("The enemy will be strangled to death! He is heavy, you will be too tired to move!");
				predictedDamage = enemy.Health;
			}else{
				Debug.Log ("You will do no damage, you have an EMP gun equipped!");
			}
			break;
		default:
			Debug.Log ("Error: Guy.cs public void DealDamage");
			break;
		}
		Debug.Log ("Enemy health after attack: "+(enemy.Health-predictedDamage));
	}

	public void DealDamage(Guy enemy){
		SpendPoint();
		switch(gearEquipped){
		case (int)SpyGear.shockRifle:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				Debug.Log ("The enemy is strangled to death! He is heavy, you're too tired to move!");
				enemy.Die ();
				while(HasPoint()) SpendPoint();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance){ //critical damage
				Debug.Log ("A shoulder shot, and the enemy is shocked!");
				enemy.TakeDamage(2);
				enemy.Shock();
			}else if(Vector2.Distance(this.tileLocation,enemy.TileLocation)<=sneakDistance+sprintDistnace){ //small dmg
				Debug.Log ("A shot to the forearm, and the enemy is shocked!");
				enemy.TakeDamage(1);
				enemy.Shock();
			}else{
				Debug.Log ("You're too far to cause damage, but the enemy is shocked!");
				enemy.Shock ();
			}
			break;
		case (int)SpyGear.empGun:
			if(Vector2.Distance(this.tileLocation,enemy.TileLocation)==1){
				Debug.Log ("The enemy is strangled to death! He is heavy, you're too tired to move!");
				enemy.Die ();
				while(HasPoint()) SpendPoint();
			}
			break;
		default:
			Debug.Log ("Error: Guy.cs public void DealDamage");
			break;
		}
	}
	
}
