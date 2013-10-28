using UnityEngine;
using System.Collections;

public enum TileType : int {Wall,Item,Open,Taken,Door};


public class Tile {
	
	public static int spacing = 10;
	public static int yTileHeight = 0;
	
	private int type;
	private bool visible;
	private bool highlighted;
	
	public Tile(int x, int z){
		type = (int)TileType.Open;
		visible = false; highlighted = false;
	}
	
	public int Type{
		get{ return type; }	
	}
	
	public bool Visible{
		get{ return visible;}
		set{ visible = value;}
	}
	
	public void GiveWall(){
		type = (int)TileType.Wall;	
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
	
	public void GiveDoor(){
		type = (int)TileType.Door;	
	}
	
	public void Highlight(){
		if(visible)
			highlighted=true;
		else {
			Debug.Log("Error: Tile.Highlight()");
		}
	}
	
	public bool isOpen(){
		return (type==(int)TileType.Open);	
	}
	
	
}
