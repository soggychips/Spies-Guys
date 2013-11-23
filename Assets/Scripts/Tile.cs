using UnityEngine;
using System.Collections;

public enum TileType : int {Wall,Item,Open,Taken,Door_Closed,Door_Open,Data};
public enum WallTypes : int {SE_Corner, SW_Corner, NW_Corner, NE_Corner,
							Horizontal_Mid, Vertical_Mid, W_Horizontal_End, E_Horizontal_End, S_Vertical_End, N_Vertical_End};

public class Tile {
	
	public static int spacing = 10;
	public static int yTileHeight = 0;
	
	private int type;
	private int wallType;
	private bool visible;
	private bool highlighted;
	private Vector3 realWorldLocation;
	private bool stored; private int storedType;

	private int depth;
	private Vector2 pathPredecessor;
	
	public Tile(int x, int z){
		type = (int)TileType.Open;
		visible = false; highlighted = false;
		depth = 0;
		realWorldLocation = new Vector3(x*spacing,yTileHeight,z*spacing);
		stored=false; storedType=(int)TileType.Open;
	}
	
	public int Type{
		get{ return type; }	
	}

	public int WallType{
		get{return wallType;}
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
	
	public bool Highlight{
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
	
	public void GiveItem(){
		type = (int)TileType.Item;	
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
	

	public bool isOpen(){
		return (type==(int)TileType.Open);	
	}

	public bool isBlocked(){
		return (type==(int)TileType.Taken || type==(int)TileType.Wall || type==(int)TileType.Door_Closed || type==(int)TileType.Data);
	}

	public bool hasClosedDoor(){
		return (type==(int)TileType.Door_Closed);
	}

	public bool hasWall(){
		return (type==(int)TileType.Wall);
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
