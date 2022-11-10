using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using TMPro;

public class WinnerScreen : MonoBehaviour {
	[Header("Winner Text Objects")]
	[SerializeField] private GameObject[] names;
	[SerializeField] private GameObject[] scores;
	public string rank1Name { get; set; }

	// Reference to the ShowControl class instance in the game
	private ShowControl _showControl;

	private bool isUpdating = false;

	// Start is called before the first frame update
	void Start() {
		_showControl = FindObjectOfType<ShowControl>();
		//Debug.Log("Number of winners: " + _showControl.winners.Length);
		SetTexts();
	}

	// Update is called once per frame
	void Update() {
		// TODO: replace with a change handler from Text Input component
		if (!isUpdating) SetTexts();
	}

	private void SetTexts() {
		isUpdating = true;

		for (var i = 0; i < _showControl.winners.Length; i++) {
			var winner = _showControl.winners[i];
			foreach (Transform child in winner.transform) {
				var text = child.gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
				if (child.name == "Name") {
					Debug.Log("Found name: " + text);
					names[i].GetComponent<TextMeshProUGUI>().text = text;
				}
				else if (child.name == "Score") {
					Debug.Log("Found score: " + text);
					scores[i].GetComponent<TextMeshProUGUI>().text = text;
				}
			}
		}

		isUpdating = false;
	}

	/*public void SetName(string text, int index) {
		//names[index].GetComponent<TextMeshProUGUI>().text =
	}*/
}
