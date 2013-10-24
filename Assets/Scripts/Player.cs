using UnityEngine;
using System.Collections;

public class Player {
	
	public static int totalMovementPoints = 10;
	public static int yPlayerHeight = 0;
	
	protected bool alive;
	protected bool selected;
	protected Vector2 tileLocation;
	protected Vector3 realWorldLocation;
	protected int movesLeft { get; set; }
	
	public bool Alive{
		get{return alive;}	
	}
	
	public bool Selected{
		get{return selected;}
		set{selected = value;}
	}
	
	public Vector2 TileLocation{
		get{return tileLocation;}	
	}
	
	public Player(){}
	
	public Player(int x, int z){
		this.tileLocation = new Vector2(x,z);
		this.realWorldLocation = new Vector3(Tile.spacing*x,yPlayerHeight,Tile.spacing*z);
		this.movesLeft = totalMovementPoints;
		alive=true;
		selected=false;
	}
	
	public bool SpendPoints(int points){
		if(points>movesLeft){
			Debug.Log("points>movesLeft :: public void SpendPoints in Player.cs");
			return false;
		}else{
			movesLeft = movesLeft-points;
			return true;
		}
	}
	
	public void Move(int x, int z){
		tileLocation = new Vector2(x,z);
	}
	
	public void Die(){
		alive=false;
	}
	
}
