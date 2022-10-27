using System;
using System.Collections;
using System.IO;
using UnityEngine;


public static class Utilities {
	public static string GetTextFromFile(string filePath) {
		StreamReader inp_stm = new StreamReader(filePath);

		string output = "";
		while (!inp_stm.EndOfStream) {
			// Do Something with the input.
			output += inp_stm.ReadLine();
		}

		inp_stm.Close();

		//Debug.Log("Read from file: " + output);
		return output;
	}

	private static bool isCoroutineExecuting = false;
	public static IEnumerator ExecuteAfterTime(float delayInSeconds, Action task) {
		if (isCoroutineExecuting) yield break;
		isCoroutineExecuting = true;
		yield return new WaitForSeconds(delayInSeconds);
		task();
		isCoroutineExecuting = false;
	}
}
