using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

	public float animationSpeed = 80;
	public bool confirmationButtonFlag;

	private bool playerHasConfirmedOrCancelled;
	private int confirmOrCancel; //set as 1 for confirm, 2 for cancel
	private float confirmationButtonLeft;
	private Vector2 confirmationButtonBoxLocation, confirmButtonLocation, cancelButtonLocation;

	void Awake(){
		confirmOrCancel = 0;
		ResetConfirmationButtonVariables();
		confirmationButtonBoxLocation = new Vector2(Screen.width-100,Screen.height/2-120);
		confirmButtonLocation = new Vector2(Screen.width-95,Screen.height/2-115);
		cancelButtonLocation = new Vector2(Screen.width-95,Screen.height/2);
	}

	void OnGUI(){
		if(confirmationButtonFlag){
			ConfirmationButtons();
		}
	} 


	public void ConfirmationButtons(){ //called in OnGUI
		if(confirmationButtonLeft<=confirmationButtonBoxLocation.x){ 								//box is in its final place. allow clicking by showing buttons
			GUI.Box (new Rect(confirmationButtonBoxLocation.x,confirmationButtonBoxLocation.y,100,240),"");
			if(GUI.Button(new Rect(confirmButtonLocation.x,confirmButtonLocation.y,90,110), "Confirm")) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 1;
			}
			if(GUI.Button (new Rect(cancelButtonLocation.x,cancelButtonLocation.y,90,110),"Cancel")) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 2;
			}
		}else{																						//Animate box to location
			GUI.Box (new Rect(confirmationButtonLeft,confirmationButtonBoxLocation.y,100,240),"");  //Display texture containing the button images pasted on (unclickable)
			confirmationButtonLeft -= Time.deltaTime * animationSpeed;
		}
	}

	private void ResetConfirmationButtonVariables ()
	{
		playerHasConfirmedOrCancelled = false;
		confirmationButtonLeft = Screen.width;
		confirmationButtonFlag = false;
	}

	public int ConfirmationButtonPlayerInput(){ //called by TouchHandler.cs 
		if(playerHasConfirmedOrCancelled){
			ResetConfirmationButtonVariables();
			return confirmOrCancel;
		}
		return 0;
	}

	public void FlagConfirmationButtons(){ //called by TouchHandler.cs
		confirmationButtonFlag = true;
	}


}
