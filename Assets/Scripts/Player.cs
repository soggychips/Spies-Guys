using UnityEngine;
using System.Collections;

public class Player {
	
	public static int totalMovementPoints = 3;
	public static int yPlayerHeight = 0;
	public static int sneakDistance = 4;
	public static int sprintDistnace = 2; //in addition to sneakDistance
	
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
	
	/*public bool SpendPoints(int points){
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
	}*/
	public bool HasPoint(){
		return(MovesLeft>=1);
	}

	public void SpendPoint(){
		if(MovesLeft<=0) Debug.Log ("Player.SpendPoint() ERROR");
		MovesLeft--;
	}

	public void GivePoint(){
		MovesLeft++;
	}
	
	public void ResetPoints(){
		movesLeft=totalMovementPoints;	
	}
	
	public void FreeMove(int x, int z){
		tileLocation = new Vector2(x,z);	
	}
	
	public void Move(int x, int z, int dist){
		SpendPoint();
		tileLocation = new Vector2(x,z);
	}
	
	public void MoveBack(int x, int z){
		GivePoint();
		tileLocation = new Vector2(x,z);
	}
	
	
	public void Die(){
		alive=false;
	}
	
}
