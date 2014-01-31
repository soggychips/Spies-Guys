using UnityEngine;
using System.Collections;

public enum TileType : int {Wall,Lightswitch,Open,Taken,Door_Closed,Door_Open,Door_Locked,Data,Extraction,LockdownSwitch};
public enum WallTypes : int {SE_Corner, SW_Corner, NW_Corner, NE_Corner,
							Horizontal_Mid, Vertical_Mid, W_Horizontal_End, E_Horizontal_End, S_Vertical_End, N_Vertical_End,W_T,E_T,S_T,N_T};
public enum DoorFacings : int {NS,EW};

public class Tile {
	
	public static int spacing = 10;
	public static int yTileHeight = 0;
	
	private int type;
	private int wallType;
	private int doorFacing_Direction;
	private bool visible;
	private bool lit;
	private bool highlighted;
	private bool sprintedTo;
	//private Vector3 realWorldLocation;
	private bool stored; private int storedType;

	private int room; //-1 for walls
	private int depth;
	private Vector2 pathPredecessor;
	
	public Tile(int x, int z){
		type = (int)TileType.Open;
		visible = false; highlighted = false;
		depth = 0;
		//realWorldLocation = new Vector3(x*spacing,yTileHeight,z*spacing);
		stored=false; storedType=(int)TileType.Open;
		sprintedTo=false;
		room = -10;
		lit = false;
	}
	

	public int Type{
		get{ return type; }	
	}

	public int StoredType{
		get{return storedType;}
	}

	public bool Lit{
		get{return lit;}
		set{lit = value;}
	}

	public int Room{
		get{return room;}
		set{room = value;}
	}

	public bool SprintedTo{
		get{return sprintedTo;}
		set{sprintedTo=value;}
	}

	public int WallType{
		get{return wallType;}
	}

	public int DoorFacing{
		get{return doorFacing_Direction;}
	}
	
	public bool Visible{
		get{ return visible;}
		set{ visible = value;}
	}

	public bool Stored{
		get{return stored;}
	}

	public int Depth{
		get{return depth;}
		set{depth=value;}
	}
	
	public bool Highlighted{
		get{return highlighted;}
		set{highlighted=value;}
	}
	
	public Vector2 PathPredecessor{
		get{return pathPredecessor;}
		set{pathPredecessor=value;}
	}
	
	public void GiveWall(){
		type = (int)TileType.Wall;	
	}

	public void GiveWall(int typeOfWall){
		type = (int)TileType.Wall;
		wallType = typeOfWall;
	}

	public void GiveData(){
		type = (int)TileType.Data;
	}
	
	public void GiveLightswitch(){
		type = (int)TileType.Lightswitch;	
	}

	public void GiveLockdownSwitch(){
		type = (int)TileType.LockdownSwitch;
	}
    
	public void GiveDoor(int doorFacing){
		type = (int)TileType.Door_Closed;
		doorFacing_Direction = doorFacing;
	}
	
	public void Take(){
		type = (int)TileType.Taken;	
	}
	
	public void Open(){
		type = (int)TileType.Open;	
	}
	
	public void CloseDoor(){
		type = (int)TileType.Door_Closed;	
	}

	public void OpenDoor(){
		type = (int)TileType.Door_Open;
	}

	public void LockDoor(){
		type = (int)TileType.Door_Locked;
	}

	public void MakeExtractionPoint(){
		type = (int)TileType.Extraction;
	}

	public void Highlight(){
		highlighted=true;
	}
	

	public bool isOpen(){
		return (type==(int)TileType.Open);	
	}

	public bool isBlocked(){ 
		return (type==(int)TileType.Taken || type==(int)TileType.Wall || type==(int)TileType.Door_Closed || type==(int)TileType.Door_Locked || type==(int)TileType.Data);
	}

	public bool isTaken(){
		return (type==(int)TileType.Taken);
	}


	public bool hasClosedDoor(){
		return (type==(int)TileType.Door_Closed);
	}

	public bool hasData(){
		return (type==(int)TileType.Data);
	}

	public bool hasLightswitch(){
		return (type==(int)TileType.Lightswitch);
	}

	public bool hasLockdownSwitch(){
		return (type==(int)TileType.LockdownSwitch);
	}

	public bool hasLockedDoor(){
		return (type==(int)TileType.Door_Locked);
	}

	public bool hasOpenDoor(){
		return (type==(int)TileType.Door_Open);
	}

	public bool hasWall(){
		return (type==(int)TileType.Wall);
	}

	public bool hasDoor(){
		return(type==(int)TileType.Door_Closed || type==(int)TileType.Door_Open || type==(int)TileType.Door_Locked);
	}

	public void StoreType(){
		stored=true;
		storedType=type;
	}

	public void StoreTypeAs(int newType){
		stored=true;
		storedType=newType;
	}

	public void LoadStoredType(){
		type=storedType;
		stored=false;
	}
}
