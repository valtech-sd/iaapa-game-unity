using System;
using UnityEngine;
using UnityEngine.UI;
public class Timer : MonoBehaviour {
	[SerializeField] bool timerIsRunning = false;
	private const float defaultTotalTime = 60;
	private float timeRemaining = defaultTotalTime;
	private Text timerText;

	private void Start() {
		// Starts the timer automatically
		//timerIsRunning = true;

		// Get the sibling Text component in the Game Object this script is attached to
		timerText = GetComponent<Text>();
	}
	void Update() {
		if (timerIsRunning) {
			if (timeRemaining > 0) {
				timeRemaining -= Time.deltaTime;
				DisplayTime(timeRemaining);
			}
			else {
				Debug.Log("Time has run out!");
				timeRemaining = 0;
				timerIsRunning = false;
			}
		}
	}
	void DisplayTime(float timeToDisplay) {
		//Debug.Log("timeToDisplay: " + timeToDisplay);
		timeToDisplay += 1;
		float minutes = Mathf.FloorToInt(timeToDisplay / 60);
		float seconds = Mathf.FloorToInt(timeToDisplay % 60);
		Debug.Log("minutes: " + minutes);
		Debug.Log("seconds: " + seconds);

		timerText.text = String.Format("{0:00}:{1:00}", minutes, seconds);
	}

	public void startTimer(float totalTime = defaultTotalTime) {
		timeRemaining = totalTime;
		timerIsRunning = true;
	}

	public void resetTimerAndWait(float totalTime = defaultTotalTime) {
		timeRemaining = totalTime;
		DisplayTime(timeRemaining);
		timerIsRunning = false;
	}
}
