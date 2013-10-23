using UnityEngine;
using System.Collections;

public class TurnState {
	
	public enum States{Neutral, CharSelected, MoveBegin, MoveAnimate, ActionBegin, ActionAnimate, Confirmation};
	
	private int currentState;
	
	public int CurrentState{
		get{return currentState; }
		set{currentState = value; }
	}
	
	public TurnState(){
		this.CurrentState = (int) States.Neutral;
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
	
	public void BeginAction(){
		CurrentState = (int) States.ActionBegin;	
	}
	
	public void AnimateAction(){
		CurrentState = (int) States.ActionAnimate;	
	}
	
	public void ConfirmOrCancel(){
		CurrentState = (int) States.Confirmation;	
	}
	
	public void Neutralize(){
		CurrentState = (int) States.Neutral;	
	}
	
}
