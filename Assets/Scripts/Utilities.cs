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
}
