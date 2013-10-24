using UnityEngine;
using System.Collections;

public class Spy : Player{

	public static int spacing = 10;
	
	public Spy(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.alive=true;
	}
	
}
