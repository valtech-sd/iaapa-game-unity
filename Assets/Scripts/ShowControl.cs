using System;
using System.Collections;
using System.Collections.Generic;
using Doozy.Runtime.UIManager.Containers;
using UnityEngine;

public class ShowControl : MonoBehaviour {

	[Serializable]
	public enum PartyState {
		Introductions,
		Qualifiers,
		PreTournament,
		Tournament,
		PostTournament,
		FreePlays
	}

	[Serializable]
	public enum GameState {
		Idle,
		OnBoarding,
		Ready,
		Running,
		Ended
	}


	[SerializeField] PartyState partyState;

	public PartyState CurrentPartyState {
		get => partyState;
		private set {
			partyState = value;
			Debug.Log($"Party State set to {value}");
			UpdateSecondaryView();
		}
	}


	[SerializeField] GameState gameState;
	public GameState CurrentGameState {
		get =>  gameState;
		private set {
			gameState = value;
			Debug.Log($"Game State set to {value}");
			UpdateMainView();
		}
	}

	/**
	 * Sets the current party state.
	 * NOTE: Using string because an enum argument is not supported for Unity button click handlers.
	 * https://stackoverflow.com/questions/50131837/unity-is-not-showing-public-methods-in-the-event-field
	 */
	public void SetPartyState(string partyStateString) {
		Enum.TryParse(partyStateString, out PartyState partyState);
		CurrentPartyState = partyState;
	}

	/**
	 * Sets the current game state.
	 * NOTE: Using string because an enum argument is not supported for Unity button click handlers.
	 * https://stackoverflow.com/questions/50131837/unity-is-not-showing-public-methods-in-the-event-field
	 */
	public void SetGameState(string gameStateString) {
		Enum.TryParse(gameStateString, out GameState gameState);
		CurrentGameState = gameState;
	}

	private GameObject mainCanvas;
	private GameObject secondaryCanvas;
	private void Awake() {
		mainCanvas = GameObject.FindWithTag("MainCanvas");
		secondaryCanvas = GameObject.FindWithTag("SecondaryCanvas");
	}

	private void Start() {

	}

	private void UpdateMainView() {
		// NOTE: Assumes View Name contains the GameState name
		ShowView(mainCanvas, CurrentGameState.ToString());
	}
	private void UpdateSecondaryView() {
		// NOTE: Some views are shared by different PartyStates.
		switch (CurrentPartyState) {
			case PartyState.Qualifiers:
				ShowView(secondaryCanvas,"HighScoresAll");
				break;
			case PartyState.PreTournament:
				ShowView(secondaryCanvas,"HighScoresTop30");
				break;
			case PartyState.Tournament:
				ShowView(secondaryCanvas,"Tournament");
				break;
			case PartyState.PostTournament:
				ShowView(secondaryCanvas,"Tournament");
				break;
			case PartyState.FreePlays:
				ShowView(secondaryCanvas,"Tournament");
				break;
			default:
				ShowView(secondaryCanvas,"Idle");
				break;
		}
	}

	private void ShowView(GameObject canvas, string viewToShow) {
		//Debug.Log(canvas.transform.childCount);

		foreach (Transform child in canvas.transform) {
			//Debug.Log(child.name);
			//UIView uiView = child.GetComponent<UIView>();
			if (child.name.Contains(viewToShow)) {
				//uiView.Show();
				child.gameObject.SetActive(true);
			}
			else {
				//uiView.Hide();
				child.gameObject.SetActive(false);
			}
		}
	}
}
