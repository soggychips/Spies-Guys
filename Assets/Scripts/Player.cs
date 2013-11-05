using UnityEngine;
using System.Collections;

public class Player {
	
	public static int totalMovementPoints = 10;
	public static int yPlayerHeight = 0;
	
	protected bool alive;
	protected bool selected;
	protected Vector2 tileLocation;
	protected Vector3 realWorldLocation;
	protected int movesLeft;
	
	public int MovesLeft{
		get{return movesLeft;}
		set{movesLeft=value;}
	}
	
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
	
	public bool SpendAllPoints(){
		if(movesLeft>0){ 
			movesLeft=0;
			return true;
		}
		return false;
	}
	
	public void ResetPoints(){
		movesLeft=totalMovementPoints;	
	}
	
	public void Move(int x, int z, int dist){
		SpendPoints(dist);
		tileLocation = new Vector2(x,z);
		realWorldLocation = tileLocation*Tile.spacing;
	}
	
	public bool CanMove(int dist){
		if(dist>movesLeft) return false;
		return true;
	}
	
	public void Die(){
		alive=false;
	}
	
}
