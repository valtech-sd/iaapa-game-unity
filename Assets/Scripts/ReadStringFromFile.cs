using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class ReadStringFromFile : MonoBehaviour
{
    void ReadTextFile()
    {
        string fullPath = string.Format("{0}/{1}", Application.streamingAssetsPath, "config.txt");
        StreamReader inputStream = new StreamReader(fullPath);

        while (!inputStream.EndOfStream)
        {
            string inputLine = inputStream.ReadLine();
            // Do Something with the input.
            Debug.LogError(inputLine); // Need to do LogError to get the console to show in dev mode... Unity is the best, this line for example only, remove it
        }

        inputStream.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        ReadTextFile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
