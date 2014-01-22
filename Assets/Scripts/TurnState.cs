using UnityEngine;
using System.Collections;

public class TurnState {
	
	public enum States: int{Begin, Neutral, CharSelected, MoveBegin, MoveAnimate, MoveConfirm, ActionBegin, ActionAnimate, ActionConfirm, End};
	public enum ActionTypes: int{Door,Data,Lightswitch,Attack,Shock};
	
	private int currentState;
	private int actionType;
	
	public int CurrentState{
		get{return currentState; }
		set{currentState = value; }
	}

	public int ActionType{
		get{return actionType;}
		set{actionType = value;}
	}
	
	public TurnState(){
		this.CurrentState = (int) States.Neutral; //TODO: change to begin
	}
	
	public void BeginTurn(){
		CurrentState = (int)States.Begin;	
	}
	
	public void SelectCharacter(){
		CurrentState = (int) States.CharSelected;	
	}
	
	public void BeginMovement(){
		CurrentState = (int) States.MoveBegin;	
	}
	
	public void AnimateMovement(){
		CurrentState = (int) States.MoveAnimate;	
	}
	
	public void BeginAction(int type){
		CurrentState = (int) States.ActionBegin;
		actionType = type;
	}
	
	public void AnimateAction(){
		CurrentState = (int) States.ActionAnimate;
	}
	
	public void EndMovement(){
		CurrentState = (int) States.MoveConfirm;	
	}
	
	public void EndAction(){
		CurrentState = (int) States.ActionConfirm;	
	}
	
	public void EndTurn(){
		CurrentState = (int)States.End;
	}
	
	public void Neutralize(){
		CurrentState = (int) States.Neutral;	
	}
	
}
