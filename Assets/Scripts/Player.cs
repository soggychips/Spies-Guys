using UnityEngine;
using System.Collections;

public class Player {
	
	public static int totalMovementPoints = 3;
	public static int yPlayerHeight = 0;
	
	protected bool alive;
	protected bool selected;
	protected Vector2 tileLocation;
	protected Vector3 realWorldLocation;
	protected int movesLeft;
	protected int gearEquipped;
	protected int health;
	
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
		gearEquipped=0;
		health = 10;
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
	
	public void GivePoints(int points){
		movesLeft += points;	
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
	
	public void FreeMove(int x, int z){
		tileLocation = new Vector2(x,z);	
	}
	
	public void Move(int x, int z, int dist){
		bool lastMove = !SpendPoints(dist);
		if(lastMove) SpendAllPoints();
		tileLocation = new Vector2(x,z);
	}
	
	public void MoveBack(int x, int z, int dist){
		if(movesLeft==0) GivePoints(1);
		else GivePoints(dist);
		tileLocation = new Vector2(x,z);
	}
	
	
	public void Die(){
		alive=false;
	}
	
}
