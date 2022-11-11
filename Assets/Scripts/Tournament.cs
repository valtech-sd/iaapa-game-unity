using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using TMPro;

public class Tournament : MonoBehaviour {
	public string rank1Name {
		get => names[0].GetComponent<TextMeshProUGUI>().text;
		set => SetName(0, value);
	}
	public string rank2Name {
		get => names[1].GetComponent<TextMeshProUGUI>().text;
		set => SetName(1, value);
	}
	public string rank3Name {
		get => names[2].GetComponent<TextMeshProUGUI>().text;
		set => SetName(2, value);
	}
	public string rank4Name {
		get => names[3].GetComponent<TextMeshProUGUI>().text;
		set => SetName(3, value);
	}
	public string rank5Name {
		get => names[4].GetComponent<TextMeshProUGUI>().text;
		set => SetName(4, value);
	}
	public string rank6Name {
		get => names[5].GetComponent<TextMeshProUGUI>().text;
		set => SetName(5, value);
	}
	
	public string rank1Score {
		get => scores[0].GetComponent<TextMeshProUGUI>().text;
		set => SetScore(0, value);
	}
	public string rank2Score {
		get => scores[1].GetComponent<TextMeshProUGUI>().text;
		set => SetScore(1, value);
	}
	public string rank3Score {
		get => scores[2].GetComponent<TextMeshProUGUI>().text;
		set => SetScore(2, value);
	}
	public string rank4Score {
		get => scores[3].GetComponent<TextMeshProUGUI>().text;
		set => SetScore(3, value);
	}
	public string rank5Score {
		get => scores[4].GetComponent<TextMeshProUGUI>().text;
		set => SetScore(4, value);
	}
	public string rank6Score {
		get => scores[5].GetComponent<TextMeshProUGUI>().text;
		set => SetScore(5, value);
	}
	
	[Header("Winner Text Objects")]
	[SerializeField] private GameObject[] names = new GameObject[6];
	[SerializeField] private GameObject[] scores = new GameObject[6];

	// Reference to the Tournament class instance in the game
	private ShowControl _showControl;

	// Start is called before the first frame update
	void Start() {
		_showControl = FindObjectOfType<ShowControl>();

		Debug.Log("Initializing displayed winners list");
		SetTexts();
	}

	private void SetTexts() {
		//Debug.Log("Number of winners: " + _showControl.winners.Length);
		for (var i = 0; i < _showControl.winners.Length; i++) {
			var winner = _showControl.winners[i];
			foreach (Transform child in winner.transform) {
				var text = child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
				if (child.name == "Name") {
					SetName(i, text);
				}
				else if (child.name == "Score") {
					SetScore(i, text);
				}
			}
		}
	}

	private void SetName(int index, string text) {
		Debug.Log($"Setting winner name at index {index} to {text}");
		//Debug.Log("Number of name slots: " + names.Length);
		names[index].GetComponent<TextMeshProUGUI>().text = text;
	}
	private void SetScore(int index, string text) {
		Debug.Log($"Setting winner score at index {index} to {text}");
		//Debug.Log("Number of score slots: " + scores.Length);
		scores[index].GetComponent<TextMeshProUGUI>().text = text;
	}


}
