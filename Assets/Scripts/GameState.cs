using UnityEngine;
using System.Collections;

public class GameState {

	public enum States {Menu, MatchCreated, P1, P2, GameOver};
	
	
	
	private int currentState;
	
	public int CurrentState{
		get{ return currentState; }
		set{ currentState = value; }
	}
	
	public GameState(){
		this.CurrentState = (int)States.Menu;	
	}
	
	public void CreateMatch(){
		this.CurrentState = (int) States.MatchCreated;	
	}
	
	public void GiveControlToPlayer1(){
		this.CurrentState = (int) States.P1;
	}
	
	public void SwitchPlayers(){
		if(this.CurrentState ==(int)States.P1)
			this.CurrentState = (int)States.P2;
		else {
			this.CurrentState = (int)States.P1;
		}
	}
	
	public void EndGame(){
		this.CurrentState = (int)States.GameOver;	
	}
	
}
