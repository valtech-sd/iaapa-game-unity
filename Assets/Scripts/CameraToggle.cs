using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraToggle : MonoBehaviour
{
    public GameObject cam1;
    public GameObject cam2;
    public GameObject cam3;

    PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    void Start()
    {
        playerControls.Enable(); 
    }

    // Update is called once per frame
    void Update()
    {
        bool isKey1Pressed = playerControls.Player.SwitchCam1.ReadValue<float>() > 0.1f;
        bool isKey2Pressed = playerControls.Player.SwitchCam2.ReadValue<float>() > 0.1f;
        bool isKey3Pressed = playerControls.Player.SwitchCam3.ReadValue<float>() > 0.1f;

        if (isKey1Pressed)
        {
            cam1.SetActive(true);
            cam2.SetActive(false);
            cam3.SetActive(false);
        }

        if (isKey2Pressed)
        {
            cam1.SetActive(false);
            cam2.SetActive(true);
            cam3.SetActive(false);
        }

        if (isKey3Pressed)
        {
            cam1.SetActive(false);
            cam2.SetActive(false);
            cam3.SetActive(true);
        }






    }

}