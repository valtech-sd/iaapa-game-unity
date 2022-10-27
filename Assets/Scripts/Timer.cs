using System;
using UnityEngine;
using UnityEngine.UI;
public class Timer : MonoBehaviour {
	private bool _showTimer = false;
	public bool showTimer {
		get => _showTimer;
		set {
			_showTimer = value;
			Debug.Log("showTimer has been set to " + value);
		}
	}

	private const float k_DefaultTotalTimeInSeconds = 60;
	private float _timeRemaining = k_DefaultTotalTimeInSeconds;
	private Text _timerText;

	private void Start() {
		// Get the sibling Text component in the Game Object this script is attached to
		_timerText = GetComponent<Text>();
	}
	void Update() {
		if (showTimer) {
			if (_timeRemaining > 0) {
				_timeRemaining -= Time.deltaTime;
				DisplayTime(_timeRemaining);
			}
			else {
				Debug.Log("Time is up!");
				_timeRemaining = 0;
			}
		}
		else {
			_timerText.text = "";
		}
	}
	void DisplayTime(float timeToDisplay) {
		//Debug.Log("timeToDisplay: " + timeToDisplay);
		timeToDisplay += 1;
		float minutes = Mathf.FloorToInt(timeToDisplay / 60);
		float seconds = Mathf.FloorToInt(timeToDisplay % 60);
		//Debug.Log("minutes: " + minutes);
		//Debug.Log("seconds: " + seconds);

		_timerText.text = String.Format("{0:00}:{1:00}", minutes, seconds);
	}

	public void StartTimer(float totalTime = k_DefaultTotalTimeInSeconds) {
		Debug.Log("StartTimer: " + totalTime);
		_timeRemaining = totalTime;
		showTimer = true;
	}

	public void ResetTimerAndWait(float totalTime = k_DefaultTotalTimeInSeconds) {
		_timeRemaining = totalTime;
		DisplayTime(_timeRemaining);
		showTimer = false;
	}
}
