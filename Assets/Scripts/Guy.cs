﻿using UnityEngine;
using System.Collections;

public class Guy : Player {
	
	public static int totalMovementPoints = 10;
	
	public Guy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
		this.movesLeft = totalMovementPoints;
	}
	
	
}
