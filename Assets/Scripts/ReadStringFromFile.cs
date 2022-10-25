using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class ReadStringFromFile : MonoBehaviour
{
    void readTextFile()
    {
        string full_path = string.Format("{0}/{1}", Application.streamingAssetsPath, "config.txt");
        StreamReader inp_stm = new StreamReader(full_path);

        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            // Do Something with the input. 
            Debug.LogError(inp_ln); // Need to do LogError to get the console to show in dev mode... Unity is the best, this line for example only, remove it
        }

        inp_stm.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        readTextFile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
