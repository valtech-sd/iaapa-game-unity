using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WinnerScreen : MonoBehaviour
{
    public TMP_InputField playerNameInput;

    private TextMeshProUGUI playerNameText;

    // Start is called before the first frame update
    void Start()
    {
        playerNameText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        playerNameText.text = playerNameInput.text;
    }
}
